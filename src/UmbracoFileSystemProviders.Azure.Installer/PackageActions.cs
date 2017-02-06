// <copyright file="PackageActions.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Web;
    using System.Xml;

    using global::Umbraco.Core.Logging;
    using Microsoft.Web.XmlTransform;
    using umbraco.cms.businesslogic.packager.standardPackageActions;
    using umbraco.interfaces;

    /// <summary>
    /// Handles installer package actions.
    /// </summary>
    public class PackageActions
    {
        /// <summary>
        /// The transform config package action.
        /// </summary>
        public class TransformConfig : IPackageAction
        {
            /// <inheritdoc/>
            public string Alias()
            {
                return "UmbracoFileSystemProviders.Azure.TransformConfig";
            }

            /// <inheritdoc/>
            public bool Execute(string packageName, XmlNode xmlData)
            {
                return this.Transform(packageName, xmlData);
            }

            /// <inheritdoc/>
            public XmlNode SampleXml()
            {
                string xml = "<Action runat=\"install\" "
                          + "undo=\"true\" alias=\"UmbracoFileSystemProviders.Azure.TransformConfig\" "
                          + "file=\"~/web.config\" xdtfile=\"~/app_plugins/UmbracoFileSystemProviders/Azure/install/web.config\">"
                          + "</Action>";
                return helper.parseStringToXmlNode(xml);
            }

            /// <inheritdoc/>
            public bool Undo(string packageName, XmlNode xmlData)
            {
                return this.Transform(packageName, xmlData, true);
            }

            /// <summary>
            /// Applied the transform.
            /// </summary>
            /// <param name="packageName">The package name.</param>
            /// <param name="xmlData">The XML data</param>
            /// <param name="uninstall">Whether to uninstall.</param>
            /// <returns><c>true</c> if the transform is sucessful.</returns>
            private bool Transform(string packageName, XmlNode xmlData, bool uninstall = false)
            {
                // The config file we want to modify
                if (xmlData.Attributes != null)
                {
                    string file = xmlData.Attributes.GetNamedItem("file").Value;

                    string sourceDocFileName = VirtualPathUtility.ToAbsolute(file);

                    // The xdt file used for tranformation
                    string fileEnd = "install.xdt";
                    if (uninstall)
                    {
                        fileEnd = $"un{fileEnd}";
                    }

                    string xdtfile = $"{xmlData.Attributes.GetNamedItem("xdtfile").Value}.{fileEnd}";
                    string xdtFileName = VirtualPathUtility.ToAbsolute(xdtfile);

                    // The translation at-hand
                    using (XmlTransformableDocument xmlDoc = new XmlTransformableDocument())
                    {
                        xmlDoc.PreserveWhitespace = true;
                        xmlDoc.Load(HttpContext.Current.Server.MapPath(sourceDocFileName));

                        using (XmlTransformation xmlTrans = new XmlTransformation(HttpContext.Current.Server.MapPath(xdtFileName)))
                        {
                            if (xmlTrans.Apply(xmlDoc))
                            {
                                // If we made it here, sourceDoc now has transDoc's changes
                                // applied. So, we're going to save the final result off to
                                // destDoc.
                                try
                                {
                                    xmlDoc.Save(HttpContext.Current.Server.MapPath(sourceDocFileName));
                                }
                                catch (Exception e)
                                {
                                    // Log error message
                                    string message = "Error executing TransformConfig package action (check file write permissions): " + e.Message;
                                    LogHelper.Error(typeof(TransformConfig), message, e);
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
