// <copyright file="AzureBlobFileSystemTestsBase.cs" company="James Jackson-South and contributors">
// Copyright (c) James Jackson-South and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Moq;
    using NUnit.Compatibility;
    using NUnit.Framework;

    /// <summary>
    /// The <see cref="AzureBlobFileSystem"/> tests base.
    /// </summary>
    [TestFixture]
    public class AzureBlobFileSystemTestsBase
    {
        /// <summary>
        /// Constant sas token definition
        /// </summary>
        public const string SASConnectionStringInvalid = "SharedAccessSignature=sv=";

        // private const string SASrootUrl = "https://[accountName].blob.core.windows.net/";
#if SASContainerLevel

        /// <summary>
        /// Constant sas token definition
        /// </summary>
        private const string SASContainerLevelConnectionString = "BlobEndpoint=https://[accountName].blob.core.windows.net/;SharedAccessSignature=[sasQueryStringWithoutLeadingQuestionMark]";
#elif SASAccountLevel
        /// <summary>
        /// Constant sas token definition
        /// </summary>
        private const string SASServiceLevelConnectionString = "[sasAccountConnectionString]";
#endif

        /// <summary>
        /// Gets name of Azure storage root
        /// </summary>
        public string RootUrl { get; private set; }

        /// <summary>
        /// Gets name of Azure Blob container
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="AzureBlobFileSystem"/> set up for developmental testing.
        /// </summary>
        /// <param name="disableVirtualPathProvider">Whether to disable the virtual path provider.</param>
        /// <param name="appVirtualPath">Mocked virtual path of application</param>
        /// <param name="connectionString">Optional input of custom connection string e.g. sas token</param>
        /// <param name="containerName">Optional input of container name</param>
        /// <param name="useDefaultRoute">if true containerName is ignored and Umbraco default "media" is used</param>
        /// <returns>
        /// The <see cref="AzureBlobFileSystem"/>.
        /// </returns>
        public AzureBlobFileSystem CreateAzureBlobFileSystem(bool disableVirtualPathProvider = false, string appVirtualPath = "", string connectionString = null, string containerName = null, string useDefaultRoute = null)
        {
            string maxDays = "30";
            string usePrivateContainer = "false";
            if (string.IsNullOrEmpty(useDefaultRoute))
            {
                useDefaultRoute = "true";
            }

            this.ContainerName = string.IsNullOrEmpty(containerName) ? "media" : containerName;
            this.RootUrl = "http://127.0.0.1:10000/devstoreaccount1/";

#if SASContainerLevel
            connectionString = connectionString ?? SASContainerLevelConnectionString;
            this.RootUrl = SASrootUrl;
#elif SASAccountLevel
            connectionString = connectionString ?? SASServiceLevelConnectionString;
            this.RootUrl = SASrootUrl;
#else
            connectionString = connectionString ?? "UseDevelopmentStorage=true";
#endif
            Mock<IMimeTypeResolver> mimeTypeHelper = new Mock<IMimeTypeResolver>();
            
            if (mimeTypeHelper.Object == null)
            {
                throw new Exception("mimeTypeHelper.Object null");
            }

            return new AzureBlobFileSystem(this.ContainerName, this.RootUrl, connectionString, maxDays, useDefaultRoute, usePrivateContainer)
            {
                FileSystem =
                {
                    MimeTypeResolver = mimeTypeHelper.Object,
                    DisableVirtualPathProvider = disableVirtualPathProvider,
                    ApplicationVirtualPath = appVirtualPath
                }
            };
        }
    }
}
