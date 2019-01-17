namespace Our.Umbraco.FileSystemProviders.Azure
{
    public class AzureBlobFileSystemConfig
    {
        public bool DisableVirtualPathProvider { get; set; }

        /// <summary>
        /// The container name
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// The root url
        /// </summary>
        public string RootUrl { get; set; }

        /// <summary>
        /// The connection string
        /// </summary>
        public string ConnectionString { get; set; }

        //TODO: INT - The current underlying AzureBlobFileSystem expects this as string - YUK
        /// <summary>
        /// The maximum number of days to cache blob items for in the browser
        /// </summary>
        public string MaxDays { get; set; }

        //TODO: BOOL - The current underlying AzureBlobFileSystem expects this as string - YUK
        /// <summary>
        /// Whether to use the default "media" route in the url independent of the blob container
        /// </summary>
        public string UseDefaultRoute { get; set; }

        //TODO: BOOL - The current underlying AzureBlobFileSystem expects this as string - YUK
        /// <summary>
        /// Blob container can be private (no direct access) or public (direct access possible, default)
        /// </summary>
        public string UsePrivateContainer { get; set; }
    }
}
