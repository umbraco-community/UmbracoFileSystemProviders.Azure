// <copyright file="AzureFileSystem.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

// <summary>
// A singleton class for communicating with Azure Blob Storage.
// </summary>
namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using global::Azure;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using global::Umbraco.Core.Composing;
    using global::Umbraco.Core.Configuration;
    using global::Umbraco.Core.IO;
    using global::Umbraco.Core.Logging;

    /// <summary>
    /// A class for communicating with Azure Blob Storage.
    /// </summary>
    public class AzureFileSystem : IFileSystem
    {
        /// <summary>
        /// The configuration key for enabling the storage emulator.
        /// </summary>
        private const string UseStorageEmulatorKey = Constants.Configuration.UseStorageEmulatorKey;

        /// <summary>
        /// The configuration key for disabling the virtual path provider.
        /// </summary>
        private const string DisableVirtualPathProviderKey = Constants.Configuration.DisableVirtualPathProviderKey;

        /// <summary>
        /// The delimiter.
        /// </summary>
        private const string Delimiter = "/";

        /// <summary>
        /// The regex for parsing container names.
        /// </summary>
        private static readonly Regex ContainerRegex = new Regex("^[a-z0-9](?:[a-z0-9]|(\\-(?!\\-))){1,61}[a-z0-9]$|^\\$root$", RegexOptions.Compiled);

        /// <summary>
        /// Our object to lock against during initialization.
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// A list of <see cref="AzureFileSystem"/>.
        /// </summary>
        private static readonly List<AzureFileSystem> FileSystems = new List<AzureFileSystem>();

        /// <summary>
        /// The root host url.
        /// </summary>
        private readonly string rootHostUrl;

        /// <summary>
        /// The combined root and container url.
        /// </summary>
        private readonly string rootContainerUrl;

        /// <summary>
        /// The cloud media blob container.
        /// </summary>
        private readonly BlobContainerClient cloudBlobContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="useDefaultRoute">Whether to use the default "media" route in the url independent of the blob container.</param>
        /// <param name="accessType"><see cref="BlobContainerPublicAccessType"/> indicating the access permissions.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="containerName"/> is null or whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="connectionString"/> is invalid.
        /// </exception>
        internal AzureFileSystem(string containerName, string rootUrl, string connectionString, int maxDays, bool useDefaultRoute, PublicAccessType accessType)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            this.DisableVirtualPathProvider = ConfigurationHelper.GetAppSetting(DisableVirtualPathProviderKey) != null
                                              && ConfigurationHelper.GetAppSetting(DisableVirtualPathProviderKey)
                                             .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            bool useEmulator = ConfigurationHelper.GetAppSetting(UseStorageEmulatorKey) != null
                               && ConfigurationHelper.GetAppSetting(UseStorageEmulatorKey)
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            if (useEmulator)
            {
                _connectionString = "UseDevelopmentStorage=true";
                rootUrl = "http://127.0.0.1:10000/devstoreaccount1/";
            }
            else
            {
                _connectionString = connectionString;
            }

            ValidateSasToken(connectionString);

            this.cloudBlobContainer = CreateContainer(containerName, accessType);

            // First assign a local copy before editing. We use that to track the type.
            // TODO: Do we need this? The container should be an identifer.
            this.rootHostUrl = rootUrl;

            if (!rootUrl.Trim().EndsWith("/"))
            {
                rootUrl = rootUrl.Trim() + "/";
            }

            this.rootContainerUrl = rootUrl + containerName + "/";
            this.ContainerName = containerName;
            this.MaxDays = maxDays;
            this.UseDefaultRoute = useDefaultRoute;

            this.MimeTypeResolver = new MimeTypeResolver();
        }

        private static void ValidateSasToken(string connectionString)
        {
            //TODO: Validate SAS

            //bool isValidSas = true;
            //var sasTokenParts = cloudStorageAccount.Credentials.SASToken.Split('&');
            //var si = sasTokenParts.Where(t => t.StartsWith("si=")).FirstOrDefault();
            //if (si != null)
            //{
            //    var siValue = si.Split('=')[1];

            //    // I could not find a way how to get the permissions of a referenced access policy
            //    // var permissions = this.cloudBlobContainer.GetPermissions(AccessCondition.GenerateIfExistsCondition());
            //}
            //else
            //{
            //    var sr = sasTokenParts.Where(t => t.StartsWith("sr=")).FirstOrDefault();
            //    var ss = sasTokenParts.Where(t => t.StartsWith("ss=")).FirstOrDefault();
            //    var srt = sasTokenParts.Where(t => t.StartsWith("srt=")).FirstOrDefault();
            //    var sp = sasTokenParts.Where(t => t.StartsWith("sp=")).FirstOrDefault();
            //    if ((ss == null || !ss.Contains("b")) && (sr == null || !sr.Contains("c")))
            //    {
            //        isValidSas = false;
            //    }
            //    else if (sp != null)
            //    {
            //        var value = sp.Split('=')[1].ToCharArray();
            //        if (!value.Contains('r') || !value.Contains('w') || !value.Contains('d') || !value.Contains('l'))
            //        {
            //            isValidSas = false;
            //        }
            //        else if (srt != null)
            //        {
            //            value = srt.Split('=')[1].ToCharArray();
            //            if (!value.Contains('s') || !value.Contains('c') || !value.Contains('o'))
            //            {
            //                isValidSas = false;
            //            }
            //        }
            //    }
            //}

            //if (!isValidSas)
            //{
            //    throw new Exception("SAS token permissions do NOT grant full functionality for UmbracoFileSystemProviders.Azure.");
            //}
        }

        /// <summary>
        /// Gets or sets the MIME type resolver.
        /// </summary>
        public IMimeTypeResolver MimeTypeResolver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the virtual path provider.
        /// </summary>
        public bool DisableVirtualPathProvider { get; set; }

        /// <summary>
        /// Gets the container name.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        /// Gets the maximum number of days to cache blob items for in the browser.
        /// </summary>
        public int MaxDays { get; }

        /// <summary>
        /// Gets a value indicating whether to use the default "media" route in the url
        /// independent of the blob container.
        /// </summary>
        public bool UseDefaultRoute { get; }

        /// <summary>
        /// Gets or sets func to calculate application virtual path
        /// </summary>
        public string ApplicationVirtualPath { get; internal set; } = HttpRuntime.AppDomainAppVirtualPath;

        public bool CanAddPhysical => false;

        /// <summary>
        /// Returns a singleton instance of the <see cref="AzureFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="useDefaultRoute">Whether to use the default "media" route in the url independent of the blob container.</param>
        /// <param name="usePrivateContainer">Whether to use private blob access (no direct access) or public (direct access possible, default) access.</param>
        /// <returns>The <see cref="AzureFileSystem"/></returns>
        public static AzureFileSystem GetInstance(string containerName, string rootUrl, string connectionString, string maxDays, string useDefaultRoute, string usePrivateContainer)
        {
            lock (Locker)
            {
                AzureFileSystem fileSystem = FileSystems.SingleOrDefault(fs => fs.ContainerName == containerName && fs.rootHostUrl == rootUrl);

                if (fileSystem == null)
                {
                    if (!int.TryParse(maxDays, out int max))
                    {
                        max = 365;
                    }

                    if (!bool.TryParse(useDefaultRoute, out bool defaultRoute))
                    {
                        defaultRoute = true;
                    }

                    if (!bool.TryParse(usePrivateContainer, out bool privateContainer))
                    {
                        privateContainer = true;
                    }

                    PublicAccessType blobContainerPublicAccessType = privateContainer ? PublicAccessType.None : PublicAccessType.Blob;

                    fileSystem = new AzureFileSystem(containerName, rootUrl, connectionString, max, defaultRoute, blobContainerPublicAccessType);
                    FileSystems.Add(fileSystem);
                }

                return fileSystem;
            }
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">The path to the given file.</param>
        /// <param name="stream">The <see cref="Stream"/> containing the file contents.</param>
        /// <param name="overrideIfExists">Whether to override the file if it already exists.</param>
        public void AddFile(string path, Stream stream, bool overrideIfExists)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"AddFile(path, steam, overrideIfExists) method executed with path:{path}");

            BlobClient blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                bool exists = blockBlob.Exists();
                DateTimeOffset created = DateTimeOffset.MinValue;

                if (!overrideIfExists && exists)
                {
                    InvalidOperationException error = new InvalidOperationException($"File already exists at {blockBlob.Uri}");
                    Current.Logger.Error<AzureBlobFileSystem>(error, "File already exists as {Path}", path);
                    return;
                }

                try
                {
                    BlobProperties properties = null;
                    if (exists)
                    {
                        // Ensure original created date is preserved.
                        properties = blockBlob.GetProperties().Value;
                        if (properties.Metadata.ContainsKey("CreatedDate"))
                        {
                            // We store the creation date in meta data.
                            created = DateTime.Parse(properties.Metadata["CreatedDate"], CultureInfo.InvariantCulture).ToUniversalTime();
                        }
                    }

                    blockBlob.Upload(stream,overrideIfExists);

                    string contentType = this.MimeTypeResolver.Resolve(path);

                    properties = blockBlob.GetProperties().Value;
                    var updatedHeaders = new BlobHttpHeaders();
                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        updatedHeaders.ContentType = contentType;
                    }

                    updatedHeaders.CacheControl = $"public, max-age={this.MaxDays * 86400}";

                    blockBlob.SetHttpHeaders(updatedHeaders);

                    if (created == DateTimeOffset.MinValue)
                    {
                        created = DateTimeOffset.UtcNow;
                    }

                    var updatedProps = new Dictionary<string, string>();
                    // Store the creation date in meta data.
                    updatedProps.Add("CreatedDate",created.ToString(CultureInfo.InvariantCulture));

                    blockBlob.SetMetadata(updatedProps);
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<AzureBlobFileSystem>(ex, "Unable to upload file at {Path}", path);
                }
            }
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">The path to the given file.</param>
        /// <param name="stream">The <see cref="Stream"/> containing the file contents.</param>
        public void AddFile(string path, Stream stream)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"AddFile(path, steam) method executed with path:{path}");

            this.AddFile(path, stream, true);
        }

        /// <inheritdoc/>
        public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
        {
            var fullPath = GetFullPath(path);
            Current.Logger.Debug<AzureBlobFileSystem>($"NotImplemented! AddFile(path, physicalPath, overrideIfExists, copy) method executed with path:{path}, {physicalPath}, {overrideIfExists}, {copy} - fullPath: {fullPath}");

            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <remarks>Azure blob storage has no real concept of directories so deletion is always recursive.</remarks>
        /// <param name="path">The name of the directory to remove.</param>
        /// <param name="recursive">
        /// <c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>.
        /// </param>
        public void DeleteDirectory(string path, bool recursive)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"DeleteDirectory(path, recursive) method executed with path:{path}");

            path = this.FixPath(path);

            if (!this.DirectoryExists(path))
            {
                return;
            }

            var directory = this.GetDirectoryReference(path);

            // WB: This will only delete a folder if it only has files & not sub directories
            // IEnumerable<BlobClient> blobs = directory.ListBlobs().OfType<BlobClient>();
            var blobs = directory.ListBlobs();

            if (recursive)
            {
                foreach (var blobItem in blobs)
                {
                    try
                    {
                        
                        if (blobItem.IsPrefix)
                        {
                            var blobFolder = new AzureBlobDirectory(directory.Container, blobItem.Prefix);

                            // Resursively call this method
                            this.DeleteDirectory(blobFolder.Prefix);
                        }
                        else
                        {
                            // Can assume its a file aka CloudBlob
                            var blobFile = GetBlockBlobReference(blobItem.Blob.Name);
                            blobFile?.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                        }
                    }
                    catch (Exception ex)
                    {
                        Current.Logger.Error<AzureBlobFileSystem>(ex, "Unable to delete directory at {Path}", path);
                    }
                }

                return;
            }

            // Delete the directory.
            // Force recursive since Azure has no real concept of directories
            this.DeleteDirectory(path, true);
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        public void DeleteDirectory(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"DeleteDirectory(path) method executed with path:{path}");

            this.DeleteDirectory(path, false);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to remove.</param>
        public void DeleteFile(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"DeleteFile(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                try
                {
                    blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<AzureBlobFileSystem>(ex, "Unable to delete file at {Path}", path);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified directory exists.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>
        /// <c>True</c> if the directory exists and the user has permission to view it; otherwise <c>false</c>.
        /// </returns>
        public bool DirectoryExists(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"DirectoryExists(path) method executed with path:{path}");

            string fixedPath = this.FixPath(path);
            var directory = new AzureBlobDirectory(cloudBlobContainer,fixedPath);

            return directory.ListBlobs().Any();
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        /// <c>True</c> if the file exists and the user has permission to view it; otherwise <c>false</c>.
        /// </returns>
        public bool FileExists(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"FileExists(path) method executed with path:{path}");

            var blockBlobReference = this.GetBlockBlobReference(path);
            return blockBlobReference?.Exists() ?? false;
        }

        /// <summary>
        /// Gets the created date/time of the file, expressed as a UTC value.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="DateTimeOffset"/>.
        /// </returns>
        public DateTimeOffset GetCreated(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetCreated(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                // Populate the blob's attributes.
                var properties = blockBlob.GetProperties().Value;
                if (properties.Metadata.ContainsKey("CreatedDate"))
                {
                    // We store the creation date in meta data.
                    return DateTimeOffset.Parse(properties.Metadata["CreatedDate"], CultureInfo.InvariantCulture).ToUniversalTime();
                }
            }

            return DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Gets all directories matching the given path.
        /// </summary>
        /// <param name="path">The path to the directories.</param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched directories.
        /// </returns>
        public IEnumerable<string> GetDirectories(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetDirectories(path) method executed with path:{path}");

            var directory = this.GetDirectoryReference(path);

            var listedBlobs = directory.ListBlobs();
            var prefixes = listedBlobs.Where(blob => blob.IsPrefix).Select(x=>x.Prefix).ToList();
            var blobPrefixes = listedBlobs.Where(x => x.IsBlob && x.Blob.Name.LastIndexOf('/') >= 0).Select(x => x.Blob.Name.Substring(0, x.Blob.Name.LastIndexOf('/') + 1));


            // Always get last segment for media sub folder simulation. E.g 1001, 1002
            var all = prefixes.Union(blobPrefixes);
            return all.Select(cd => cd.TrimEnd('/'));
        }

        /// <summary>
        /// Gets all files matching the given path and filter.
        /// </summary>
        /// <param name="path">The path to the files.</param>
        /// <param name="filter">A filter that allows the querying of file extension. <example>*.jpg</example></param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched files.
        /// </returns>
        public IEnumerable<string> GetFiles(string path, string filter)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetFiles(path, filter) method executed with path:{path} & filter {filter}");

            IEnumerable<BlobHierarchyItem> blobs = this.cloudBlobContainer.GetBlobsByHierarchy(prefix:this.FixPath(path));

            var blobList = blobs as IList<BlobHierarchyItem> ?? blobs.ToList();

            if (!blobList.Any())
            {
                var ex = new DirectoryNotFoundException($"Blob not found at '{path}'");
                Current.Logger.Error<AzureBlobFileSystem>(ex, "Blob not found at {Path}", path);
                return Enumerable.Empty<string>();
            }

            return blobList.Where(x=>x.IsBlob).Select(cd =>
            {
                string url = cloudBlobContainer.Uri.AbsoluteUri + "/" + cd.Blob.Name;

                if (filter.Equals("*.*", StringComparison.InvariantCultureIgnoreCase))
                {
                    return url.Substring(this.rootContainerUrl.Length);
                }

                // Filter by name.
                filter = filter.TrimStart('*');
                if (url.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return url.Substring(this.rootContainerUrl.Length);
                }

                return null;
            }).Where(x => x != null);
        }

        /// <summary>
        /// Gets all files matching the given path.
        /// </summary>
        /// <param name="path">The path to the files.</param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched files.
        /// </returns>
        public IEnumerable<string> GetFiles(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetFiles(path) method executed with path:{path}");

            return this.GetFiles(path, "*.*");
        }

        /// <summary>
        /// Gets the full path to the media item.
        /// </summary>
        /// <param name="path">The file to return the full path for.</param>
        /// <returns>
        /// The <see cref="string"/> representing the full path.
        /// </returns>
        public string GetFullPath(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetFullPath(path) method executed with path:{path}");

            return this.ResolveUrl(path, false);
        }

        /// <summary>
        /// Gets the last modified date/time of the file, expressed as a UTC value.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="DateTimeOffset"/>.
        /// </returns>
        public DateTimeOffset GetLastModified(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetLastModified(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                var properties = blockBlob.GetProperties().Value;
                return properties.LastModified;
            }

            return DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Returns the application relative path to the file.
        /// </summary>
        /// <param name="fullPathOrUrl">The full path or url.</param>
        /// <returns>
        /// The <see cref="string"/> representing the relative path.
        /// </returns>
        public string GetRelativePath(string fullPathOrUrl)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetRelativePath(path) method executed with fullPathOrUrl:{fullPathOrUrl}");

            return this.FixPath(fullPathOrUrl);
        }

        /// <summary>
        /// Returns the application relative url to the file.
        /// </summary>
        /// <remarks>If the virtual path provider is enabled this returns a relative url.</remarks>
        /// <param name="path">The path to return the url for.</param>
        /// <returns>
        /// <see cref="string"/>.
        /// </returns>
        public string GetUrl(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetUrl(path) method executed with path:{path}");

            if (this.DisableVirtualPathProvider)
            {
                return this.ResolveUrl(path, false);
            }

            return this.ResolveUrl(path, true);
        }

        /// <inheritdoc/>
        public long GetSize(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetSize(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                var properties = blockBlob.GetProperties().Value;
                return properties.ContentLength;
            }

            return long.MinValue;
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> representing the file at the gieven path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="Stream"/>.
        /// </returns>
        public Stream OpenFile(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"OpenFile(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                if (!blockBlob.Exists())
                {
                    Current.Logger.Info<AzureBlobFileSystem>("No file exists at {Path}", path);
                    return null;
                }

                MemoryStream stream = new MemoryStream();
                blockBlob.DownloadTo(stream);

                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                return stream;
            }

            return null;
        }

        public string GetETag(string path,string format = "G")
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetSize(path) method executed with path:{path}");

            var blockBlob = this.GetBlockBlobReference(path);

            if (blockBlob != null)
            {
                var properties = blockBlob.GetProperties().Value;
                return properties.ETag.ToString(format);
            }

            return null;
        }

        private string _connectionString;

        private bool _isSasCredential = false;// cloudBlobClient.Credentials.IsSAS;
        /// <summary>
        /// Returns the media container, creating a new one if none exists.
        /// </summary>
        /// <param name="cloudBlobClient"><see cref="CloudBlobClient"/> where the container is stored.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessType"><see cref="BlobContainerPublicAccessType"/> indicating the access permissions.</param>
        /// <returns>The <see cref="CloudBlobContainer"/></returns>
        public BlobContainerClient CreateContainer(string containerName, PublicAccessType accessType)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"CreateContainer(cloudBlobClient, containerName, accessType) method executed with containerName:{containerName}");

            containerName = containerName.ToLowerInvariant();

            // Validate container name - from: http://stackoverflow.com/a/23364534/5018
            bool isContainerNameValid = ContainerRegex.IsMatch(containerName);
            if (isContainerNameValid == false)
            {
                throw new ArgumentException($"The container name {containerName} is not valid, see https://msdn.microsoft.com/en-us/library/azure/dd135715.aspx for the restrtictions for container names.");
            }

            BlobContainerClient container = new BlobContainerClient(_connectionString,containerName.ToLowerInvariant());
            if (_isSasCredential)
            {
                bool canCreateContainer = SasCredentialHasContainerCreatePermission();
                if (canCreateContainer)
                {
                    container.CreateIfNotExists(accessType);
                }
            }
            else if (!container.Exists())
            {
                container.CreateIfNotExists();
                container.SetAccessPolicy(accessType);
            }

            return container;
        }

        private static bool SasCredentialHasContainerCreatePermission()
        {
            throw new NotImplementedException();
            //// Shared access signatures (SAS) have some limitations compared to shared access keys
            //// read more on: https://docs.microsoft.com/en-us/azure/storage/common/storage-dotnet-shared-access-signature-part-1
            //string[] sasTokenProperties = cloudBlobClient.Credentials.SASToken.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //bool isAccountSas = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("ss=")).FirstOrDefault() != null;
            //string allowedServices = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("ss=")).FirstOrDefault();
            //if (allowedServices != null)
            //{
            //    allowedServices = allowedServices.Split('=')[1].ToLower();
            //}
            //else
            //{
            //    allowedServices = string.Empty;
            //}

            //string resourceTypes = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("srt=")).FirstOrDefault();
            //if (resourceTypes != null)
            //{
            //    resourceTypes = resourceTypes.Split('=')[1].ToLower();
            //}
            //else
            //{
            //    resourceTypes = string.Empty;
            //}

            //string permissions = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("sp=")).FirstOrDefault();
            //if (permissions != null)
            //{
            //    permissions = permissions.Split('=')[1].ToLower();
            //}
            //else
            //{
            //    permissions = string.Empty;
            //}

            //bool canCreateContainer = allowedServices.Contains('b') && resourceTypes.Contains('c') && permissions.Contains('c');
            //return canCreateContainer;
        }

        /// <summary>
        /// Gets a reference to the block blob matching the given path.
        /// </summary>
        /// <param name="path">The path to the blob.</param>
        /// <returns>
        /// The <see cref="BlobClient"/> reference.
        /// </returns>
        private BlobClient GetBlockBlobReference(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetBlockBlobReference(path) method executed with path:{path}");

            string blobPath = this.FixPath(path);

            // Only make the request if there is an actual path. See issue 8.
            // https://github.com/JimBobSquarePants/UmbracoFileSystemProviders.Azure/issues/8
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return this.cloudBlobContainer.GetBlobClient(blobPath);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // blob doesn't exist yet
                var blobReference = this.cloudBlobContainer.GetBlobClient(blobPath);
                return blobReference;

            }
            catch (RequestFailedException ex)
            {
                Current.Logger.Error<AzureBlobFileSystem>(
                    $"GetBlockBlobReference exception {ex}");
                return null;
            }
        }

        /// <summary>
        /// Gets a reference to the directory matching the given path.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>
        /// The <see cref="BlobClient"/> reference.
        /// </returns>
        private AzureBlobDirectory GetDirectoryReference(string path)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"GetDirectoryReference(path) method executed with path:{path}");

            string blobPath = this.FixPath(path);
            return new AzureBlobDirectory(cloudBlobContainer,blobPath);
        }

        /// <summary>
        /// Returns the correct url to the media item.
        /// </summary>
        /// <param name="path">The path to the item to return.</param>
        /// <param name="relative">Whether to return a relative path.</param>
        /// <returns>
        /// <see cref="string"/>.
        /// </returns>
        private string ResolveUrl(string path, bool relative)
        {
            Current.Logger.Debug<AzureBlobFileSystem>($"ResolveUrl(path) method executed with path:{path}");

            // First create the full url
            string fixedPath = this.FixPath(path);

            Uri url = new Uri(new Uri(this.rootContainerUrl, UriKind.Absolute), fixedPath);

            if (!relative)
            {
                return url.AbsoluteUri;
            }

            if (this.UseDefaultRoute)
            {
                return $"{this.ApplicationVirtualPath?.TrimEnd('/')}/{Constants.DefaultMediaRoute}/{fixedPath}";
            }

            return $"{this.ApplicationVirtualPath?.TrimEnd('/')}/{this.ContainerName}/{fixedPath}";
        }

        /// <summary>
        /// Fixes the path ensuring that only the media subfolder normalized to
        /// a url format is returned.
        /// </summary>
        /// <param name="path">The path to fix.</param>
        /// <returns>
        /// The <see cref="string"/> <example>1001/image.jpg</example>.
        /// </returns>
        private string FixPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = path.Replace("\\", Delimiter);

            string appVirtualPath = this.ApplicationVirtualPath;
            if (!string.IsNullOrWhiteSpace(appVirtualPath) && path.StartsWith(appVirtualPath))
            {
                path = path.Substring(appVirtualPath.Length);
            }

            // Strip ~  before any others
            if (path.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(1);
            }

            if (path.StartsWith(Delimiter))
            {
                path = path.Substring(1);
            }

            // Strip root url
            if (path.StartsWith(this.rootContainerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(this.rootContainerUrl.Length);
            }

            // Strip default route
            if (path.StartsWith(Constants.DefaultMediaRoute, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(Constants.DefaultMediaRoute.Length);
            }

            // Strip container Prefix
            if (path.StartsWith(this.ContainerName, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(this.ContainerName.Length);
            }

            if (path.StartsWith(Delimiter))
            {
                path = path.Substring(1);
            }

            return path.TrimStart(Delimiter.ToCharArray()).TrimEnd(Delimiter.ToCharArray());
        }
    }
}
