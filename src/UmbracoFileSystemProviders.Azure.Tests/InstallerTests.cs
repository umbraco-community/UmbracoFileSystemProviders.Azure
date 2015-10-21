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
        public void CheckFirstParameterKey()
        {
            var parameters = InstallerController.GetParametersFromXdt("FileSystemProviders.config.install.xdt");
            Assert.AreEqual("containerName", parameters.First().Key);
        }

        [Test]
        public void CheckNumberOfParamters()
        {
            var parameters = InstallerController.GetParametersFromXdt("FileSystemProviders.config.install.xdt");
            Assert.AreEqual(4, parameters.Count());
        }

    }
}
