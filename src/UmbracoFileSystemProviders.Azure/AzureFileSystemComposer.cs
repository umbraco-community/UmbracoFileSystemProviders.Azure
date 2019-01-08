namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Configuration;
    using global::Umbraco.Core;
    using global::Umbraco.Core.Components;
    using global::Umbraco.Core.Exceptions;
    using global::Umbraco.Core.IO;

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AzureFileSystemComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            var containerName = ConfigurationManager.AppSettings[Constants.Configuration.ContainerNameKey];
            var rootUrl = ConfigurationManager.AppSettings[Constants.Configuration.RootUrlKey];
            var connectionString = ConfigurationManager.AppSettings[Constants.Configuration.ConnectionStringKey];
            var maxDays = ConfigurationManager.AppSettings[Constants.Configuration.MaxDaysKey];
            var useDefaultRoute = ConfigurationManager.AppSettings[Constants.Configuration.UseDefaultRouteKey]; 
            var usePrivateContainer = ConfigurationManager.AppSettings[Constants.Configuration.UsePrivateContainer];

            //Check we have all values set - otherwise make sure Umbraco does NOT boot so it can be configured correctly
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullOrEmptyException("containerName", $"The Azure File System is missing the value '{Constants.Configuration.ContainerNameKey}' from AppSettings");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentNullOrEmptyException("rootUrl", $"The Azure File System is missing the value '{Constants.Configuration.RootUrlKey}' from AppSettings");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullOrEmptyException("connectionString", $"The Azure File System is missing the value '{Constants.Configuration.ConnectionStringKey}' from AppSettings");

            if (string.IsNullOrEmpty(maxDays))
                throw new ArgumentNullOrEmptyException("maxDays", $"The Azure File System is missing the value '{Constants.Configuration.MaxDaysKey}' from AppSettings");

            if (string.IsNullOrEmpty(useDefaultRoute))
                throw new ArgumentNullOrEmptyException("useDefaultRoute", $"The Azure File System is missing the value '{Constants.Configuration.UseDefaultRouteKey}' from AppSettings");

            if (string.IsNullOrEmpty(usePrivateContainer))
                throw new ArgumentNullOrEmptyException("usePrivateContainer", $"The Azure File System is missing the value '{Constants.Configuration.UsePrivateContainer}' from AppSettings");


            composition.RegisterFileSystem<IMediaFileSystem, MediaFileSystem>(_ => new AzureBlobFileSystem(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, usePrivateContainer));
            composition.Components().Append<AzureFileSystemComponent>();
        }
    }
}
