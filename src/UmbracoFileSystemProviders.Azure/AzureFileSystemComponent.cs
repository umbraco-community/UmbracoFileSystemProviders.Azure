// <copyright file="AzureFileSystemComponent.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Configuration;
    using global::Umbraco.Core.Components;
    using global::Umbraco.Core.IO;

    public class AzureFileSystemComponent : UmbracoComponentBase, IUmbracoUserComponent
    {
        /// <summary>
        /// The configuration key for disabling the virtual path provider.
        /// </summary>
        private const string DisableVirtualPathProviderKey = Constants.Configuration.DisableVirtualPathProviderKey;

        public void Initialize(FileSystems fileSystems)
        {
            bool disable = ConfigurationManager.AppSettings[DisableVirtualPathProviderKey] != null
                           && ConfigurationManager.AppSettings[DisableVirtualPathProviderKey]
                                                  .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            IFileSystem fileSystem = fileSystems.GetUnderlyingFileSystemProvider(Constants.DefaultMediaRoute);
            bool isAzureBlobFileSystem = fileSystem is AzureBlobFileSystem;

            if (!disable && isAzureBlobFileSystem)
            {
                AzureFileSystem azureFileSystem = ((AzureBlobFileSystem)fileSystem).FileSystem;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (azureFileSystem.UseDefaultRoute)
                {
                    FileSystemVirtualPathProvider.ConfigureMedia(Constants.DefaultMediaRoute);
                }
                else
                {
                    FileSystemVirtualPathProvider.ConfigureMedia(azureFileSystem.ContainerName);
                }
            }
        }
    }
}
