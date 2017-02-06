// <copyright file="AzureBlobFileSystemRelativeTests.cs" company="James Jackson-South and contributors">
// Copyright (c) James Jackson-South and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// The <see cref="AzureBlobFileSystem"/> relative tests.
    /// </summary>
    [TestFixture]
    public class AzureBlobFileSystemRelativeTests : AzureBlobFileSystemTestsBase
    {
        /// <summary>
        /// Asserts that the file system correctly resolves the url.
        /// </summary>
        [Test]
        public void ResolveUrl()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetUrl("1010/media.jpg");

            // Assert
            Assert.AreEqual("/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the url
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void ResolveUrlPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetUrl("media/1010/media.jpg");

            // Assert
            Assert.AreEqual("/media/1010/media.jpg", actual);
        }
    }
}
