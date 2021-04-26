// <copyright file="InstallerController.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Xml;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Composing;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using global::Umbraco.Core.Xml;

    using Enums;
    using Models;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;

    /// <summary>
    /// The installer controller for managing installer logic.
    /// </summary>
    [PluginController("FileSystemProviders")]

    public class InstallerController : UmbracoAuthorizedApiController
    {
        private static readonly string ImageProcessorConfigPath = HostingEnvironment.MapPath(Constants.ImageProcessor.ConfigPath);

        private static readonly string ImageProcessorSecurityConfigPath = HostingEnvironment.MapPath($"{Constants.ImageProcessor.ConfigPath}{Constants.ImageProcessor.SecurityConfigFile}");
        private static readonly string ImageProcessorSecurityDefaultConfigPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.ImageProcessor.SecurityConfigFile}");
        private static readonly string ImageProcessorSecurityInstallXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.ImageProcessor.SecurityConfigFile}.install.xdt");

        private readonly string webConfigXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.WebConfigFile}.install.xdt");
        private readonly string webConfigPath = HostingEnvironment.MapPath($"/{Constants.WebConfigFile}");


        private readonly string mediaWebConfigXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.MediaWebConfigXdtFile}.install.xdt");

        public InstallerController()
        {

        }

        /// <summary>
        /// Gets the parameters from the XDT transform file.
        /// </summary>
        /// <remarks>
        /// Route: /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        /// </remarks>
        /// <returns>The <see cref="IEnumerable{Parameter}"/></returns>
        public IEnumerable<Parameter> GetParameters()
        {
            return GetParametersFromXdt(this.webConfigXdtPath, this.webConfigPath);
        }

        /// <summary>
        /// Allows the posting of the parameter collection to the controller.
        /// </summary>
        /// <remarks>
        /// Route: /Umbraco/backoffice/FileSystemProviders/Installer/PostParameters
        /// </remarks>
        /// <param name="parameters">The parameters</param>
        /// <returns>The <see cref="InstallerStatus"/></returns>
        [HttpPost]
        public InstallerStatus PostParameters(IEnumerable<Parameter> parameters)
        {
            IList<Parameter> newParameters = parameters as IList<Parameter> ?? parameters.ToList();

            // TODO: Handle possible NullReferenceException.
            string connection = newParameters.SingleOrDefault(k => k.Key == "ConnectionString").Value;
            string containerName = newParameters.SingleOrDefault(k => k.Key == "ContainerName").Value;
            bool useDefaultRoute = bool.Parse(newParameters.SingleOrDefault(k => k.Key == "UseDefaultRoute").Value);
            bool usePrivateContainer = bool.Parse(newParameters.SingleOrDefault(k => k.Key == "UsePrivateContainer").Value);
            string rootUrl = newParameters.SingleOrDefault(k => k.Key == "RootUrl").Value;

            var blobContainerPublicAccessType = usePrivateContainer ? PublicAccessType.None : PublicAccessType.Blob;

            if (!TestAzureCredentials(connection, containerName, blobContainerPublicAccessType))
            {
                return InstallerStatus.ConnectionError;
            }

            string routePrefix = useDefaultRoute ? Azure.Constants.DefaultMediaRoute : containerName;

            if (SaveParametersToWebConfigXdt(this.webConfigXdtPath, newParameters) && SaveContainerNameToWebConfigXdt(this.webConfigXdtPath, routePrefix))
            {
                if (!ExecuteWebConfigTransform() || !ExecuteMediaWebConfigTransform())
                {
                    return InstallerStatus.SaveConfigError;
                }

                // Merge in storage url to ImageProcessor security.config xdt
                SaveBlobPathToImageProcessorSecurityXdt(ImageProcessorSecurityInstallXdtPath, rootUrl, routePrefix, containerName);

                // Transform ImageProcessor security.config
                if (!ExecuteImageProcessorSecurityConfigTransform())
                {
                    return InstallerStatus.ImageProcessorWebConfigError;

                }          

                return InstallerStatus.Ok;
            }

            return InstallerStatus.SaveXdtError;
        }

        /// <summary>
        /// Saves the parameter collection to the XDT transform.
        /// </summary>
        /// <param name="xdtPath">The file path.</param>
        /// <param name="newParameters">The parameters</param>
        /// <returns><c>true</c> if the save is sucessful.</returns>
        internal static bool SaveParametersToWebConfigXdt(string xdtPath, IList<Parameter> newParameters)
        {
            foreach (var parameter in newParameters)
            {
                parameter.Key = $"{Azure.Constants.Configuration.ConfigrationSettingPrefix}.{parameter.Key}:media";
            }

            bool result = false;
            XmlDocument document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Inset a Parameter element with Xdt remove so that updated values get saved (for upgrades), we don't want this for NuGet packages which is why it's here instead
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
            string strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            XmlNode providerElement = document.SelectSingleNode($"//appSettings");
            if (providerElement == null)
            {
                return false;
            }

            var removeNodesExist = false;
            var removeNodes = document.SelectNodes($"//appSettings/add[@xdt:Transform='Remove']", nsMgr);
            if (removeNodes != null && removeNodes.Count > 0)
            {
                removeNodesExist = true;
            }

            XmlNodeList settings = document.SelectNodes($"//appSettings/add");
            if (settings == null)
            {
                return false;
            }

            foreach (XmlElement setting in settings)
            {
                string key = setting.GetAttribute("key");
                string value = setting.GetAttribute("value");

                Parameter newParameter = newParameters.FirstOrDefault(x => x.Key == key);
                if (newParameter != null)
                {
                    string newValue = newParameter.Value;

                    if (!value.Equals(newValue))
                    {
                        setting.SetAttribute("value", newValue);
                    }
                }

                if (!removeNodesExist)
                {
                    XmlNode settingRemoveElement = document.CreateNode("element", "add", null);

                    XmlAttribute keyAttr = document.CreateAttribute("key");
                    keyAttr.Value = setting.GetAttribute("key");
                    settingRemoveElement.Attributes.Append(keyAttr);

                    XmlAttribute locatorAttr = document.CreateAttribute("Locator", strNamespace);
                    locatorAttr.Value = "Match(key)";
                    settingRemoveElement.Attributes.Append(locatorAttr);

                    XmlAttribute transformAttr = document.CreateAttribute("Transform", strNamespace);
                    transformAttr.Value = "Remove";
                    settingRemoveElement.Attributes.Append(transformAttr);

                    providerElement.InsertBefore(settingRemoveElement, setting);
                }
            }

            try
            {
                document.Save(xdtPath);

                // No errors so the result is true
                result = true;
            }
            catch (Exception e)
            {
                // Log error message
                Current.Logger.Error<InstallerController>(e, "Error saving XDT Parameters");
            }

            return result;
        }

        /// <summary>
        /// Saves the container name to the XDT transform.
        /// </summary>
        /// <param name="xdtPath">The file path.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns><c>true</c> if the save is sucessful.</returns>
        internal static bool SaveContainerNameToWebConfigXdt(string xdtPath, string containerName)
        {
            bool result = false;
            XmlDocument document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Inset a Parameter element with Xdt remove so that updated values get saved (for upgrades), we don't want this for NuGet packages which is why it's here instead
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
            string strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            XmlNode locationElement = document.SelectSingleNode("//location");
            if (locationElement?.Attributes != null)
            {
                locationElement.Attributes["path"].Value = containerName;
            }

            try
            {
                document.Save(xdtPath);

                // No errors so the result is true
                result = true;
            }
            catch (Exception e)
            {
                // Log error message
                Current.Logger.Error<InstallerController>(e, "Error saving XDT Parameters");
            }

            return result;
        }

        /// <summary>
        /// Saves the blob path to the ImageProcessor configuration file.
        /// </summary>
        /// <param name="xdtPath">The file path.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns><c>true</c> if the save is sucessful.</returns>
        internal static bool SaveBlobPathToImageProcessorSecurityXdt(string xdtPath, string rootUrl, string prefix, string containerName)
        {
            bool result = false;
            XmlDocument document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Set the prefix attribute on both the Remove and InsertIfMissing actions
            XmlNodeList rawServices = document.SelectNodes($"//services/service[@name = '{Constants.ImageProcessor.SecurityServiceName}' and @type = '{Constants.ImageProcessor.SecurityServiceType}']");
            if (rawServices == null)
            {
                return false;
            }

            foreach (XmlElement service in rawServices)
            {
                service.SetAttribute("prefix", $"{prefix}/");
            }

            // Set the settings within the InsertIfMissing action
            XmlNodeList rawSettings = document.SelectNodes($"//services/service[@prefix = '{prefix}/' and @name = '{Constants.ImageProcessor.SecurityServiceName}' and @type = '{Constants.ImageProcessor.SecurityServiceType}']/settings/setting");
            if (rawSettings == null)
            {
                return false;
            }

            foreach (XmlElement setting in from XmlElement setting in rawSettings select setting)
            {
                if (setting.GetAttribute("key").InvariantEquals("Host"))
                {
                    setting.SetAttribute("value", $"{rootUrl}");
                }

                if (setting.GetAttribute("key").InvariantEquals("Container"))
                {
                    setting.SetAttribute("value", $"{containerName}");
                }
            }

            try
            {
                document.Save(xdtPath);

                // No errors so the result is true
                result = true;
            }
            catch (Exception e)
            {
                // Log error message
                Current.Logger.Error<InstallerController>(e, "Error saving XDT Parameters");
            }

            return result;
        }

        /// <summary>
        /// Gets the parameter collection from the XDT transform.
        /// </summary>
        /// <param name="xdtPath">The file path.</param>
        /// <param name="configPath">The configuration file path.</param>
        /// <returns>The <see cref="IEnumerable{Parameter}"/>.</returns>
        internal static IEnumerable<Parameter> GetParametersFromXdt(string xdtPath, string configPath)
        {
            // For package upgrades check for configured values in existing Web.Config and merge with the settings from the XDT file (there could be new ones)
            List<Parameter> xdtParameters = GetAppSettingsFromConfig(xdtPath, true).ToList();
            List<Parameter> currentConfigParameters = GetAppSettingsFromConfig(configPath, false).ToList();

            foreach (Parameter parameter in xdtParameters)
            {
                if (currentConfigParameters.Select(k => k.Key).Contains(parameter.Key))
                {
                    Parameter currentParameter = currentConfigParameters.SingleOrDefault(k => k.Key == parameter.Key);
                    if (currentParameter != null)
                    {
                        parameter.Value = currentParameter.Value;
                    }
                }
            }

            foreach (var setting in xdtParameters)
            {
                setting.Key = setting.Key.TrimEnd(":media");
                setting.Key = setting.Key.TrimStart($"{Azure.Constants.Configuration.ConfigrationSettingPrefix}.");
            }

            return xdtParameters;
        }

        /// <summary>
        /// Gets the parameter collection from the XML file.
        /// </summary>
        /// <param name="xmlPath">The file path</param>
        /// <returns>The <see cref="IEnumerable{Parameter}"/>.</returns>
        internal static IEnumerable<Parameter> GetAppSettingsFromConfig(string xmlPath, bool isXdt)
        {
            List<Parameter> settings = new List<Parameter>();

            XmlDocument document = XmlHelper.OpenAsXmlDocument(xmlPath);

            XmlNodeList parameters;
            if (isXdt)
            {
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
                string strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
                nsMgr.AddNamespace("xdt", strNamespace);
                parameters = document.SelectNodes($"//appSettings/add[@xdt:Transform='InsertIfMissing']", nsMgr);
            }
            else
            {
                parameters = document.SelectNodes($"//appSettings/add");
            }

            if (parameters == null)
            {
                return settings;
            }

            foreach (XmlElement parameter in parameters)
            {
                if (parameter.GetAttribute("key").StartsWith(Azure.Constants.Configuration.ConfigrationSettingPrefix) && parameter.GetAttribute("key").EndsWith($":{Constants.MediaProviderPostFix}"))
                {
                    settings.Add(new Parameter
                    {
                        Key = parameter.GetAttribute("key"),
                        Value = parameter.GetAttribute("value")
                    });
                }
            }

            return settings;
        }

        private static bool ExecuteWebConfigTransform()
        {
            XmlNode transFormConfigAction =
                ParseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"true\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/web.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/web.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", ToXElement(transFormConfigAction));
            return true;
        }

        private static bool ExecuteMediaWebConfigTransform()
        {
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Media/web.config")))
            {
                XmlNode transFormConfigAction =
                    ParseStringToXmlNode("<Action runat=\"install\" "
                                + "undo=\"true\" "
                                + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                + "file=\"~/Media/web.config\" "
                                + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/media-web.config\">"
                                + "</Action>").FirstChild;

                PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
                return transformConfig.Execute("UmbracoFileSystemProviders.Azure", ToXElement(transFormConfigAction));
                return true;
            }

            return true;
        }

        private static bool ExecuteImageProcessorSecurityConfigTransform()
        {
            // Ensure that security.config exists in ~/Config/Imageprocessor/
            if (!File.Exists(ImageProcessorSecurityConfigPath))
            {
                if (!Directory.Exists(ImageProcessorConfigPath))
                {
                    Directory.CreateDirectory(ImageProcessorConfigPath);
                }

                File.Copy(ImageProcessorSecurityDefaultConfigPath, ImageProcessorSecurityConfigPath);
            }

            XmlNode transFormConfigAction =
                ParseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"false\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/config/imageprocessor/security.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/security.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", ToXElement(transFormConfigAction));
            return true;
        }

        private static bool TestAzureCredentials(string connectionString, string containerName, PublicAccessType accessType)
        {
            bool useEmulator = ConfigurationHelper.GetAppSetting(Azure.Constants.Configuration.UseStorageEmulatorKey) != null
                               && ConfigurationHelper.GetAppSetting(Azure.Constants.Configuration.UseStorageEmulatorKey)
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                var cloudStorageAccount = connectionString;

                // This should fully check that the connection works.
                var azf = AzureFileSystem.GetInstance(containerName, "", connectionString, "", "", "");
                var testContainer = azf.CreateContainer( containerName, accessType);

                if (testContainer.Exists())
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Current.Logger.Error<InstallerController>(e, "Error validating Azure storage connection");
                return false;
            }
        }

        private static XmlNode ParseStringToXmlNode(string value)
        {
            var xmlDocument = new XmlDocument();
            var xmlNode = AddTextNode(xmlDocument, "error", "");

            try
            {
                xmlDocument.LoadXml(value);
                return xmlDocument.SelectSingleNode(".");
            }
            catch
            {
                return xmlNode;
            }
        }
        private static XmlNode AddTextNode(XmlDocument xmlDocument, string name, string value)
        {
            var node = xmlDocument.CreateNode(XmlNodeType.Element, name, "");
            node.AppendChild(xmlDocument.CreateTextNode(value));
            return node;
        }
        private static XElement ToXElement(XmlNode xmlElement)
        {
            using (var nodeReader = new XmlNodeReader(xmlElement))
            {
                nodeReader.MoveToContent();
                return XElement.Load(nodeReader);
            }
        }
    }
}
