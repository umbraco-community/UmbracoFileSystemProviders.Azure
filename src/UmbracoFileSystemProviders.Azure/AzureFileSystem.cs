// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureFileSystem.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South and contributors.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   A singleton class for communicating with Azure Blob Storage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using global::Umbraco.Core.IO;

    /// <summary>
    /// A singleton class for communicating with Azure Blob Storage.
    /// </summary>
    internal class AzureFileSystem : IFileSystem
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
        /// Our object to lock against during initialization.
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// The singleton instance of <see cref="AzureFileSystem"/>.
        /// </summary>
        private static Dictionary<string, AzureFileSystem> fileSystems;

        /// <summary>
        /// The container name.
        /// </summary>
        private readonly string containerName;

        /// <summary>
        /// The root url.
        /// </summary>
        private readonly string rootUrl;

        /// <summary>
        /// The cloud media blob container.
        /// </summary>
        private readonly CloudBlobContainer cloudBlobContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="containerName"/> is null or whitespace.
        /// </exception>
        private AzureFileSystem(string containerName, string rootUrl, string connectionString, int maxDays)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException("containerName");
            }

            this.DisableVirtualPathProvider = ConfigurationManager.AppSettings[DisableVirtualPathProviderKey] != null
                                              && ConfigurationManager.AppSettings[DisableVirtualPathProviderKey]
                                             .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            bool useEmulator = ConfigurationManager.AppSettings[UseStorageEmulatorKey] != null
                               && ConfigurationManager.AppSettings[UseStorageEmulatorKey]
                                                      .Equals("true", StringComparison.InvariantCultureIgnoreCase);

            CloudStorageAccount cloudStorageAccount;
            if (useEmulator)
            {
                cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                rootUrl = cloudStorageAccount.BlobStorageUri.PrimaryUri.AbsoluteUri;
            }
            else
            {
                cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            }

            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            this.cloudBlobContainer = CreateContainer(cloudBlobClient, containerName, BlobContainerPublicAccessType.Blob);

            if (!rootUrl.Trim().EndsWith("/"))
            {
                rootUrl = rootUrl.Trim() + "/";
            }

            this.rootUrl = rootUrl + containerName + "/";
            this.containerName = containerName;
            this.MaxDays = maxDays;

            this.LogHelper = new WrappedLogHelper();
            this.MimeTypeResolver = new MimeTypeResolver();
        }

        /// <summary>
        /// Gets or sets the log helper.
        /// </summary>
        public ILogHelper LogHelper { get; set; }

        /// <summary>
        /// Gets or sets the MIME type resolver.
        /// </summary>
        public IMimeTypeResolver MimeTypeResolver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the virtual path provider.
        /// </summary>
        public bool DisableVirtualPathProvider { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of days to cache blob items for in the browser.
        /// </summary>
        public int MaxDays { get; set; }

        /// <summary>
        /// Returns a singleton instance of the <see cref="AzureFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <returns>The <see cref="AzureFileSystem"/></returns>
        public static AzureFileSystem GetInstance(string containerName, string rootUrl, string connectionString, string maxDays)
        {
            lock (Locker)
            {
                if (fileSystems == null)
                {
                    fileSystems = new Dictionary<string, AzureFileSystem>();
                }

                if (!fileSystems.ContainsKey(containerName) || fileSystems[containerName] == null)
                { 
                    int max;
                    if (!int.TryParse(maxDays, out max))
                    {
                        max = 365;
                    }

                    fileSystems[containerName] = new AzureFileSystem(containerName, rootUrl, connectionString, max);
                }

                return fileSystems[containerName];
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
            CloudBlockBlob blockBlob = this.GetBlockBlobReference(path);
            bool exists = blockBlob.Exists();
            DateTimeOffset created = DateTimeOffset.MinValue;

            if (!overrideIfExists && exists)
            {
                InvalidOperationException error = new InvalidOperationException(string.Format("File already exists at {0}", blockBlob.Uri));
                this.LogHelper.Error<AzureBlobFileSystem>(string.Format("File already exists at {0}", path), error);
                return;
            }

            try
            {
                if (exists)
                {
                    // Ensure original created date is preserved.
                    blockBlob.FetchAttributes();
                    if (blockBlob.Metadata.ContainsKey("CreatedDate"))
                    {
                        // We store the creation date in meta data.
                        created = DateTime.Parse(blockBlob.Metadata["CreatedDate"], CultureInfo.InvariantCulture).ToUniversalTime();
                    }
                }

                blockBlob.UploadFromStream(stream);

                string contentType = this.MimeTypeResolver.Resolve(path);

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    blockBlob.Properties.ContentType = contentType;
                }

                blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", this.MaxDays * 86400);
                blockBlob.SetProperties();

                if (created == DateTimeOffset.MinValue)
                {
                    created = DateTimeOffset.UtcNow;
                }

                // Store the creation date in meta data.
                if (blockBlob.Metadata.ContainsKey("CreatedDate"))
                {
                    blockBlob.Metadata["CreatedDate"] = created.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    blockBlob.Metadata.Add("CreatedDate", created.ToString(CultureInfo.InvariantCulture));
                }
                blockBlob.SetMetadata();
            }
            catch (Exception ex)
            {
                this.LogHelper.Error<AzureBlobFileSystem>(string.Format("Unable to upload file at {0}", path), ex);
            }
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">The path to the given file.</param>
        /// <param name="stream">The <see cref="Stream"/> containing the file contents.</param>
        public void AddFile(string path, Stream stream)
        {
            this.AddFile(path, stream, true);
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
            path = this.FixPath(path);

            if (!this.DirectoryExists(path))
            {
                return;
            }

            CloudBlobDirectory directory = this.GetDirectoryReference(path);

            IEnumerable<CloudBlockBlob> blobs = directory.ListBlobs().OfType<CloudBlockBlob>();

            if (recursive)
            {
                foreach (CloudBlockBlob blockBlob in blobs)
                {
                    try
                    {
                        blockBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                    }
                    catch (Exception ex)
                    {
                        this.LogHelper.Error<AzureBlobFileSystem>(string.Format("Unable to delete directory at {0}", path), ex);
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
            this.DeleteDirectory(path, false);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to remove.</param>
        public void DeleteFile(string path)
        {
            CloudBlockBlob blockBlob = this.GetBlockBlobReference(path);

            try
            {
                blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (Exception ex)
            {
                this.LogHelper.Error<AzureBlobFileSystem>(string.Format("Unable to delete file at {0}", path), ex);
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
            return this.GetDirectories(path).Any();
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
            return this.GetBlockBlobReference(path).Exists();
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
            CloudBlockBlob blockBlob = this.GetBlockBlobReference(path);

            // Populate the blob's attributes.
            blockBlob.FetchAttributes();
            if (blockBlob.Metadata.ContainsKey("CreatedDate"))
            {
                // We store the creation date in meta data.
                return DateTimeOffset.Parse(blockBlob.Metadata["CreatedDate"], CultureInfo.InvariantCulture).ToUniversalTime();
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
            CloudBlobDirectory directory = this.GetDirectoryReference(path);

            IEnumerable<IListBlobItem> blobs = directory.ListBlobs();

            // Always get last segment for media sub folder simulation. E.g 1001, 1002
            return blobs.Select(cd =>
                                cd.Uri.Segments[cd.Uri.Segments.Length - 1].Split(Delimiter.ToCharArray())[0]);
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
            IEnumerable<IListBlobItem> blobs = this.cloudBlobContainer.ListBlobs(this.FixPath(path), true);
            return blobs.OfType<CloudBlockBlob>().Select(cd =>
                {
                    string url = cd.Uri.AbsoluteUri;

                    if (filter.Equals("*.*", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return url.Substring(this.rootUrl.Length);
                    }

                    // Filter by name.
                    filter = filter.TrimStart('*');
                    if (url.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return url.Substring(this.rootUrl.Length);
                    }

                    return null;
                });
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
            CloudBlockBlob blockBlob = this.GetBlockBlobReference(path);
            blockBlob.FetchAttributes();
            return blockBlob.Properties.LastModified.GetValueOrDefault();
        }

        /// <summary>
        /// Returns the relative path to the media item.
        /// </summary>
        /// <param name="fullPathOrUrl">The full path or url.</param>
        /// <returns>
        /// The <see cref="string"/> representing the relative path.
        /// </returns>
        public string GetRelativePath(string fullPathOrUrl)
        {
            return this.ResolveUrl(fullPathOrUrl, true);
        }

        /// <summary>
        /// Returns the url to the media item.
        /// </summary>
        /// <remarks>If the virtual path provider is enabled this returns a relative url.</remarks>
        /// <param name="path">The path to return the url for.</param>
        /// <returns>
        /// <see cref="string"/>.
        /// </returns>
        public string GetUrl(string path)
        {
            if (this.DisableVirtualPathProvider)
            {
                return this.ResolveUrl(path, false);
            }

            return this.ResolveUrl(path, true);
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> containing the contains of the given file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="Stream"/>.
        /// </returns>
        public Stream OpenFile(string path)
        {
            // TODO: Caching?
            CloudBlockBlob blockBlob = this.GetBlockBlobReference(path);

            if (!blockBlob.Exists())
            {
                this.LogHelper.Info<AzureBlobFileSystem>(string.Format("No file exists at {0}.", path));
                return null;
            }

            MemoryStream stream = new MemoryStream();
            blockBlob.DownloadToStream(stream);

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }

        /// <summary>
        /// Returns the media container, creating a new one if none exists.
        /// </summary>
        /// <param name="cloudBlobClient"><see cref="CloudBlobClient"/> where the container is stored.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessType"><see cref="BlobContainerPublicAccessType"/> indicating the access permissions.</param>
        /// <returns>The <see cref="CloudBlobContainer"/></returns>
        private static CloudBlobContainer CreateContainer(CloudBlobClient cloudBlobClient, string containerName, BlobContainerPublicAccessType accessType)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = accessType });
            return container;
        }

        /// <summary>
        /// Gets a reference to the block blob matching the given path.
        /// </summary>
        /// <param name="path">The path to the blob.</param>
        /// <returns>
        /// The <see cref="CloudBlockBlob"/> reference.
        /// </returns>
        private CloudBlockBlob GetBlockBlobReference(string path)
        {
            string blobPath = this.FixPath(path);
            return this.cloudBlobContainer.GetBlockBlobReference(blobPath);
        }

        /// <summary>
        /// Gets a reference to the directory matching the given path.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>
        /// The <see cref="CloudBlockBlob"/> reference.
        /// </returns>
        private CloudBlobDirectory GetDirectoryReference(string path)
        {
            string blobPath = this.FixPath(path);
            return this.cloudBlobContainer.GetDirectoryReference(blobPath);
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
            // First create the full url
            Uri url = new Uri(new Uri(this.rootUrl, UriKind.Absolute), this.FixPath(path));

            if (!relative)
            {
                return url.AbsoluteUri;
            }

            int index = url.AbsolutePath.IndexOf(this.containerName, StringComparison.Ordinal) - 1;
            string relativePath = url.AbsolutePath.Substring(index);
            return relativePath;
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

            if (path.StartsWith(Delimiter))
            {
                path = path.Substring(1);
            }

            // Strip root url
            if (path.StartsWith(this.rootUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(this.rootUrl.Length);
            }

            // Strip container Prefix
            if (path.StartsWith(this.containerName, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(this.containerName.Length);
            }

            if (path.StartsWith(Delimiter))
            {
                path = path.Substring(1);
            }

            return path.Replace("\\", Delimiter).TrimStart(Delimiter.ToCharArray()).TrimEnd(Delimiter.ToCharArray());
        }
    }
}
