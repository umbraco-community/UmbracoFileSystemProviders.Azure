// <copyright file="AzureBlobFileSystem.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;

    using global::Umbraco.Core.IO;

    /// <summary>
    /// The azure file system.
    /// </summary>
    public class AzureBlobFileSystem : IFileSystem
    {
        /// <summary>
        /// The configuration key for determining the connection string.
        /// </summary>
        private const string ConnectionStringKey = Constants.Configuration.ConnectionStringKey;

        /// <summary>
        /// The configuration key for determining the container name.
        /// </summary>
        private const string ContainerNameKey = Constants.Configuration.ContainerNameKey;

        /// <summary>
        /// The configuration key for determining the root url.
        /// </summary>
        private const string RootUrlKey = Constants.Configuration.RootUrlKey;

        /// <summary>
        /// The configuration key for determining the maximum days to cache values.
        /// </summary>
        private const string MaxDaysKey = Constants.Configuration.MaxDaysKey;

        /// <summary>
        /// The configuration key for determining whether the path provider should use the default root.
        /// </summary>
        private const string UseDefaultRootKey = Constants.Configuration.UseDefaultRouteKey;

        /// <summary>
        /// The configuration key for determining whether the container should be private.
        /// </summary>
        private const string UsePrivateContainerKey = Constants.Configuration.UsePrivateContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        public AzureBlobFileSystem(string containerName, string rootUrl, string connectionString)
            : this(containerName, rootUrl, connectionString, "365", "true", "false")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        public AzureBlobFileSystem(string containerName, string rootUrl, string connectionString, string maxDays)
            : this(containerName, rootUrl, connectionString, maxDays, "true", "false")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="useDefaultRoute">Whether to use the default "media" route in the url independent of the blob container.</param>
        public AzureBlobFileSystem(string containerName, string rootUrl, string connectionString, string maxDays, string useDefaultRoute)
            : this(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, "false")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="useDefaultRoute">Whether to use the default "media" route in the url independent of the blob container.</param>
        /// <param name="usePrivateContainer">blob container can be private (no direct access) or public (direct access possible, default)</param>
        public AzureBlobFileSystem(string containerName, string rootUrl, string connectionString, string maxDays, string useDefaultRoute, string usePrivateContainer)
        {
            this.FileSystem = AzureFileSystem.GetInstance(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, usePrivateContainer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class
        /// from values in application settings.
        /// </summary>
        /// <param name="alias">The alias of the provider</param>
        public AzureBlobFileSystem(string alias)
        {
            string connectionString = ConfigurationManager.AppSettings[$"{ConnectionStringKey}:{alias}"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                string rootUrl = ConfigurationManager.AppSettings[$"{RootUrlKey}:{alias}"];
                if (string.IsNullOrWhiteSpace(rootUrl))
                {
                    throw new InvalidOperationException("Azure Storage Root URL is not defined in application settings. The " + RootUrlKey + " property was not defined or is empty.");
                }

                string containerName = ConfigurationManager.AppSettings[$"{ContainerNameKey}:{alias}"];
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    containerName = "media";
                }

                string maxDays = ConfigurationManager.AppSettings[$"{MaxDaysKey}:{alias}"];
                if (string.IsNullOrWhiteSpace(maxDays))
                {
                    maxDays = "365";
                }

                string useDefaultRoute = ConfigurationManager.AppSettings[$"{UseDefaultRootKey}:{alias}"];
                if (string.IsNullOrWhiteSpace(useDefaultRoute))
                {
                    useDefaultRoute = "true";
                }

                string accessType = ConfigurationManager.AppSettings[$"{UsePrivateContainerKey}:{alias}"];
                if (string.IsNullOrWhiteSpace(accessType))
                {
                    accessType = "true";
                }

                this.FileSystem = AzureFileSystem.GetInstance(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, accessType);
            }
            else
            {
                throw new InvalidOperationException("Unable to retrieve the Azure Storage configuration from the application settings. " + ConnectionStringKey + " was not defined or is empty.");
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="AzureFileSystem"/> class.
        /// </summary>
        internal AzureFileSystem FileSystem { get; }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">
        /// The path to the given file.
        /// </param>
        /// <param name="stream">
        /// The <see cref="Stream"/> containing the file contents.
        /// </param>
        /// <param name="overrideIfExists">
        /// Whether to override the file if it already exists.
        /// </param>
        public void AddFile(string path, Stream stream, bool overrideIfExists)
        {
            this.FileSystem.AddFile(path, stream, overrideIfExists);
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">
        /// The path to the given file.
        /// </param>
        /// <param name="stream">
        /// The <see cref="Stream"/> containing the file contents.
        /// </param>
        public void AddFile(string path, Stream stream)
        {
            this.FileSystem.AddFile(path, stream);
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
            this.FileSystem.DeleteDirectory(path, recursive);
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        public void DeleteDirectory(string path)
        {
            this.FileSystem.DeleteDirectory(path, false);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to remove.</param>
        public void DeleteFile(string path)
        {
            this.FileSystem.DeleteFile(path);
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
            return this.FileSystem.DirectoryExists(path);
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
            return this.FileSystem.FileExists(path);
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
            return this.FileSystem.GetCreated(path);
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
            return this.FileSystem.GetDirectories(path);
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
            return this.FileSystem.GetFiles(path, filter);
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
            return this.FileSystem.GetFiles(path);
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
            return this.FileSystem.GetFullPath(path);
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
            return this.FileSystem.GetLastModified(path);
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
            return this.FileSystem.GetRelativePath(fullPathOrUrl);
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
            return this.FileSystem.GetUrl(path);
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
            return this.FileSystem.OpenFile(path);
        }
    }
}