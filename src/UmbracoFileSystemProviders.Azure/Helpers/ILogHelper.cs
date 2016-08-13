// <copyright file="ILogHelper.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System;

    /// <summary>
    /// Provides methods for logging messages within the application.
    /// </summary>
    public interface ILogHelper
    {
        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled.
        /// Use this method to avoid calling any long-running methods such as "ToDebugString" if
        /// logging is disabled.
        /// </summary>
        /// <typeparam name="T">The type of class the message is associated with.</typeparam>
        /// <param name="generateMessage">The generate message delegate.</param>
        void Info<T>(Func<string> generateMessage);

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled.
        /// </summary>
        /// <param name="callingType">The calling <see cref="Type"/>.</param>
        /// <param name="generateMessage">The generate message delegate.</param>
        void Info(Type callingType, Func<string> generateMessage);

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled.
        /// </summary>
        /// <typeparam name="T">The type of class the message is associated with.</typeparam>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        void Info<T>(string generateMessageFormat, params Func<object>[] formatItems);

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled.
        /// </summary>
        /// <param name="type">The calling <see cref="Type"/>.</param>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems);

        /// <summary>
        /// Adds an error message to the log.
        /// </summary>
        /// <typeparam name="T">The type of class the message is associated with.</typeparam>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The <see cref="Exception"/> containing additional information.</param>
        void Error<T>(string message, Exception exception);

        /// <summary>
        /// Adds an error message to the log.
        /// </summary>
        /// <param name="callingType">The calling <see cref="Type"/>.</param>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The <see cref="Exception"/> containing additional information.</param>
        void Error(Type callingType, string message, Exception exception);
    }
}
