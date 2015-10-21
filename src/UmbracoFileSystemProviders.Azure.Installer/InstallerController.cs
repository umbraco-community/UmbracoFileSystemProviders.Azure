// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerController.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved. Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Runtime.CompilerServices;

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

        // /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        public IEnumerable<Parameter> GetParameters()
        {
            return GetParametersFromXdt(_fileSystemProvidersConfigInstallXdtPath);
        }

        // /Umbraco/backoffice/FileSystemProviders/Installer/PostParameters
        [HttpPost]
        public bool PostParameters(IEnumerable<Parameter> parameters)
        {
            if (SaveParametersToXdt(_fileSystemProvidersConfigInstallXdtPath, parameters))
            {
                if (ExecuteFileSystemConfigTransform() && ExecuteWebConfigTransform())
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool SaveParametersToXdt(string configPath, IEnumerable<Parameter> newParameters)
        {
            var modified = false;
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(configPath);

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
                    document.Save(configPath);
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

        internal static IEnumerable<Parameter> GetParametersFromXdt(string configPath)
        {
            // TODO check existing FileSystemProviders.config to see if this is an package upgrade and transfer the values

            var settings = new List<Parameter>();

            var document = XmlHelper.OpenAsXmlDocument(configPath);

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
    }
}
