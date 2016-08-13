// <copyright file="VirtualPathProviderController.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Configuration;

    using global::Umbraco.Core;
    using global::Umbraco.Core.IO;

    /// <summary>
    /// Configures the virtual path provider to correctly retrieve and serve resources from the media section.
    /// </summary>
    public class VirtualPathProviderController : ApplicationEventHandler
    {
        /// <summary>
        /// The configuration key for disabling the virtual path provider.
        /// </summary>
        private const string DisableVirtualPathProviderKey = Constants.Configuration.DisableVirtualPathProviderKey;

        /// <summary>
        /// Overridable method to execute when All resolvers have been initialized but resolution is not
        /// frozen so they can be modified in this method
        /// </summary>
        /// <param name="umbracoApplication">The current <see cref="UmbracoApplicationBase"/></param>
        /// <param name="applicationContext">The Umbraco <see cref="ApplicationContext"/> for the current application.</param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            bool disable = ConfigurationManager.AppSettings[DisableVirtualPathProviderKey] != null
                           && ConfigurationManager.AppSettings[DisableVirtualPathProviderKey]
                                                  .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            IFileSystem fileSystem = FileSystemProviderManager.Current.GetUnderlyingFileSystemProvider(Constants.DefaultMediaRoute);
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

            base.ApplicationStarting(umbracoApplication, applicationContext);
        }
    }
}
