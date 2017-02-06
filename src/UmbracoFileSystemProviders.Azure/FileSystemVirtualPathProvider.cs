// <copyright file="FileSystemVirtualPathProvider.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Hosting;

    using global::Umbraco.Core.IO;

    /// <summary>
    /// Provides a set of methods that enable a Web application to retrieve
    /// resources from a virtual file system implementing <see cref="IFileSystem"/>.
    /// </summary>
    public class FileSystemVirtualPathProvider : VirtualPathProvider
    {
        /// <summary>
        /// The path prefix.
        /// </summary>
        private readonly string pathPrefix;

        /// <summary>
        /// The file system.
        /// </summary>
        private readonly Lazy<IFileSystem> fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemVirtualPathProvider"/> class.
        /// </summary>
        /// <param name="pathPrefix">
        /// The path prefix.
        /// </param>
        /// <param name="fileSystem">
        /// The file system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either argument is null.
        /// </exception>
        public FileSystemVirtualPathProvider(string pathPrefix, Lazy<IFileSystem> fileSystem)
        {
            if (string.IsNullOrEmpty(pathPrefix))
            {
                throw new ArgumentNullException(nameof(pathPrefix));
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            this.pathPrefix = this.FormatVirtualPathPrefix(pathPrefix);
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets the path prefix.
        /// </summary>
        public string PathPrefix => this.pathPrefix;

        /// <summary>
        /// Configures the virtual path provider.
        /// </summary>
        /// <param name="pathPrefix">
        /// The path prefix.
        /// </param>
        /// <typeparam name="TProviderTypeFilter">
        /// The provider type filter.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="pathPrefix"/> is null.
        /// </exception>
        public static void Configure<TProviderTypeFilter>(string pathPrefix = Constants.DefaultMediaRoute)
            where TProviderTypeFilter : FileSystemWrapper
        {
            if (string.IsNullOrEmpty(pathPrefix))
            {
                throw new ArgumentNullException(nameof(pathPrefix));
            }

            Lazy<IFileSystem> fileSystem = new Lazy<IFileSystem>(() => FileSystemProviderManager.Current.GetFileSystemProvider<TProviderTypeFilter>());
            FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider(pathPrefix, fileSystem);
            HostingEnvironment.RegisterVirtualPathProvider(provider);
        }

        /// <summary>
        /// Configures the virtual path provider for media.
        /// </summary>
        /// <param name="pathPrefix">
        /// The path prefix.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Resharper seems drunk.")]
        public static void ConfigureMedia(string pathPrefix = Constants.DefaultMediaRoute)
        {
            Configure<MediaFileSystem>(pathPrefix);
        }

        /// <summary>
        /// Gets a value that indicates whether a file exists in the virtual file system.
        /// </summary>
        /// <returns>
        /// true if the file exists in the virtual file system; otherwise, false.
        /// </returns>
        /// <param name="virtualPath">The path to the virtual file.</param>
        public override bool FileExists(string virtualPath)
        {
            string path = this.FormatVirtualPath(virtualPath);

            if (!path.StartsWith(this.pathPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return base.FileExists(virtualPath);
            }

            string fileSystemPath = this.RemovePathPrefix(path);
            return this.fileSystem.Value.FileExists(fileSystemPath);
        }

        /// <summary>
        /// Gets a virtual file from the virtual file system.
        /// </summary>
        /// <returns>
        /// A descendent of the <see cref="T:System.Web.Hosting.VirtualFile"/> class that represents a
        /// file in the virtual file system.
        /// </returns>
        /// <param name="virtualPath">The path to the virtual file.</param>
        public override VirtualFile GetFile(string virtualPath)
        {
            string path = this.FormatVirtualPath(virtualPath);
            if (!path.StartsWith(this.pathPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return base.GetFile(virtualPath);
            }

            string fileSystemPath = this.RemovePathPrefix(path);

            return new FileSystemVirtualFile(virtualPath, this.fileSystem, fileSystemPath);
        }

        /// <summary>
        /// Correctly formats the virtual path prefix.
        /// </summary>
        /// <param name="prefix">
        /// The prefix to format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the prefix.
        /// </returns>
        private string FormatVirtualPathPrefix(string prefix)
        {
            prefix = prefix.Replace("\\", "/");
            prefix = prefix.StartsWith("/") ? prefix : string.Concat("/", prefix);
            prefix = prefix.EndsWith("/") ? prefix : string.Concat(prefix, "/");
            return prefix;
        }

        /// <summary>
        /// Removes the path prefix from the given virtual path.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the virtual path.
        /// </returns>
        private string RemovePathPrefix(string virtualPath)
        {
            return virtualPath.Substring(this.pathPrefix.Length);
        }

        /// <summary>
        /// Correctly formats the virtual path.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path to format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the virtual path.
        /// </returns>
        private string FormatVirtualPath(string virtualPath)
        {
            return virtualPath.StartsWith("~")
                ? virtualPath.Substring(1)
                : virtualPath;
        }
    }
}
