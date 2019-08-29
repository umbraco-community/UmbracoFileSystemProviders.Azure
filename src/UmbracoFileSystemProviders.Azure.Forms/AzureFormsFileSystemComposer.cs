using Our.Umbraco.FileSystemProviders.Azure;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Forms.Core.Components;
using Umbraco.Forms.Data.FileSystem;
using Constants = Our.Umbraco.FileSystemProviders.Azure.Constants;

namespace UmbracoFileSystemProviders.Azure.Forms
{
    [ComposeAfter(typeof(UmbracoFormsComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AzureFormsFileSystemComposer : IComposer
    {
        private const string ProviderAlias = "forms";
        public void Compose(Composition composition)
        {
            var connectionString = ConfigurationHelper.GetAppSetting(Constants.Configuration.ConnectionStringKey, ProviderAlias);
            if (connectionString != null)
            {
                var config = CreateConfiguration();

                //Reads config from AppSetting keys
                composition.RegisterUnique(config);
                composition.RegisterUniqueFor<IFileSystem, FormsFileSystemForSavedData>(_ => new AzureBlobFileSystem(config));
            }
        }

        private AzureBlobFileSystemConfig CreateConfiguration()
        {
            var containerName = ConfigurationHelper.GetAppSetting(Constants.Configuration.ContainerNameKey, ProviderAlias);
            var rootUrl = ConfigurationHelper.GetAppSetting(Constants.Configuration.RootUrlKey, ProviderAlias);
            var connectionString = ConfigurationHelper.GetAppSetting(Constants.Configuration.ConnectionStringKey, ProviderAlias);
            var usePrivateContainer = ConfigurationHelper.GetAppSetting(Constants.Configuration.UsePrivateContainer, ProviderAlias);

            //Check we have all values set - otherwise make sure Umbraco does NOT boot so it can be configured correctly
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullOrEmptyException("containerName", $"The Azure File System is missing the value '{Constants.Configuration.ContainerNameKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentNullOrEmptyException("rootUrl", $"The Azure File System is missing the value '{Constants.Configuration.RootUrlKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullOrEmptyException("connectionString", $"The Azure File System is missing the value '{Constants.Configuration.ConnectionStringKey}:{ProviderAlias}' from AppSettings");

            if (string.IsNullOrEmpty(usePrivateContainer))
                throw new ArgumentNullOrEmptyException("usePrivateContainer", $"The Azure File System is missing the value '{Constants.Configuration.UsePrivateContainer}:{ProviderAlias}' from AppSettings");

            return new AzureBlobFileSystemConfig
            {
                ContainerName = containerName,
                RootUrl = rootUrl,
                ConnectionString = connectionString,
                UsePrivateContainer = usePrivateContainer
            };
        }
    }
}
