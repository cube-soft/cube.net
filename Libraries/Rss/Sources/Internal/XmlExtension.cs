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
using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Cube.Mixin.Xml;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// XmlExtension
    ///
    /// <summary>
    /// Provides extended methods to parse the RSS and Atom feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class XmlExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        ///
        /// <summary>
        /// Gets the value from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>String value.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string name) =>
            GetValue(e, string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        ///
        /// <summary>
        /// Gets the value from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="ns">Target XML namespace.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>String value.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string ns, string name) =>
            e.GetElement(ns, name)?.Value ?? string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// GetTitle
        ///
        /// <summary>
        /// Gets the title from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        ///
        /// <returns>String value.</returns>
        ///
        /// <remarks>
        /// If the title tag does not exist, it will be replaced by the
        /// content of the link tag.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetTitle(this XElement e)
        {
            var title = e.GetValue("title");
            if (!string.IsNullOrEmpty(title)) return title.Trim();

            var uri = e.GetUri("link");
            return uri?.ToString() ?? string.Empty;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetDateTime
        ///
        /// <summary>
        /// Gets the DateTime object from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>DateTime object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime? GetDateTime(this XElement e, string name) =>
            e.GetDateTime(string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetDateTime
        ///
        /// <summary>
        /// Gets the DateTime object from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="ns">Target XML namespace.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>DateTime オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime? GetDateTime(this XElement e, string ns, string name)
        {
            var value = e.GetValue(ns, name);
            return DateTime.TryParse(value, out DateTime dest) ? dest : default(DateTime?);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        ///
        /// <summary>
        /// Gets the Uri object from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>Uri object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement e, string name) =>
            e.GetUri(string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        ///
        /// <summary>
        /// Gets the Uri object from the specified XML element.
        /// </summary>
        ///
        /// <param name="e">Source XML element.</param>
        /// <param name="ns">Target XML namespace.</param>
        /// <param name="name">Target name.</param>
        ///
        /// <returns>Uri object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement e, string ns, string name)
        {
            try
            {
                var value = e.GetValueOrAttribute(ns, name, "href");
                return !string.IsNullOrEmpty(value) ? new Uri(value.Trim()) : default;
            }
            catch { return default; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Strip
        ///
        /// <summary>
        /// Normalizes the specified string by removing HTML tags, etc.
        /// </summary>
        ///
        /// <param name="src">Source string.</param>
        /// <param name="n">maximum string length.</param>
        ///
        /// <returns>Normalized string.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string Strip(this string src, int n)
        {
            var dest = string.IsNullOrEmpty(src) ?
                       string.Empty :
                       Regex.Replace(src, @"<(""[^""]*""|'[^']*'|[^'"">])*>|\t|\n|\r", string.Empty).Trim();
            return dest.Length <= n ?
                   dest :
                   dest.Substring(0, n);
        }

        #endregion
    }
}
