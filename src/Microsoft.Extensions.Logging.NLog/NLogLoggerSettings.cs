// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright (c) Wallsmedia 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// NLog Logger Provider for Microsoft.Extensions.Logging.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Extensions.Logging.NLog
{
    /// <summary>
    /// The NLog Logger Provider settings for <see cref="NLogLogger"/>.
    /// </summary>
    [DataContract]
    public class NLogLoggerSettings
    {
        /// <summary>
        /// The list of accepted category names. Which is the logger pattern name for NLog. If it is empty all categories will be accepted.
        /// The name can have the wild char '*' at the end, which means "start with"; or at the beginning , which means "end with".
        /// </summary>
        [DataMember]
        public List<string> AcceptedCategoryNames { get; set; } = new List<string>();

        /// <summary>
        /// The list of accepted mapping pairs of category names. Each is a mapping to the logger pattern name for NLog.
        /// The name can have the wild char '*' at the end, which means "start with"; or at the beginning , which means "end with".
        /// </summary>
        [DataMember]
        public Dictionary<string, string> AcceptedAliasesCategoryNames { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The minimal logger level.
        /// </summary>
        [DataMember]
        public Microsoft.Extensions.Logging.LogLevel? MinLevel { get; set; }

        /// <summary>
        /// The function used to filter events based on the log level and category name.
        /// </summary>
        [IgnoreDataMember]
        public Func<string, Microsoft.Extensions.Logging.LogLevel, bool> Filter { get; set; }

        /// <summary>
        /// Gets or sets the include scope into the message.
        /// </summary>
        [DataMember]
        public bool IncludeScopes { get; set; }
    }
}
