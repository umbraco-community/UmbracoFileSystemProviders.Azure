// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South and contributors.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Constant strings for use within the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Our.Umbraco.FileSystemProviders.Azure
{
    /// <summary>
    /// Constant strings for use within the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The configuration setting constants.
        /// </summary>
        public static class Configuration
        {
            /// <summary>
            /// The configuration key for disabling the virtual path provider.
            /// </summary>
            public const string DisableVirtualPathProviderKey = "AzureBlobFileSystem.DisableVirtualPathProvider";

            /// <summary>
            /// The configuration key for enabling the storage emulator.
            /// </summary>
            public const string UseStorageEmulatorKey = "AzureBlobFileSystem.UseStorageEmulator";
        }
    }
}
