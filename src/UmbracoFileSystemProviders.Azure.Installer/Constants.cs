// <copyright file="Constants.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    /// <summary>
    /// Contains constants related to the installer.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The installer path for the plugin.
        /// </summary>
        public const string InstallerPath = "~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/";

        /// <summary>
        /// The filesystem provider configuration file name.
        /// </summary>
        public const string FileSystemProvidersConfigFile = "FileSystemProviders.config";

        /// <summary>
        /// The Umbraco configuration path.
        /// </summary>
        public const string UmbracoConfigPath = "~/Config/";

        /// <summary>
        /// The web configuration file name.
        /// </summary>
        public const string WebConfigFile = "web.config";

        /// <summary>
        /// The media web configuration xdt file name.
        /// </summary>
        public const string MediaWebConfigXdtFile = "media-web.config";

        /// <summary>
        /// The full qualified type name for the provider.
        /// </summary>
        public const string ProviderType = "Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure";

        /// <summary>
        /// Contains constant values related to the ImageProcessor library.
        /// </summary>
        public class ImageProcessor
        {
            /// <summary>
            /// The assembly path to the ImageProcessor.Web binary.
            /// </summary>
            public const string WebAssemblyPath = "~/bin/ImageProcessor.Web.dll";

            /// <summary>
            /// The minimum ImageProcessor.Web version.
            /// </summary>
            public const string WebMinRequiredVersion = "4.3.2.0";

            /// <summary>
            /// The ImageProcessor.Web configuration path.
            /// </summary>
            public const string ConfigPath = "~/Config/imageprocessor/";

            /// <summary>
            /// The ImageProcessor.Web security configuration file.
            /// </summary>
            public const string SecurityConfigFile = "security.config";

            /// <summary>
            /// The full qualified type for the ImageProcessor.Web CloudImageService.
            /// </summary>
            public const string SecurityServiceType = "ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web";

            /// <summary>
            /// The CloudImageService name.
            /// </summary>
            public const string SecurityServiceName = "CloudImageService";
        }
    }
}
