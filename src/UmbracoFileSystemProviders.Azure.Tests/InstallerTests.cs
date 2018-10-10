// <copyright file="InstallerTests.cs" company="James Jackson-South and contributors">
// Copyright (c) James Jackson-South and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Installer.Models;
    using Installer;
    using NUnit.Framework;

    /// <summary>
    /// Tests the Umbraco installer
    /// </summary>
    [TestFixture]
    public class InstallerTests
    {
        /// <summary>
        /// Asserts that the first parameter is correct.
        /// </summary>
        [Test]
        public void CheckXdtFirstParameterKey()
        {
            IEnumerable<Parameter> parameters = InstallerController.GetParametersFromXml("..\\..\\build\\transforms\\FileSystemProviders.config.install.xdt");
            Assert.AreEqual("containerName", parameters.First().Key);
        }

        /// <summary>
        /// Asserts that the parameter count is correct.
        /// </summary>
        [Test]
        public void CheckXdtNumberOfParameters()
        {
            IEnumerable<Parameter> parameters = InstallerController.GetParametersFromXml("..\\..\\build\\transforms\\FileSystemProviders.config.install.xdt");
            Assert.AreEqual(6, parameters.Count());
        }

        /// <summary>
        /// Asserts that the "rootUrl" parameter is correct on upgrade.
        /// </summary>
        [Test]
        public void CheckUpgradeRootUrlParameter()
        {
            IEnumerable<Parameter> parameters = InstallerController.GetParametersFromXdt("..\\..\\build\\transforms\\FileSystemProviders.config.install.xdt", "FileSystemProviders.upgrade.config");
            Assert.AreEqual("http://existing123456789.blob.core.windows.net/", parameters.Single(k => k.Key == "rootUrl").Value);
        }

        /// <summary>
        /// Asserts that the "rootUrl" parameter is correct.
        /// </summary>
        [Test]
        public void CheckNewInstallDefaultConfig()
        {
            IEnumerable<Parameter> parameters = InstallerController.GetParametersFromXdt("..\\..\\build\\transforms\\FileSystemProviders.config.install.xdt", "FileSystemProviders.default.config");
            Assert.AreEqual("http://[myAccountName].blob.core.windows.net/", parameters.Single(k => k.Key == "rootUrl").Value);
        }
    }
}