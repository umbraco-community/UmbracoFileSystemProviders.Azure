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
                throw new ArgumentNullOrEmptyException("containerName");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentNullOrEmptyException("rootUrl");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullOrEmptyException("connectionString");

            if (string.IsNullOrEmpty(maxDays))
                throw new ArgumentNullOrEmptyException("maxDays");

            if (string.IsNullOrEmpty(useDefaultRoute))
                throw new ArgumentNullOrEmptyException("useDefaultRoute");

            if (string.IsNullOrEmpty(usePrivateContainer))
                throw new ArgumentNullOrEmptyException("usePrivateContainer");


            composition.RegisterFileSystem<IMediaFileSystem, MediaFileSystem>(_ => new AzureBlobFileSystem(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, usePrivateContainer));
            composition.Components().Append<AzureFileSystemComponent>();
        }
    }
}
