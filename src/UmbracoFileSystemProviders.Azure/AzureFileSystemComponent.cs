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

    public class AzureFileSystemComponent : IComponent
    {
        /// <summary>
        /// The configuration key for disabling the virtual path provider.
        /// </summary>
        private const string DisableVirtualPathProviderKey = Constants.Configuration.DisableVirtualPathProviderKey;
        private readonly SupportingFileSystems supportingFileSystems;

        public AzureFileSystemComponent(SupportingFileSystems supportingFileSystems)
        {
            this.supportingFileSystems = supportingFileSystems;
        }

        public void Initialize()
        {
            bool disable = ConfigurationManager.AppSettings[DisableVirtualPathProviderKey] != null
                           && ConfigurationManager.AppSettings[DisableVirtualPathProviderKey]
                                                  .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            var azureFs = this.supportingFileSystems.For<IMediaFileSystem>() as AzureBlobFileSystem;
            if (!disable && azureFs != null)
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
