namespace Our.Umbraco.FileSystemProviders.Azure.Umbraco.Installer
{
    using System.Web;

    using Microsoft.Web.XmlTransform;

    using umbraco.cms.businesslogic.packager.standardPackageActions;
    using umbraco.interfaces;

    public class PackageActions
    {
        public class TransformConfig : IPackageAction
        {
            public string Alias()
            {
                return "UmbracoFileSystemProviders.Azure.TransformConfig";
            }

            private bool Transform(string packageName, System.Xml.XmlNode xmlData, bool uninstall = false)
            {
                //The config file we want to modify
                var file = xmlData.Attributes.GetNamedItem("file").Value;

                string sourceDocFileName = VirtualPathUtility.ToAbsolute(file);

                //The xdt file used for tranformation 
                var fileEnd = "install.xdt";
                if (uninstall)
                {
                    fileEnd = string.Format("un{0}", fileEnd);
                }

                var xdtfile = string.Format("{0}.{1}", xmlData.Attributes.GetNamedItem("xdtfile").Value, fileEnd);
                string xdtFileName = VirtualPathUtility.ToAbsolute(xdtfile);

                // The translation at-hand
                using (var xmlDoc = new XmlTransformableDocument())
                {
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(HttpContext.Current.Server.MapPath(sourceDocFileName));

                    using (var xmlTrans = new XmlTransformation(HttpContext.Current.Server.MapPath(xdtFileName)))
                    {
                        if (xmlTrans.Apply(xmlDoc))
                        {
                            // If we made it here, sourceDoc now has transDoc's changes
                            // applied. So, we're going to save the final result off to
                            // destDoc.
                            xmlDoc.Save(HttpContext.Current.Server.MapPath(sourceDocFileName));
                        }
                    }
                }
                return true;
            }

            public bool Execute(string packageName, System.Xml.XmlNode xmlData)
            {
                return Transform(packageName, xmlData);
            }

            public System.Xml.XmlNode SampleXml()
            {
                string str = "<Action runat=\"install\" undo=\"true\" alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" file=\"~/web.config\" xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/web.config\">" +
                         "</Action>";
                return helper.parseStringToXmlNode(str);
            }

            public bool Undo(string packageName, System.Xml.XmlNode xmlData)
            {
                return Transform(packageName, xmlData, true);
            }
        }
    }
}
