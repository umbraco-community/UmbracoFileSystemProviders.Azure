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
            string actual = provider.GetUrl("110/image.jpg");

            // Assert
            Assert.AreEqual($"/{this.ContainerName}/110/image.jpg", actual);
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
            string actual = provider.GetUrl($"{this.ContainerName}/110/image.jpg");

            // Assert
            Assert.AreEqual($"/{this.ContainerName}/110/image.jpg", actual);
        }
    }
}
