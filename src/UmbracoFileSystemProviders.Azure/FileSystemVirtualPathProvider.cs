// <copyright file="FileSystemVirtualPathProvider.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;
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

            // The standard HostingEnvironment.RegisterVirtualPathProvider(virtualPathProvider) method is ignored when
            // BuildManager.IsPrecompiledApp is true so we have to use reflection when registering the provider.
            if (!BuildManager.IsPrecompiledApp)
            {
                HostingEnvironment.RegisterVirtualPathProvider(provider);
            }
            else
            {
                // Gets the private _theHostingEnvironment reference.
                HostingEnvironment hostingEnvironmentInstance = (HostingEnvironment)typeof(HostingEnvironment)
                    .InvokeMember("_theHostingEnvironment", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

                if (hostingEnvironmentInstance == null)
                {
                    return;
                }

                // Get the static internal MethodInfo for RegisterVirtualPathProviderInternal method.
                MethodInfo methodInfo = typeof(HostingEnvironment)
                    .GetMethod("RegisterVirtualPathProviderInternal", BindingFlags.NonPublic | BindingFlags.Static);

                if (methodInfo == null)
                {
                    return;
                }

                // Invoke RegisterVirtualPathProviderInternal method with one argument which is the instance of our own provider.
                methodInfo.Invoke(hostingEnvironmentInstance, new object[] { provider });
            }
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

            // if Umbraco is running in a virtual path, the path then needs to be included in the prefix
            if (HttpRuntime.AppDomainAppVirtualPath != "/")
            {
                return HttpRuntime.AppDomainAppVirtualPath + prefix;
            }

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
