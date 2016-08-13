// <copyright file="InstallerStatus.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Installer.Enums
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Provides enumeration of the installer status codes.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstallerStatus
    {
        /// <summary>
        /// The install has completed sucessfully.
        /// </summary>
        Ok,

        /// <summary>
        /// Unable to save to the XDT file.
        /// </summary>
        SaveXdtError,

        /// <summary>
        /// Unable to save the configuration value.
        /// </summary>
        SaveConfigError,

        /// <summary>
        /// Unable to connect to the blob storage account.
        /// </summary>
        ConnectionError,

        /// <summary>
        /// Incompatible ImageProcessor.Web version.
        /// </summary>
        ImageProcessorWebCompatibility,

        /// <summary>
        /// Unable to save the ImageProcessor.Web configuration value.
        /// </summary>
        ImageProcessorWebConfigError
    }
}
