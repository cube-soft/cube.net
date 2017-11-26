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
    /// Rss100Parser
    ///
    /// <summary>
    /// RSS 1.0 を解析するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    internal static class Rss100Parser
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
        public static string Namespace { get; } = "http://purl.org/rss/1.0/";

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
        public static RssFeed Parse(XElement root)
        {
            var e = root.GetElement(Namespace, "channel");
            if (e == null) return default(RssFeed);
            return new RssFeed
            {
                Title       = e.GetTitle(Namespace),
                Description = e.GetValue(Namespace, "description"),
                Link        = e.GetUri(Namespace, "link"),
                Items       = ParseItems(root),
                LastChecked = DateTime.Now,
            };
        }


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
            .Descendants(XNamespace.Get(Namespace) + "item")
            .Select(e => new RssItem
            {
                Title       = e.GetTitle(Namespace),
                Summary     = e.GetValue(Namespace, "description"),
                Content     = GetContent(e),
                Link        = e.GetUri(Namespace, "link"),
                PublishTime = e.GetDateTime(Namespace, "pubDate"),
                Read        = false,
            })
            .ToList();

        /* ----------------------------------------------------------------- */
        ///
        /// GetContent
        /// 
        /// <summary>
        /// Content を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private static string GetContent(XElement src)
        {
            var ns = "http://purl.org/rss/1.0/modules/content/";
            var encoded = src.GetValue(ns, "encoded");

            return !string.IsNullOrEmpty(encoded) ?
                   encoded :
                   src.GetValue(Namespace, "description");
        }

        #endregion
    }
}
