// <copyright file="Helpers.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Installer
{
    using System;
    using System.Configuration;

    public static class Helpers
    {
        public static Version GetUmbracoVersion()
        {
            var umbracoVersion = new Version(ConfigurationManager.AppSettings["umbracoConfigurationStatus"]);
            return umbracoVersion;
        }
    }
}
