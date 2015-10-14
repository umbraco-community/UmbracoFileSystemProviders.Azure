using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Our.Umbraco.FileSystemProviders.Azure.Umbraco.Installer;

namespace Our.Umbraco.FileSystemProviders.Azure.Tests
{
    [TestFixture]
    public class InstallerTests
    {
        [Test]
        public void CheckFirstParameterKey()
        {
            var parameters = InstallerController.GetParametersFromXdt();
            Assert.AreEqual("containerName", parameters.First().Key);
        }

        [Test]
        public void CheckNumberOfParamters()
        {
            var parameters = InstallerController.GetParametersFromXdt();
            Assert.AreEqual(4, parameters.Count);
        }

    }
}
