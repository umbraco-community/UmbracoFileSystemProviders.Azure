// <copyright file="Parameter.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure.Installer.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// A parameter consisting of a key/value pair
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
