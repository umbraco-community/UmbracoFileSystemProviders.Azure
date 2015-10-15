using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ClientDependency.Core;
using umbraco;
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
            // Save back to XDT
            // Execute XDT Transform
            // Do something with values for upgrades?

            return true;
        }

        internal static IEnumerable<Parameter> GetParametersFromXdt(string configPath)
        {
            var settings = new List<Parameter>();

            var document = xmlHelper.OpenAsXmlDocument(configPath);

            var parameters = document.SelectNodes("//Provider[@type = 'Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure']/Parameters/add");

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
