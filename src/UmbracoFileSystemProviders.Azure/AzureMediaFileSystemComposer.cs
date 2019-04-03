namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using global::Umbraco.Core;
    using global::Umbraco.Core.Composing;
    using global::Umbraco.Core.Exceptions;

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AzureMediaFileSystemComposer : IComposer
    {
        private const string ProviderAlias = "media";
        public void Compose(Composition composition)
        {
            // if no connectionString appSetting then Umbraco installer hasn't completed yet
            var connectionString = ConfigurationHelper.GetAppSetting(Constants.Configuration.ConnectionStringKey, ProviderAlias);
            if (connectionString != null)
            {
                //Configuration
                var config = CreateConfiguration();

                //Reads config from AppSetting keys
                composition.RegisterUnique(config);

                //Set the Media FS to use our Azure FS with our config from AppSettings
                composition.SetMediaFileSystem(_ => new AzureBlobFileSystem(config));

                //Register component that deals with the VirtualPathProvider
                composition.Components().Append<AzureMediaFileSystemComponent>();
            }        
        }

        private AzureBlobFileSystemConfig CreateConfiguration()
        {
            var containerName = ConfigurationHelper.GetAppSetting(Constants.Configuration.ContainerNameKey, ProviderAlias);
            var rootUrl = ConfigurationHelper.GetAppSetting(Constants.Configuration.RootUrlKey, ProviderAlias);
            var connectionString = ConfigurationHelper.GetAppSetting(Constants.Configuration.ConnectionStringKey, ProviderAlias);
            var maxDays = ConfigurationHelper.GetAppSetting(Constants.Configuration.MaxDaysKey, ProviderAlias);
            var useDefaultRoute = ConfigurationHelper.GetAppSetting(Constants.Configuration.UseDefaultRouteKey, ProviderAlias);
            var usePrivateContainer = ConfigurationHelper.GetAppSetting(Constants.Configuration.UsePrivateContainer, ProviderAlias);

            //Check we have all values set - otherwise make sure Umbraco does NOT boot so it can be configured correctly
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullOrEmptyException("containerName", $"The Azure File System is missing the value '{Constants.Configuration.ContainerNameKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentNullOrEmptyException("rootUrl", $"The Azure File System is missing the value '{Constants.Configuration.RootUrlKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullOrEmptyException("connectionString", $"The Azure File System is missing the value '{Constants.Configuration.ConnectionStringKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(maxDays))
                throw new ArgumentNullOrEmptyException("maxDays", $"The Azure File System is missing the value '{Constants.Configuration.MaxDaysKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(useDefaultRoute))
                throw new ArgumentNullOrEmptyException("useDefaultRoute", $"The Azure File System is missing the value '{Constants.Configuration.UseDefaultRouteKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(usePrivateContainer))
                throw new ArgumentNullOrEmptyException("usePrivateContainer", $"The Azure File System is missing the value '{Constants.Configuration.UsePrivateContainer}:{ProviderAlias}' from AppSettings");

            bool disableVirtualPathProvider = ConfigurationHelper.GetAppSetting(Constants.Configuration.DisableVirtualPathProviderKey, ProviderAlias) != null
                           && ConfigurationHelper.GetAppSetting(Constants.Configuration.DisableVirtualPathProviderKey, ProviderAlias)
                                                  .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            return new AzureBlobFileSystemConfig
            {
                DisableVirtualPathProvider = disableVirtualPathProvider,
                ContainerName = containerName,
                RootUrl = rootUrl,
                ConnectionString = connectionString,
                MaxDays = maxDays,
                UseDefaultRoute = useDefaultRoute,
                UsePrivateContainer = usePrivateContainer
            };
        }

    }
}
