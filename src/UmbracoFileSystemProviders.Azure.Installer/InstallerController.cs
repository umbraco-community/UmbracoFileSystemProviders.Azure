// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerController.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
[assembly: InternalsVisibleTo("Our.Umbraco.FileSystemProviders.Azure.Tests")]
namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Xml;

    using Microsoft.WindowsAzure.Storage;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using umbraco.cms.businesslogic.packager.standardPackageActions;

    using Enums;
    using Models;

    [PluginController("FileSystemProviders")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        private const string ProviderType = "Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure";
        private static readonly string ImageProcessorWebAssemblyPath = HostingEnvironment.MapPath("~/bin/ImageProcessor.Web.dll");
        private static readonly Version ImageProcessorWebMinRequiredVersion = new Version("4.3.2.0");

        private readonly string fileSystemProvidersConfigInstallXdtPath = HostingEnvironment.MapPath("~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/FileSystemProviders.config.install.xdt");
        private readonly string fileSystemProvidersConfigPath = HostingEnvironment.MapPath("~/Config/FileSystemProviders.config");

        // /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        public IEnumerable<Parameter> GetParameters()
        {
            return GetParametersFromXdt(this.fileSystemProvidersConfigInstallXdtPath, this.fileSystemProvidersConfigPath);
        }

        // /Umbraco/backoffice/FileSystemProviders/Installer/PostParameters
        [HttpPost]
        public InstallerStatus PostParameters(IEnumerable<Parameter> parameters)
        {
            var connection = parameters.SingleOrDefault(k => k.Key == "connectionString").Value;
            var containerName = parameters.SingleOrDefault(k => k.Key == "containerName").Value;            

            if (!TestAzureCredentials(connection, containerName))
            {
                return InstallerStatus.ConnectionError;
            }

            if (SaveParametersToXdt(this.fileSystemProvidersConfigInstallXdtPath, parameters))
            {
                if (!ExecuteFileSystemConfigTransform() || !ExecuteWebConfigTransform())
                {
                    return InstallerStatus.SaveConfigError;
                }

                if (!CheckImageProcessorWebCompatibleVersion(ImageProcessorWebMinRequiredVersion))
                {
                    return InstallerStatus.ImageProcessorWebCompatibility;
                }

                return InstallerStatus.Ok;
            }

            return InstallerStatus.SaveXdtError;
        }

        internal static bool SaveParametersToXdt(string xdtPath, IEnumerable<Parameter> newParameters)
        {
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Inset a Parameter element with Xdt remove so that updated values get saved (for upgrades), we don't want this for NuGet packages which is why it's here instead
            var nsMgr = new XmlNamespaceManager(document.NameTable);
            var strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            var providerElement = document.SelectSingleNode(string.Format("//Provider[@type = '{0}']", ProviderType));
            var parametersElement = providerElement.SelectSingleNode("./Parameters");
            var parameterRemoveElement = document.CreateNode("element", "Parameters", null);
            var tranformAttr = document.CreateAttribute("Transform", strNamespace);
            tranformAttr.Value = "Remove";

            parameterRemoveElement.Attributes.Append(tranformAttr);
            providerElement.InsertBefore(parameterRemoveElement, parametersElement);

            var parameters = document.SelectNodes(string.Format("//Provider[@type = '{0}']/Parameters/add", ProviderType));

            if (parameters == null)
            {
                return false;
            }

            foreach (XmlElement parameter in parameters)
            {
                var key = parameter.GetAttribute("key");
                var value = parameter.GetAttribute("value");

                var newParameter = newParameters.FirstOrDefault(x => x.Key == key);
                if (newParameter != null)
                {
                    var newValue = newParameter.Value;

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
                var message = "Error saving XDT Parameters: " + e.Message;
                LogHelper.Error(typeof(InstallerController), message, e);
            }

            return result;
        }

        internal static IEnumerable<Parameter> GetParametersFromXdt(string xdtPath, string configPath)
        {
            // For package upgrades check for configured values in existing FileSystemProviders.config and merge with the Parameters from the XDT file (there could be new ones)
            var xdtParameters = GetParametersFromXml(xdtPath);
            var currentConfigParameters = GetParametersFromXml(configPath);

            foreach (var parameter in xdtParameters)
            {
                if (currentConfigParameters.Select(k => k.Key).Contains(parameter.Key))
                {
                    var currentParameter = currentConfigParameters.SingleOrDefault(k => k.Key == parameter.Key);
                    if (currentParameter != null)
                    {
                        parameter.Value = currentParameter.Value;
                    }
                }
            }

            return xdtParameters;
        }

        internal static IEnumerable<Parameter> GetParametersFromXml(string xmlPath)
        {
            var settings = new List<Parameter>();

            var document = XmlHelper.OpenAsXmlDocument(xmlPath);

            var parameters = document.SelectNodes(string.Format("//Provider[@type = '{0}']/Parameters/add", ProviderType));

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

        private static bool ExecuteFileSystemConfigTransform()
        {
            var transFormConfigAction = helper.parseStringToXmlNode("<Action runat=\"install\" undo=\"true\" alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" file=\"~/Config/FileSystemProviders.config\" xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/FileSystemProviders.config\">" +
         "</Action>").FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool ExecuteWebConfigTransform()
        {
            var transFormConfigAction = helper.parseStringToXmlNode("<Action runat=\"install\" undo=\"true\" alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" file=\"~/web.config\" xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/web.config\">" +
         "</Action>").FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("UmbracoFileSystemProviders.Azure", transFormConfigAction);
        }

        private static bool TestAzureCredentials(string connectionString, string containerName)
        {
            var useEmulator = ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey] != null
                               && ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey]
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                var cloudStorageAccount = useEmulator ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.Parse(connectionString);

                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var blobContainer = cloudBlobClient.GetContainerReference(containerName);

                // this should fully check that the connection works, the result is not relevant
                var blobExists = blobContainer.Exists();
            }
            catch (Exception e)
            {
                LogHelper.Error<InstallerController>(string.Format("Error validating Azure storage connection: {0}", e.Message), e);
                return false;
            }

            return true;
        }

        private static bool CheckImageProcessorWebCompatibleVersion(Version imageProcessorWebMinRequiredVersion)
        {
            if (!File.Exists(ImageProcessorWebAssemblyPath))
            {
                return false;
            }

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(ImageProcessorWebAssemblyPath);
            var currentImageProcessorWebVersionInfo = new Version(fileVersionInfo.ProductVersion);
            return currentImageProcessorWebVersionInfo >= imageProcessorWebMinRequiredVersion;
        }
    }
}
