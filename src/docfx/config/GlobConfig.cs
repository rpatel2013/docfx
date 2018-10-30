// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Microsoft.Docs.Build
{
    internal sealed class GlobConfig<T>
    {
        /// <summary>
        /// Gets the include patterns of files.
        /// </summary>
        public string[] Include = Array.Empty<string>();

        /// <summary>
        /// Gets the exclude patterns of files.
        /// </summary>
        public string[] Exclude = Array.Empty<string>();

        /// <summary>
        /// Gets the value to apply to files.
        /// </summary>
        public T Value;

        /// <summary>
        /// Gets whether the value of <see cref="Include"/> and <see cref="Exclude"/> is glob pattern.
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsGlob = true;

        private Func<string, bool> _glob;

        public bool Match(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            if (IsGlob)
            {
                return LazyInitializer.EnsureInitialized(
                    ref _glob,
                    () => GlobUtility.CreateGlobMatcher(Include, Exclude))(filePath);
            }
            else
            {
                if (Exclude.Any(e => MatchItem(filePath, e)))
                    return false;
                if (Include.Any(i => MatchItem(filePath, i)))
                    return true;

                return false;
            }
        }

        private bool MatchItem(string filePath, string pattern)
        {
            Debug.Assert(!IsGlob);

            return pattern.EndsWith('/') ?
                filePath.StartsWith(pattern, PathUtility.PathComparison) :
                filePath.Equals(pattern, PathUtility.PathComparison);
        }
    }
}