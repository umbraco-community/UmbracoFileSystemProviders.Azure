// <copyright file="MimeTypeResolver.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System.Web;

    /// <summary>
    /// The default MIME type resolver.
    /// </summary>
    public class MimeTypeResolver : IMimeTypeResolver
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
        public string Resolve(string filename)
        {
            return MimeMapping.GetMimeMapping(filename);
        }
    }
}
