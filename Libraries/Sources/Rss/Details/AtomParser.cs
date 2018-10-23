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
using Cube.Net.Rss.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
    /// <remarks>
    /// このクラスでは Atom 0.3 で定義された一部のタグも解析します。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    internal static class AtomParser
    {
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
            var items = ParseItems(root);
            return new RssFeed(items)
            {
                Title         = root.GetTitle(),
                Link          = root.GetUri("link"),
                LastChecked   = DateTime.Now,
                LastPublished = items.FirstOrDefault()?.PublishTime,
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
        private static IList<RssItem> ParseItems(XElement src) => src
            .Descendants(src.GetDefaultNamespace() + "entry")
            .Select(e => new RssItem
            {
                Title       = e.GetTitle(),
                Summary     = GetSummary(e),
                Content     = GetContent(e),
                Link        = e.GetUri("link"),
                PublishTime = GetPublishTime(e),
                Status      = RssItemStatus.Unread,
            })
            .OrderByDescending(e => e.PublishTime)
            .ToList();

        /* ----------------------------------------------------------------- */
        ///
        /// GetSummary
        ///
        /// <summary>
        /// Summary を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static string GetSummary(XElement src)
        {
            var n    = RssParseOptions.MaxSummaryLength;
            var dest = src.GetValue("summary").Strip(n);
            return !string.IsNullOrEmpty(dest) ? dest : src.GetValue("content").Strip(n);
        }

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
            var dest = src.GetValue("content");
            if (string.IsNullOrEmpty(dest)) dest = src.GetValue("summary");
            return dest.Trim();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetPublishTime
        ///
        /// <summary>
        /// 公開日時を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static DateTime? GetPublishTime(XElement src) =>
            src.GetDateTime("updated")   ?? // Atom 1.0
            src.GetDateTime("modified")  ?? // Atom 0.3
            src.GetDateTime("issued")    ?? // Atom 0.3
            src.GetDateTime("published") ?? // Atom 1.0
            src.GetDateTime("created");     // Atom 0.3

        #endregion
    }
}
