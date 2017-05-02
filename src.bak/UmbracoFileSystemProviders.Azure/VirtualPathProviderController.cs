// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VirtualPathProviderController.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South and contributors.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Configures the virtual path provider to correctly retrieve and serve resources from the media section.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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

            IFileSystem fileSystem = FileSystemProviderManager.Current.GetUnderlyingFileSystemProvider("media");
            bool isAzureBlobFileSystem = fileSystem.GetType() == typeof(AzureBlobFileSystem);

            if (!disable && isAzureBlobFileSystem)
            {
                var containerName = ((AzureBlobFileSystem)fileSystem).FileSystem.ContainerName;
                FileSystemVirtualPathProvider.ConfigureMedia(containerName);
            }

            base.ApplicationStarting(umbracoApplication, applicationContext);
        }
    }
}
