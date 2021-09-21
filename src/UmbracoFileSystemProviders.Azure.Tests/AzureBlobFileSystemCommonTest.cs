// <copyright file="AzureBlobFileSystemCommonTest.cs" company="James Jackson-South and contributors">
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
    public class AzureBlobFileSystemCommonTest : AzureBlobFileSystemTestsBase
    {
#if SASContainerLevel || SASAccountLevel
        /// <summary>
        /// defines invalid connection string an and asserts FormatException exception
        /// </summary>
        [Test]
        public void InavlidSasTokenException()
        {
            if (this is AzureBlobFileSystemAbsoluteTests)
            {
                // Arrange, Act & Assert
                Assert.Throws<FormatException>(() => this.CreateAzureBlobFileSystem(connectionString: SASConnectionStringInvalid));
            }
        }
#endif

#if SASContainerLevel
        /// <summary>
        /// creating new container for container level SAS token and asserts exception
        /// </summary>
        [Test]
        public void InsuffientSasTokenPermissions()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(containerName: "xxx", useDefaultRoute: "false");

            // Act & Assert
            Assert.Throws<Microsoft.WindowsAzure.Storage.StorageException>(() => provider.AddFile("1y3jw4gd/image.jpg", Stream.Null));
        }
#elif SASAccountLevel
        /// <summary>
        /// creating new container for account level SAS token and asserts that the file system correctly determines whether a file exists.
        /// </summary>
        [Test]
        public void SuffientSasTokenPermissions()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(containerName: "xxx", useDefaultRoute: "false");

            // Act
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));
            Assert.IsFalse(provider.FileExists("1y3jw4gd/media1.jpg"));
        }
#endif

        /// <summary>
        /// Asserts that the file system correctly resolves the full path.
        /// </summary>
        [Test]
        public void ResolveFullPath()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            string actual = provider.GetFullPath("1y3jw4gd/image.jpg");

            // Assert
            Assert.AreEqual($"{this.RootUrl}{this.ContainerName}/1y3jw4gd/image.jpg", actual);
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
            string actual = provider.GetFullPath($"{this.ContainerName}/1y3jw4gd/image.jpg");

            // Assert
            Assert.AreEqual($"{this.RootUrl}{this.ContainerName}/1y3jw4gd/image.jpg", actual);
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
            string actual = provider.GetRelativePath("1y3jw4gd/image.jpg");

            // Assert
            Assert.AreEqual($"1y3jw4gd/image.jpg", actual);
        }

        /// <summary>
        /// Asserts that the file system correctly resolves the relative path when Umbraco is hosted in virtual path
        /// </summary>
        [Test]
        public void ResolveRelativePathWithAppVirtualPath()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(false, "/test");

            // Act
            string actual = provider.GetRelativePath("1y3jw4gd/image.jpg");

            // Assert
            Assert.AreEqual($"1y3jw4gd/image.jpg", actual);
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
            string actual = provider.GetRelativePath($"{this.ContainerName}/1y3jw4gd/image.jpg");

            // Assert
            Assert.AreEqual($"1y3jw4gd/image.jpg", actual);
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));
            Assert.IsFalse(provider.FileExists("1y3jw4gd/media1.jpg"));
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));
            Assert.IsFalse(provider.FileExists("1y3jw4gd/media1.jpg"));
        }

        /// <summary>
        /// Asserts that the file system correctly adds a file.
        /// </summary>
        [Test]
        public void TestAddFile()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = string.Format("{0}\\{1}", directory, "church.jpg");

            // Act
            using (FileStream fs = File.OpenRead(path))
            {
                provider.AddFile("1y3jw4gd/image.jpg", fs);
            }

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));

            // Act
            provider.DeleteFile("1y3jw4gd/image.jpg");

            // Assert
            Assert.IsFalse(provider.FileExists("1y3jw4gd/image.jpg"));
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.FileExists("1y3jw4gd/image.jpg"));

            // Act
            provider.DeleteFile($"{this.ContainerName}/1y3jw4gd/image.jpg");

            // Assert
            Assert.IsFalse(provider.FileExists("1y3jw4gd/image.jpg"));
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
                provider.AddFile("1y3jw4gd/image.jpg", expected);

                // Assert
                using (Stream actual = provider.OpenFile("1y3jw4gd/image.jpg"))
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
                provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", expected);

                // Assert
                using (Stream actual = provider.OpenFile($"{this.ContainerName}/1y3jw4gd/image.jpg"))
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);
            provider.AddFile("1011/image.jpg", Stream.Null);
            provider.AddFile("1012/image.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("/");

            // Assert
            string[] expected = { "1011", "1012", "1y3jw4gd" };
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);
            provider.AddFile($"{this.ContainerName}/1011/image.jpg", Stream.Null);
            provider.AddFile($"{this.ContainerName}/1012/image.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("/");

            // Assert
            string[] expected = { "1011", "1012", "1y3jw4gd" };
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);
            provider.AddFile("1011/image.jpg", Stream.Null);
            provider.AddFile("1012/image.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/");

            // Assert
            string[] expected = { "1011/image.jpg", "1012/image.jpg", "1y3jw4gd/image.jpg" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        /// Asserts that a invalid path parameter passed to GetFiles returns null
        /// </summary>
        [Test]
        public void TestGetFilesInvalidPath()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem();

            // Act
            IEnumerable<string> actual = provider.GetFiles("/somethingmissing", "*.jpg");

            // Assert
            Assert.IsEmpty(actual);
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);
            provider.AddFile("1011/image.jpg", Stream.Null);
            provider.AddFile("1012/image.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/", "*.jpg");
            IEnumerable<string> alternate = provider.GetFiles("/", "*.png");

            // Assert
            string[] expected = { "1011/image.jpg", "1012/image.jpg", "1y3jw4gd/image.jpg" };
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetFiles("/1y3jw4gd");

            // Assert
            string[] expected = { "1y3jw4gd/image.jpg" };
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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Act
            DateTimeOffset original = provider.GetCreated("1y3jw4gd/image.jpg");

            // Assert
            Assert.AreNotEqual(original, DateTimeOffset.MinValue);

            // Act
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);
            DateTimeOffset updated = provider.GetCreated("1y3jw4gd/image.jpg");

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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Act
            DateTimeOffset original = provider.GetLastModified("1y3jw4gd/image.jpg");

            // Assert
            Assert.AreNotEqual(original, DateTimeOffset.MinValue);

            Thread.Sleep(TimeSpan.FromSeconds(1.1));

            // Act
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);
            DateTimeOffset updated = provider.GetLastModified("1y3jw4gd/image.jpg");

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
            provider.AddFile("1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists("1y3jw4gd/"));

            // Act
            provider.DeleteDirectory("1y3jw4gd/");

            // Assert
            Assert.IsFalse(provider.DirectoryExists("1y3jw4gd/"));
            Assert.IsFalse(provider.FileExists("1y3jw4gd/image.jpg"));
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists($"{this.ContainerName}/1y3jw4gd/"));
            Assert.IsTrue(provider.FileExists($"{this.ContainerName}/1y3jw4gd/image.jpg"));

            // Act
            provider.DeleteDirectory($"{this.ContainerName}/1y3jw4gd/");

            // Assert
            Assert.IsFalse(provider.DirectoryExists($"{this.ContainerName}/1y3jw4gd/"));
            Assert.IsFalse(provider.FileExists($"{this.ContainerName}/1y3jw4gd/image.jpg"));
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
            provider.AddFile($"{this.ContainerName}/1y3jw4gd/image.jpg", Stream.Null);

            // Assert
            Assert.IsTrue(provider.DirectoryExists($"{this.ContainerName}/1y3jw4gd"));

            // Act
            provider.DeleteDirectory($"\\{this.ContainerName}\\1y3jw4gd");

            // Assert
            Assert.IsFalse(provider.DirectoryExists($"{this.ContainerName}/1y3jw4gd/"));
            Assert.IsFalse(provider.FileExists($"{this.ContainerName}/1y3jw4gd/image.jpg"));
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

        /// <summary>
        /// Asserts that the file system correctly returns a sequence of directories in the
        /// correct format when there are no subdirectories.
        /// </summary>
        [Test]
        public void TestGetSubDirectoriesNone()
        {
            // Arrange
            AzureBlobFileSystem provider = this.CreateAzureBlobFileSystem(containerName: "forms-data");
            provider.AddFile("forms/b5e2fab3-040d-4328-ac74-cf7f7ebe3918.json", Stream.Null);
            provider.AddFile("workflows/b5e2fab3-040d-4328-ac74-cf7f7ebe3918.json", Stream.Null);

            // Act
            IEnumerable<string> actual = provider.GetDirectories("forms");

            // Assert
            string[] expected = { };
            Assert.IsTrue(expected.SequenceEqual(actual));

            // Tidy up after test
            provider.DeleteDirectory("forms");
        }
    }
}
