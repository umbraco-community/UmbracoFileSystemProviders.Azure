// <copyright file="AzureFileSystemComponent.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using global:: Umbraco.Core.Composing;
    using global::Umbraco.Core.IO;

    public class AzureMediaFileSystemComponent : IComponent
    {
        private readonly SupportingFileSystems supportingFileSystems;
        private readonly AzureBlobFileSystemConfig config;

        public AzureMediaFileSystemComponent(SupportingFileSystems supportingFileSystems, AzureBlobFileSystemConfig config)
        {
            this.supportingFileSystems = supportingFileSystems;
            this.config = config;
        }

        public void Initialize()
        {
            var azureFs = this.supportingFileSystems.For<IMediaFileSystem>() as AzureBlobFileSystem;

            if (!this.config.DisableVirtualPathProvider && azureFs != null)
            {
                AzureFileSystem azureFileSystem = azureFs.FileSystem;

                var route = azureFileSystem.UseDefaultRoute ? Constants.DefaultMediaRoute : azureFileSystem.ContainerName;

                FileSystemVirtualPathProvider.Configure(route, new Lazy<IFileSystem>(() => azureFileSystem));
            }
        }

        public void Terminate()
        {

        }
    }
}
