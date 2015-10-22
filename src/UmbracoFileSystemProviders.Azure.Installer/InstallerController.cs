// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerController.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved. Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Configuration;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Our.Umbraco.FileSystemProviders.Azure.Installer.Enums;

[assembly: InternalsVisibleTo("Our.Umbraco.FileSystemProviders.Azure.Tests")]
namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Xml;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using umbraco.cms.businesslogic.packager.standardPackageActions;

    using Models;

    [PluginController("FileSystemProviders")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        private const string ProviderType = "Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure";
        private readonly string _fileSystemProvidersConfigInstallXdtPath = HostingEnvironment.MapPath("~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/FileSystemProviders.config.install.xdt");
        private readonly string _fileSystemProvidersConfigPath = HostingEnvironment.MapPath("~/Config/FileSystemProviders.config");


        // /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        public IEnumerable<Parameter> GetParameters()
        {
            return GetParametersFromXdt(_fileSystemProvidersConfigInstallXdtPath, _fileSystemProvidersConfigPath);
        }

        // /Umbraco/backoffice/FileSystemProviders/Installer/PostParameters
        [HttpPost]
        public InstallerStatus PostParameters(IEnumerable<Parameter> parameters)
        {
            var connection = parameters.SingleOrDefault(k => k.Key == "connectionString").Value;
            var containerName = parameters.SingleOrDefault(k => k.Key == "containerName").Value;

            if (!this.TestAzureCredentials(connection, containerName))
            {
                return InstallerStatus.ConnectionError;
            }

            if (SaveParametersToXdt(_fileSystemProvidersConfigInstallXdtPath, parameters))
            {
                if (!ExecuteFileSystemConfigTransform() || !ExecuteWebConfigTransform())
                {
                    return InstallerStatus.SaveConfigError;
                }

                return InstallerStatus.Ok;
            }

            return InstallerStatus.SaveXdtError;
        }

        internal static bool SaveParametersToXdt(string xdtPath, IEnumerable<Parameter> newParameters)
        {
            var modified = false;
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(xdtPath);

            var parameters = document.SelectNodes(string.Format("//Provider[@type = '{0}']/Parameters/add", ProviderType));

            if (parameters == null)
            {
                return false;
            }

            foreach (XmlElement parameter in parameters)
            {
                var key = parameter.GetAttribute("key");
                var value = parameter.GetAttribute("value");
                var newValue = newParameters.FirstOrDefault(x => x.Key == key).Value;

                if (!value.Equals(newValue))
                {
                    parameter.SetAttribute("value", newValue);
                    modified = true;
                }
            }

            if (modified)
            {
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
            }
            else
            {
                // nothing to modify
                result = true;
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

        private bool TestAzureCredentials(string connectionString, string containerName)
        {
            var useEmulator = ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey] != null
                               && ConfigurationManager.AppSettings[Azure.Constants.Configuration.UseStorageEmulatorKey]
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                CloudStorageAccount cloudStorageAccount;
                if (useEmulator)
                {
                    cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                }
                else
                {
                    cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                }

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
    }
}
