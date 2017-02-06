// <copyright file="InstallerController.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Xml;
    using Enums;
    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Models;
    using umbraco.cms.businesslogic.packager.standardPackageActions;

    /// <summary>
    /// The installer controller for managing installer logic.
    /// </summary>
    [PluginController("FileSystemProviders")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        private static readonly string ImageProcessorWebAssemblyPath = HostingEnvironment.MapPath(Constants.ImageProcessor.WebAssemblyPath);
        private static readonly Version ImageProcessorWebMinRequiredVersion = new Version(Constants.ImageProcessor.WebMinRequiredVersion);

        private static readonly string ImageProcessorConfigPath = HostingEnvironment.MapPath(Constants.ImageProcessor.ConfigPath);

        private static readonly string ImageProcessorSecurityConfigPath = HostingEnvironment.MapPath($"{Constants.ImageProcessor.ConfigPath}{Constants.ImageProcessor.SecurityConfigFile}");
        private static readonly string ImageProcessorSecurityDefaultConfigPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.ImageProcessor.SecurityConfigFile}");
        private static readonly string ImageProcessorSecurityInstallXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.ImageProcessor.SecurityConfigFile}.install.xdt");

        private readonly string fileSystemProvidersConfigInstallXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.FileSystemProvidersConfigFile}.install.xdt");
        private readonly string fileSystemProvidersConfigPath = HostingEnvironment.MapPath($"{Constants.UmbracoConfigPath}{Constants.FileSystemProvidersConfigFile}");

        private readonly string webConfigXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.WebConfigFile}.install.xdt");

        private readonly string mediaWebConfigXdtPath = HostingEnvironment.MapPath($"{Constants.InstallerPath}{Constants.MediaWebConfigXdtFile}.install.xdt");

        /// <summary>
        /// Gets the parameters from the XDT transform file.
        /// </summary>
        /// <remarks>
        /// Route: /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        /// </remarks>
        /// <returns>The <see cref="IEnumerable{Parameter}"/></returns>
        public IEnumerable<Parameter> GetParameters()
        {
            return GetParametersFromXdt(this.fileSystemProvidersConfigInstallXdtPath, this.fileSystemProvidersConfigPath);
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

            // TODO: Handle possible NullReferenceExecption.
            string connection = newParameters.SingleOrDefault(k => k.Key == "connectionString").Value;
            string containerName = newParameters.SingleOrDefault(k => k.Key == "containerName").Value;
            bool useDefaultRoute = bool.Parse(newParameters.SingleOrDefault(k => k.Key == "useDefaultRoute").Value);
            bool usePrivateContainer = bool.Parse(newParameters.SingleOrDefault(k => k.Key == "usePrivateContainer").Value);
            string rootUrl = newParameters.SingleOrDefault(k => k.Key == "rootUrl").Value;

            if (!TestAzureCredentials(connection, containerName))
            {
                return InstallerStatus.ConnectionError;
            }

            string routePrefix = useDefaultRoute ? Azure.Constants.DefaultMediaRoute : containerName;

            if (SaveParametersToFileSystemProvidersXdt(this.fileSystemProvidersConfigInstallXdtPath, newParameters) && SaveContainerNameToWebConfigXdt(this.webConfigXdtPath, routePrefix))
            {
                if (!ExecuteFileSystemConfigTransform() || !ExecuteWebConfigTransform() || !ExecuteMediaWebConfigTransform())
                {
                    return InstallerStatus.SaveConfigError;
                }

                if (!CheckImageProcessorWebCompatibleVersion(ImageProcessorWebMinRequiredVersion))
                {
                    return InstallerStatus.ImageProcessorWebCompatibility;
                }
                else
                {
                    // Merge in storage url to ImageProcessor security.config xdt
                    SaveBlobPathToImageProcessorSecurityXdt(ImageProcessorSecurityInstallXdtPath, rootUrl, routePrefix, containerName);

                    // Transform ImageProcessor security.config
                    if (ExecuteImageProcessorSecurityConfigTransform())
                    {
                        if (!ExecuteImageProcessorWebConfigTransform())
                        {
                            return InstallerStatus.ImageProcessorWebConfigError;
                        }
                    }
                    else
                    {
                        return InstallerStatus.ImageProcessorWebConfigError;
                    }
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
        internal static bool SaveParametersToFileSystemProvidersXdt(string xdtPath, IList<Parameter> newParameters)
        {
            bool result = false;
            XmlDocument document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Inset a Parameter element with Xdt remove so that updated values get saved (for upgrades), we don't want this for NuGet packages which is why it's here instead
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
            string strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            XmlNode providerElement = document.SelectSingleNode($"//Provider[@type = '{Constants.ProviderType}']");
            if (providerElement == null)
            {
                return false;
            }

            XmlNode parametersElement = providerElement.SelectSingleNode("./Parameters");
            XmlNode parameterRemoveElement = document.CreateNode("element", "Parameters", null);
            if (parameterRemoveElement.Attributes == null)
            {
                return false;
            }

            XmlAttribute tranformAttr = document.CreateAttribute("Transform", strNamespace);
            tranformAttr.Value = "Remove";

            parameterRemoveElement.Attributes.Append(tranformAttr);
            providerElement.InsertBefore(parameterRemoveElement, parametersElement);

            XmlNodeList parameters = document.SelectNodes($"//Provider[@type = '{Constants.ProviderType}']/Parameters/add");
            if (parameters == null)
            {
                return false;
            }

            foreach (XmlElement parameter in parameters)
            {
                string key = parameter.GetAttribute("key");
                string value = parameter.GetAttribute("value");

                Parameter newParameter = newParameters.FirstOrDefault(x => x.Key == key);
                if (newParameter != null)
                {
                    string newValue = newParameter.Value;

                    if (!value.Equals(newValue))
                    {
                        parameter.SetAttribute("value", newValue);
                    }
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
                string message = "Error saving XDT Parameters: " + e.Message;
                LogHelper.Error(typeof(InstallerController), message, e);
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
                string message = "Error saving XDT Parameters: " + e.Message;
                LogHelper.Error(typeof(InstallerController), message, e);
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
                string message = "Error saving XDT Settings: " + e.Message;
                LogHelper.Error(typeof(InstallerController), message, e);
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
            // For package upgrades check for configured values in existing FileSystemProviders.config and merge with the Parameters from the XDT file (there could be new ones)
            List<Parameter> xdtParameters = GetParametersFromXml(xdtPath).ToList();
            List<Parameter> currentConfigParameters = GetParametersFromXml(configPath).ToList();

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

            return xdtParameters;
        }

        /// <summary>
        /// Gets the parameter collection from the XML file.
        /// </summary>
        /// <param name="xmlPath">The file path</param>
        /// <returns>The <see cref="IEnumerable{Parameter}"/>.</returns>
        internal static IEnumerable<Parameter> GetParametersFromXml(string xmlPath)
        {
            List<Parameter> settings = new List<Parameter>();
            XmlDocument document = XmlHelper.OpenAsXmlDocument(xmlPath);

            XmlNodeList parameters = document.SelectNodes($"//Provider[@type = '{Constants.ProviderType}']/Parameters/add");

            if (parameters == null)
            {
                return settings;
            }

            foreach (XmlElement parameter in parameters)
            {
                settings.Add(new Parameter
                {
                    Key = parameter.GetAttribute("key"),
                    Value = parameter.GetAttribute("value")
                });
            }

            return settings;
        }

        /// <summary>
        /// Executes the configuration transform.
        /// </summary>
        /// <returns>True if the transform is successful, otherwise false.</returns>
        private static bool ExecuteFileSystemConfigTransform()
        {
            XmlNode transFormConfigAction =
                helper.parseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"true\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/Config/FileSystemProviders.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/FileSystemProviders.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool ExecuteWebConfigTransform()
        {
            XmlNode transFormConfigAction =
                helper.parseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"true\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/web.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/web.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool ExecuteMediaWebConfigTransform()
        {
            if (File.Exists(HttpContext.Current.Server.MapPath("~/Media/web.config")))
            {
                XmlNode transFormConfigAction =
    helper.parseStringToXmlNode("<Action runat=\"install\" "
                                + "undo=\"true\" "
                                + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                + "file=\"~/Media/web.config\" "
                                + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/media-web.config\">"
                                + "</Action>").FirstChild;

                PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
                return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
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
                helper.parseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"false\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/config/imageprocessor/security.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/security.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool ExecuteImageProcessorWebConfigTransform()
        {
            XmlNode transFormConfigAction =
                helper.parseStringToXmlNode("<Action runat=\"install\" "
                                            + "undo=\"false\" "
                                            + "alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                                            + "file=\"~/web.config\" "
                                            + "xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/imageprocessor.web.config\">"
                                            + "</Action>").FirstChild;

            PackageActions.TransformConfig transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool TestAzureCredentials(string connectionString, string containerName)
        {
            bool useEmulator = ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey] != null
                               && ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey]
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                CloudStorageAccount cloudStorageAccount = useEmulator ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.Parse(connectionString);

                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = cloudBlobClient.GetContainerReference(containerName);

                // This should fully check that the connection works.
                blobContainer.CreateIfNotExists();
                return true;
            }
            catch (Exception e)
            {
                LogHelper.Error<InstallerController>($"Error validating Azure storage connection: {e.Message}", e);
                return false;
            }
        }

        private static bool CheckImageProcessorWebCompatibleVersion(Version imageProcessorWebMinRequiredVersion)
        {
            if (!File.Exists(ImageProcessorWebAssemblyPath))
            {
                return false;
            }

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(ImageProcessorWebAssemblyPath);
            Version currentImageProcessorWebVersionInfo = new Version(fileVersionInfo.ProductVersion);
            return currentImageProcessorWebVersionInfo >= imageProcessorWebMinRequiredVersion;
        }
    }
}
