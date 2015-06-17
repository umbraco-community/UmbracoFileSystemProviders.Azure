// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemVirtualFile.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South and contributors.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents a file object in a virtual file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;
    using System.IO;
    using System.Web.Hosting;

    /// <summary>
    /// Represents a file object in a virtual file.
    /// </summary>
    internal class FileSystemVirtualFile : VirtualFile
    {
        /// <summary>
        /// The stream function delegate.
        /// </summary>
        private readonly Func<Stream> stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemVirtualFile"/> class.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="stream"/> is null.
        /// </exception>
        public FileSystemVirtualFile(string virtualPath, Func<Stream> stream)
            : base(virtualPath)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
        }

        /// <summary>
        /// Gets a value that indicates that this is a virtual resource that should be treated as a file.
        /// </summary>
        /// <returns>
        /// Always false. 
        /// </returns>
        public override bool IsDirectory
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>
        /// A read-only stream to the virtual file.
        /// </returns>
        public override Stream Open()
        {
            return this.stream();
        }
    }
}
