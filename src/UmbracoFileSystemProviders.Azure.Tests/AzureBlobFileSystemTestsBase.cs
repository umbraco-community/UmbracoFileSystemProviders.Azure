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
    using NUnit.Framework;

    /// <summary>
    /// The <see cref="AzureBlobFileSystem"/> tests base.
    /// </summary>
    [TestFixture]
    public class AzureBlobFileSystemTestsBase
    {
        /// <summary>
        /// Creates an instance of <see cref="AzureBlobFileSystem"/> set up for developmental testing.
        /// </summary>
        /// <param name="disableVirtualPathProvider">Whether to disable the virtual path provider.</param>
        /// <returns>
        /// The <see cref="AzureBlobFileSystem"/>.
        /// </returns>
        public AzureBlobFileSystem CreateAzureBlobFileSystem(bool disableVirtualPathProvider = false)
        {
            string containerName = "media";
            string rootUrl = "http://127.0.0.1:10000/devstoreaccount1/";
            string connectionString = "UseDevelopmentStorage=true";
            string maxDays = "30";
            string useDefaultRoute = "true";
            string usePrivateContainer = "false";

            Mock<ILogHelper> logHelper = new Mock<ILogHelper>();
            Mock<IMimeTypeResolver> mimeTypeHelper = new Mock<IMimeTypeResolver>();

            return new AzureBlobFileSystem(containerName, rootUrl, connectionString, maxDays, useDefaultRoute, usePrivateContainer)
            {
                FileSystem =
                {
                    LogHelper = logHelper.Object,
                    MimeTypeResolver = mimeTypeHelper.Object,
                    DisableVirtualPathProvider = disableVirtualPathProvider
                }
            };
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the full path.
        /// </summary>
        [Test]
        public void ResolveFullPath()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetFullPath("1010/media.jpg");

            // Assert
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the full path
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void ResolveFullPathPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetFullPath("media/1010/media.jpg");

            // Assert
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the relative path.
        /// </summary>
        [Test]
        public void ResolveRelativePath()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetRelativePath("1010/media.jpg");

            // Assert
            Assert.AreEqual("/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the relative path
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void ResolveRelativePathPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetRelativePath("media/1010/media.jpg");

            // Assert
            Assert.AreEqual("/media/1010/media.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly determines whether a file exists.
        /// </summary>
        [Test]
        public void TestFileExists()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));
            Assert.IsFalse(provider.FileExists("1010/media1.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly determines whether a file exists
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestFileExistsPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("media/1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));
            Assert.IsFalse(provider.FileExists("1010/media1.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly adds a file.
        /// </summary>
        [Test]
        public void TestAddFile()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly adds a file when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestAddFilePrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("media/1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly deletes a file.
        /// </summary>
        [Test]
        public void TestDeleteFile()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));

            // Act
            provider.DeleteFile("1010/media.jpg");

            // Assert
            Assert.IsFalse(provider.FileExists("1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly deletes a file when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestDeleteFilePrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("media/1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1010/media.jpg"));

            // Act
            provider.DeleteFile("media/1010/media.jpg");

            // Assert
            Assert.IsFalse(provider.FileExists("1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly returns the correct stream when opening a file.
        /// </summary>
        [Test]
        public void TestOpenFile()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            using (MemoryStream expected = new MemoryStream(Encoding.UTF8.GetBytes("test")))
            {
                // Act
                provider.AddFile("1010/media.jpg", expected);

                // Assert
                using (Stream actual = provider.OpenFile("1010/media.jpg"))
                {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        /// <summary>
        /// Asserts that the file system correctly returns the correct stream when opening a file
        /// when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestOpenFilePrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            using (MemoryStream expected = new MemoryStream(Encoding.UTF8.GetBytes("test")))
            {
                // Act
                provider.AddFile("media/1010/media.jpg", expected);

                // Assert
                using (Stream actual = provider.OpenFile("media/1010/media.jpg"))
                {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of directories in the
        /// correct format.
        /// </summary>
        [Test]
        public void TestGetDirectories()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);
            provider.AddFile("1011/media.jpg", Stream.Null);
            provider.AddFile("1012/media.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("/");

            // Assert
            string[] expected = { "1010", "1011", "1012" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of directories in the
        /// correct format when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestGetDirectoriesPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("media/1010/media.jpg", Stream.Null);
            provider.AddFile("media/1011/media.jpg", Stream.Null);
            provider.AddFile("media/1012/media.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("/");

            // Assert
            string[] expected = { "1010", "1011", "1012" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of files from the root
        /// container in the correct format.
        /// </summary>
        [Test]
        public void TestGetFilesFromRoot()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);
            provider.AddFile("1011/media.jpg", Stream.Null);
            provider.AddFile("1012/media.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/");

            // Assert
            string[] expected = { "1010/media.jpg", "1011/media.jpg", "1012/media.jpg" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of files from the root
        /// container in the correct format via a filtered request.
        /// </summary>
        [Test]
        public void TestGetFilesFromRootFiltered()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);
            provider.AddFile("1011/media.jpg", Stream.Null);
            provider.AddFile("1012/media.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/", "*.jpg");
            IEnumerable<string> alternate = provider.GetFiles("/", "*.png");

            // Assert
            string[] expected = { "1010/media.jpg", "1011/media.jpg", "1012/media.jpg" };
            Assert.IsTrue(expected.SequenceEqual(actual));
            Assert.IsFalse(expected.SequenceEqual(alternate));
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of files from a
        /// subfolder in the correct format.
        /// </summary>
        [Test]
        public void TestGetFiles()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/1010");

            // Assert
            string[] expected = { "1010/media.jpg" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        /// Asserts that the file system correctly returns the created date for a file.
        /// </summary>
        [Test]
        public void TestGetCreated()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Act
            DateTimeOffset original = provider.GetCreated("1010/media.jpg");

            // Assert
            Assert.AreNotEqual(original, DateTimeOffset.MinValue);

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);
            DateTimeOffset updated = provider.GetCreated("1010/media.jpg");

            // Assert
            Assert.AreEqual(original, updated);
        }

        /// <summary>
        /// Asserts that the file system correctly returns the last modified date for a file.
        /// </summary>
        [Test]
        public void TestGetLastModified()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Act
            DateTimeOffset original = provider.GetLastModified("1010/media.jpg");

            // Assert
            Assert.AreNotEqual(original, DateTimeOffset.MinValue);

            Thread.Sleep(TimeSpan.FromSeconds(1.1));

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);
            DateTimeOffset updated = provider.GetLastModified("1010/media.jpg");

            // Assert
            Assert.AreNotEqual(original, updated);
        }

        /// <summary>
        /// Asserts that the file system correctly deletes a directory.
        /// </summary>
        [Test]
        public void TestDeleteDirectory()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists("1010/"));

            // Act
            provider.DeleteDirectory("1010/");

            // Assert
            Assert.IsFalse(provider.DirectoryExists("1010/"));
            Assert.IsFalse(provider.FileExists("1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly creates a directory.
        /// </summary>
        [Test]
        public void TestValidDirectory()
        {
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("testvalid/test.txt", Stream.Null);
            Assert.IsTrue(provider.DirectoryExists("testvalid"));

            // Tidy up after test
            provider.DeleteDirectory("testvalid");
        }

        /// <summary>
        /// Asserts that the file system does not automatically create a directory.
        /// </summary>
        [Test]
        public void TestInvalidDirectory()
        {
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            Assert.IsFalse(provider.DirectoryExists("testinvalid/"));
        }

        /// <summary>
        /// Asserts that the file system correctly deletes a directory when the input has been prefixed.
        /// </summary>
        [Test]
        public void TestDeleteDirectoryPrefixed()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("media/1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists("media/1010/"));
            Assert.IsTrue(provider.FileExists("media/1010/media.jpg"));

            // Act
            provider.DeleteDirectory("media/1010/");

            // Assert
            Assert.IsFalse(provider.DirectoryExists("media/1010/"));
            Assert.IsFalse(provider.FileExists("media/1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly deletes a relative directory and file.
        /// </summary>
        [Test]
        public void TestDeleteDirectoryRelative()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            provider.AddFile("media/1010/media.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists("media/1010"));

            // Act
            provider.DeleteDirectory("\\media\\1010");

            // Assert
            Assert.IsFalse(provider.DirectoryExists("media/1010/"));
            Assert.IsFalse(provider.FileExists("media/1010/media.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of sub-directories in the
        /// correct format.
        /// </summary>
        [Test]
        public void TestGetSubDirectories()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("forms/form_123/kitty.jpg", Stream.Null);
            provider.AddFile("forms/form_123/dog.jpg", Stream.Null);
            provider.AddFile("forms/form_456/panda.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("forms");

            // Assert
            string[] expected = { "forms/form_123", "forms/form_456" };
            Assert.IsTrue(expected.SequenceEqual(actual));

            // Tidy up after test
            provider.DeleteDirectory("forms");
        }

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of sub-directories in the
        /// correct format.
        /// </summary>
        [Test]
        public void TestGetSubDirectoriesAndFiles()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();
            provider.AddFile("forms/form_123/kitty.jpg", Stream.Null);
            provider.AddFile("forms/form_123/dog.jpg", Stream.Null);
            provider.AddFile("forms/form_456/panda.jpg", Stream.Null);

            // Act
            var subfolders = provider.GetDirectories("forms");
            var actual = new List<string>();

            foreach (var folder in subfolders)
            {
                // Get files in subfolder and add to a single collection
                actual.AddRange(provider.GetFiles(folder));
            }

            // Assert
            string[] expected = { "forms/form_123/dog.jpg", "forms/form_123/kitty.jpg", "forms/form_456/panda.jpg" };
            Assert.IsTrue(expected.SequenceEqual(actual));

            // Tidy up after test
            provider.DeleteDirectory("forms");
        }
    }
}
