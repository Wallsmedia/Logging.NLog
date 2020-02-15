// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright (c) Wallsmedia 2019-2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// NLog Logger Provider for Microsoft.Extensions.Logging.

using System;
using System.Text;
using WrappedNLog = NLog;

namespace Microsoft.Extensions.Logging.NLog
{

    /// <summary>
    /// A logger that writes messages to NLog Logger.
    /// </summary>
    public class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {

        /// <summary>
        /// The logging category name.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// The base NLog logger instance.
        /// </summary>
        public WrappedNLog.Logger Logger { get; }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="nlogger"></param>
        public static implicit operator WrappedNLog.Logger(NLogLogger nlogger)
        {
            return nlogger.Logger;
        }

        [ThreadStatic]
        private StringBuilder _logBuilder;

        private IExternalScopeProvider _scopeProvider;
        private NLogLoggerSettings _settings;

        /// <summary>
        /// Constructs the NLog logger.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="nLogSettings">The NLog Logger Provider settings.</param>
        /// <param name="externalScopeProvider">The scope data provider.</param>
        public NLogLogger(string categoryName, NLogLoggerSettings nLogSettings, IExternalScopeProvider externalScopeProvider)
        {
            CategoryName = categoryName;
            _settings = nLogSettings ?? throw new ArgumentNullException(nameof(nLogSettings));
            _scopeProvider = externalScopeProvider ?? throw new ArgumentNullException(nameof(externalScopeProvider));
            Logger = WrappedNLog.LogManager.GetLogger(categoryName);
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return _scopeProvider?.Push(state);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            if (logLevel == Microsoft.Extensions.Logging.LogLevel.None)
            {
                return false;
            }

            return IsEnabled(logLevel, _settings);
        }

        /// <summary>
        /// Checks if the given <paramref name="level"/> is enabled.
        /// </summary>
        /// <param name="nLogSettings">The memory logger settings</param>
        /// <param name="level">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level, NLogLoggerSettings nLogSettings)
        {
            if (nLogSettings.MinLevel != null && level < nLogSettings.MinLevel)
            {
                return false;
            }

            if (nLogSettings.Filter != null)
            {
                return nLogSettings.Filter(CategoryName, level);
            }

            return true;
        }

        /// <summary>
        /// Writes a log entry.
        /// Usually it has not been used directly.
        /// There are lots of extension methods from "Microsoft.Extensions.Logging" nuget package.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>v
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nLogLogLevel = MapMsLogLevelToNLog(logLevel);
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var message = formatter(state, exception);

            if (_settings.IncludeScopes)
            {
                string scopeString = String.Empty;
                int count = GetScopeInformation(logBuilder, _scopeProvider);
                if (count > 0)
                {
                    logBuilder.Append(message);
                    message = logBuilder.ToString();
                }
            }

            WrappedNLog.LogEventInfo logEventInfo = WrappedNLog.LogEventInfo.Create(nLogLogLevel, Logger.Name, exception, null, message);

            Logger.Log(logEventInfo);

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        /// <summary>
        /// Maps <see cref="Microsoft.Extensions.Logging.LogLevel"/> to <see cref="WrappedNLog.LogLevel"/>.
        /// </summary>
        /// <param name="logLevel">level to be converted.</param>
        /// <returns>The mapped value of <see cref="WrappedNLog.LogLevel"/>.</returns>
        private WrappedNLog.LogLevel MapMsLogLevelToNLog(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return WrappedNLog.LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return WrappedNLog.LogLevel.Debug;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return WrappedNLog.LogLevel.Info;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return WrappedNLog.LogLevel.Warn;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return WrappedNLog.LogLevel.Error;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return WrappedNLog.LogLevel.Fatal;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    return WrappedNLog.LogLevel.Off;
                default:
                    return WrappedNLog.LogLevel.Debug;
            }
        }

        /// <summary>
        /// Generates the string representation of a scope.
        /// </summary>
        /// <param name="stringBuilder">The output scope in the builder.</param>
        /// <param name="scopeProvider">The scope data provider.</param>
        /// <returns>The length of the scope.</returns>
        private int GetScopeInformation(StringBuilder stringBuilder, IExternalScopeProvider scopeProvider)
        {
            var initialLength = stringBuilder.Length;
            if (scopeProvider != null)
            {
                scopeProvider.ForEachScope(
                    callback: (scope, state) =>
                    {
                        var (builder, length) = state;
                        var first = length == builder.Length;
                        builder.Append(first ? "=> " : " => ").Append(scope);
                    },
                    state: (stringBuilder, initialLength));

                if (stringBuilder.Length > initialLength)
                {
                    stringBuilder.Append(" |");
                }
            }
            return stringBuilder.Length - initialLength;
        }
    }
}
