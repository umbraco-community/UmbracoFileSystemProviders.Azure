using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

[assembly: InternalsVisibleTo("Our.Umbraco.FileSystemProviders.Azure.Tests")]
namespace Our.Umbraco.FileSystemProviders.Azure.Umbraco.Installer
{
    [PluginController("FileSystemProviders")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        // /Umbraco/backoffice/FileSystemProviders/Installer/GetParameters
        public IEnumerable<Parameter> GetParameters()
        {
            var path = HostingEnvironment.MapPath("~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/FileSystemProviders.config.install.xdt");
            return GetParametersFromXdt(path);
        }

        [HttpPost]
        public bool PostParameters(IEnumerable<Parameter> parameters)
        {
            var path = HostingEnvironment.MapPath("~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/FileSystemProviders.config.install.xdt");
            return SaveParametersToXdt(path, parameters);
           
            // Execute XDT Transform
            // Do something with values for upgrades?
        }

        internal static bool SaveParametersToXdt(string configPath, IEnumerable<Parameter> newParameters)
        {
            var modified = false;
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(configPath);

            var parameters = document.SelectNodes("//Provider[@type = 'Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure']/Parameters/add");

            if (parameters == null) return false;

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
            var settings = new List<Parameter>();

            var document = XmlHelper.OpenAsXmlDocument(configPath);

            var parameters = document.SelectNodes("//Provider[@type = 'Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure']/Parameters/add");

            if (parameters == null) return settings;

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
    }

    public class Parameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
