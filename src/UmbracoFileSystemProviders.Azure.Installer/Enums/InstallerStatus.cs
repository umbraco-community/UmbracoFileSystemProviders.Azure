// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerStatus.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved. Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Our.Umbraco.FileSystemProviders.Azure.Installer.Enums
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstallerStatus
    {
        Ok,
        SaveXdtError,
        SaveConfigError,
        ConnectionError,
        ImageProcessorWebCompatibility
    }
}
