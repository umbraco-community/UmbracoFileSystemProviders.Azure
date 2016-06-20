// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Our.Umbraco.FileSystemProviders.Azure.Installer.Models
{
    using Newtonsoft.Json;

    public class Parameter
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
