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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cube.Net.Rss.Parsing;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// AtomParser
    ///
    /// <summary>
    /// Atom を解析するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    internal static class AtomParser
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Namespace
        /// 
        /// <summary>
        /// Atom フィードの名前空間を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string Namespace { get; } = "http://www.w3.org/2005/Atom";

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        /// 
        /// <summary>
        /// XML オブジェクトから RssFeed オブジェクトを生成します。
        /// </summary>
        /// 
        /// <param name="root">XML のルート要素</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement root) => new RssFeed
        {
            Title       = root.GetValue(Namespace, "title"),
            Link        = root.GetUri(Namespace, "link"),
            Items       = ParseItems(root),
            LastChecked = DateTime.Now,
        };

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ParseItems
        /// 
        /// <summary>
        /// XML オブジェクトから RssFeed オブジェクトを生成します。
        /// </summary>
        /// 
        /// <param name="src">XML</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IList<RssItem> ParseItems(XElement src)
            => src
            .Descendants(XNamespace.Get(Namespace) + "entry")
            .Select(e => new RssItem
            {
                Id          = e.GetValue(Namespace, "id"),
                Title       = e.GetValue(Namespace, "title"),
                Summary     = e.GetValue(Namespace, "summary"),
                Content     = e.GetContent(Namespace),
                Link        = e.GetUri(Namespace, "link"),
                PublishTime = e.GetDateTime(Namespace, "published"),
                Read        = false,
            })
            .ToList();

        #endregion
    }
}
