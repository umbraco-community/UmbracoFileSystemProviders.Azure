// <copyright file="AzureFileSystemComponent.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using global::Umbraco.Core.Components;
    using global::Umbraco.Core.IO;

    public class AzureFileSystemComponent : IComponent
    {

        private readonly SupportingFileSystems supportingFileSystems;
        private readonly AzureBlobFileSystemConfig config;

        public AzureFileSystemComponent(SupportingFileSystems supportingFileSystems, AzureBlobFileSystemConfig config)
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

        public void Terminate()
        {

        }
    }
}
