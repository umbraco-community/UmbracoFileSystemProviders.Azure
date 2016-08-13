// <copyright file="IMimeTypeResolver.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    /// <summary>
    /// Provides methods for resolving MIME types.
    /// </summary>
    public interface IMimeTypeResolver
    {
        /// <summary>
        /// Returns the correct MIME mapping for the given file name.
        /// </summary>
        /// <param name="filename">
        /// The file name that is used to determine the MIME type.
        /// </param>
        /// <returns>
        /// <see cref="string"/>.
        /// </returns>
        string Resolve(string filename);
    }
}
