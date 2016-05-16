// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Constant strings for use within the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    public static class Constants
    {
        public const string InstallerPath = "~/App_Plugins/UmbracoFileSystemProviders/Azure/Install/";
        public const string FileSystemProvidersConfigFile = "FileSystemProviders.config";
        public const string UmbracoConfigPath = "~/Config/";
        public const string WebConfigFile = "web.config";
        public const string ProviderType = "Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure";

        public class ImageProcessor
        {
            public const string WebAssemblyPath = "~/bin/ImageProcessor.Web.dll";
            public const string WebMinRequiredVersion = "4.3.2.0";
            public const string ConfigPath = "~/Config/imageprocessor/";
            public const string SecurityConfigFile = "security.config";
            public const string SecurityServiceType = "ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web";
            public const string SecurityServiceName = "CloudImageService";

        }
    }
}
