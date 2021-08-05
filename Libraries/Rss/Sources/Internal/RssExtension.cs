/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using System.Xml.Linq;

namespace Cube.Net.Rss.Parsing
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssExtension
    ///
    /// <summary>
    /// Provides extended methods to parse the RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class RssExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssSummary
        ///
        /// <summary>
        /// Gets the summary from the specified XML element.
        /// </summary>
        ///
        /// <param name="src">Source XML element.</param>
        ///
        /// <returns>String value.</returns>
        ///
        /// <remarks>
        /// If the description tag does not exist and the content:encoded
        /// tag exists, it will return the content of the content:encoded
        /// with the HTML tag removed.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetRssSummary(this XElement src)
        {
            var n    = RssParseOptions.MaxSummaryLength;
            var dest = src.GetValue("description").Strip(n);
            return !string.IsNullOrEmpty(dest) ?
                   dest :
                   src.GetValue("content", "encoded").Strip(n);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssContent
        ///
        /// <summary>
        /// Get the content from the specified XML element.
        /// </summary>
        ///
        /// <param name="src">Source XML element.</param>
        ///
        /// <returns>String value.</returns>
        ///
        /// <remarks>
        /// The content:encoded tag is often used in both RSS 1.0 and 2.0,
        /// so it returns the content of the tag if it exists, or the
        /// content of the description if the content:encoded tag does not
        /// exist.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetRssContent(this XElement src)
        {
            var enc  = src.GetValue("content", "encoded");
            var dest = !string.IsNullOrEmpty(enc) ?
                       enc :
                       src.GetValue("description") ??
                       string.Empty;
            return dest.Trim();
        }

        #endregion
    }
}
