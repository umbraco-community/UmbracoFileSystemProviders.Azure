// <copyright file="AzureBlobFileSystemAbsoluteTests.cs" company="James Jackson-South and contributors">
// Copyright (c) James Jackson-South and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// The <see cref="AzureBlobFileSystem"/> absolute tests.
    /// </summary>
    [TestFixture]
    public class AzureBlobFileSystemAbsoluteTests : AzureBlobFileSystemTestsBase
    {
        /// <summary>
        /// Asserts that the file system correctly resolves the url.
        /// </summary>
        [Test]
        public void ResolveUrl()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(true);

            // Act
            string actual = provider.GetUrl("1010/media.jpg");

            // Assert
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the url
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void ResolveUrlPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(true);

            // Act
            string actual = provider.GetUrl("media/1010/media.jpg");

            // Assert
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/media/1010/media.jpg", actual);
        }
    }
}
