// <copyright file="FileSystemVirtualPathProviderTests.cs" company="James Jackson-South and contributors">
// Copyright (c) James Jackson-South and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using System;
    using System.IO;
    using System.Web.Hosting;
    using Azure;
    using global::Umbraco.Core.IO;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// The <see cref="FileSystemVirtualPathProvider"/> tests.
    /// </summary>
    [TestFixture]
    public class FileSystemVirtualPathProviderTests
    {
        /// <summary>
        /// Asserts that the file path prefix is correctly formatted.
        /// </summary>
        [Test]
        public void FilePathPrefixFormatted()
        {
            // Arrange/Act
            Mock<IFileSystem> fileProvider = new Mock<IFileSystem>();
            FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider(Constants.DefaultMediaRoute, new Lazy<IFileSystem>(() => fileProvider.Object));

            // Assert
            Assert.AreEqual(provider.PathPrefix, "/media/");
        }

        /// <summary>
        /// Asserts that the file path is correctly executed.
        /// </summary>
        [Test]
        public void FilePathShouldBeExecuted()
        {
            // Arrange
            Mock<IFileSystem> fileProvider = new Mock<IFileSystem>();
            FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider("media", new Lazy<IFileSystem>(() => fileProvider.Object));

            // Act
            VirtualFile result = provider.GetFile("~/media/1010/media.jpg");

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Asserts that the file path should be not ignored.
        /// </summary>
        [Test]
        public void FilePathShouldBeNotIgnored()
        {
            // Arrange
            Mock<IFileSystem> fileProvider = new Mock<IFileSystem>();
            FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider("media", new Lazy<IFileSystem>(() => fileProvider.Object));

            // Act
            VirtualFile result = provider.GetFile("~/styles/main.css");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Asserts that the provider should call <see cref="IFileSystem"/> OpenFile method.
        /// </summary>
        [Test]
        public void ProviderShouldCallFileSystemOpenFile()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Arrange
                Mock<IFileSystem> fileProvider = new Mock<IFileSystem>();
                fileProvider.Setup(p => p.OpenFile("1010/media.jpg")).Returns(stream);
                FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider("media", new Lazy<IFileSystem>(() => fileProvider.Object));

                // Act
                VirtualFile result = provider.GetFile("~/media/1010/media.jpg");

                // Assert
                using (Stream streamResult = result.Open())
                {
                    Assert.AreEqual(stream, streamResult);
                    fileProvider.Verify(p => p.OpenFile("1010/media.jpg"), Times.Once);
                }
            }
        }

        /// <summary>
        /// Asserts that the provider should call <see cref="IFileSystem"/> FileExists method.
        /// </summary>
        [Test]
        public void ProviderShouldCallFileSystemFileExists()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Arrange
                Mock<IFileSystem> fileProvider = new Mock<IFileSystem>();
                fileProvider.Setup(p => p.OpenFile("1010/media.jpg")).Returns(stream);
                FileSystemVirtualPathProvider provider = new FileSystemVirtualPathProvider("media", new Lazy<IFileSystem>(() => fileProvider.Object));

                // Act
                provider.FileExists("~/media/1010/media.jpg");

                // Assert
                fileProvider.Verify(p => p.FileExists("1010/media.jpg"), Times.Once);
            }
        }
    }
}
