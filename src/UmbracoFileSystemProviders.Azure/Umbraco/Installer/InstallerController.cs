using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
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
        public Dictionary<string, string> GetParameters()
        {
            return GetParametersFromXdt();
        }

        internal static Dictionary<string, string> GetParametersFromXdt()
        {
            var settings = new Dictionary<string, string>();

            var filename = ("FileSystemProviders.config.install.xdt");

            var document = xmlHelper.OpenAsXmlDocument(filename);

            var parameters = document.SelectNodes("//Provider[@type = 'Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure']/Parameters/add");

            foreach (XmlElement parameter in parameters)
            {
                settings.Add(parameter.GetAttribute("key"), parameter.GetAttribute("value"));
            }

            return settings;
        }
    }
}
