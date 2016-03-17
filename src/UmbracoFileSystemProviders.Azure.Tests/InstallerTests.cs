// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerTests.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South and contributors.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using System.Linq;
    using NUnit.Framework;

    using Installer;

    [TestFixture]
    public class InstallerTests
    {
        [Test]
        public void CheckXdtFirstParameterKey()
        {
            var parameters = InstallerController.GetParametersFromXml("FileSystemProviders.config.install.xdt");
            Assert.AreEqual("containerName", parameters.First().Key);
        }

        [Test]
        public void CheckXdtNumberOfParameters()
        {
            var parameters = InstallerController.GetParametersFromXml("FileSystemProviders.config.install.xdt");
            Assert.AreEqual(5, parameters.Count());
        }

        [Test]
        public void CheckUpgradeRootUrlParameter()
        {
            var parameters = InstallerController.GetParametersFromXdt("FileSystemProviders.config.install.xdt", "FileSystemProviders.upgrade.config");
            Assert.AreEqual("http://existing123456789.blob.core.windows.net/", parameters.Single(k => k.Key == "rootUrl").Value);
        }

        [Test]
        public void CheckNewInstallDefaultConfig()
        {
            var parameters = InstallerController.GetParametersFromXdt("FileSystemProviders.config.install.xdt", "FileSystemProviders.default.config");
            Assert.AreEqual("http://[myAccountName].blob.core.windows.net/", parameters.Single(k => k.Key == "rootUrl").Value);
        }

    }
}
