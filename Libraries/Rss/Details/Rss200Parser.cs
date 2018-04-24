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
    /// Rss200Parser
    ///
    /// <summary>
    /// RSS 2.0 を解析するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class Rss200Parser
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
            var e = root.Element("channel");
            if (e == null) return default(RssFeed);

            var items = ParseItems(e);
            return new RssFeed(items)
            {
                Title         = e.GetTitle(),
                Description   = e.GetValue("description"),
                Link          = e.GetUri("link"),
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
            .Descendants("item")
            .Select(e => new RssItem
            {
                Title       = e.GetTitle(),
                Summary     = GetSummary(e),
                Content     = GetContent(e),
                Link        = e.GetUri("link"),
                PublishTime = e.GetDateTime("pubDate"),
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
        private static string GetSummary(XElement src) =>
            src.GetValue("description").Strip(RssParseOptions.MaxSummaryLength);

        /* ----------------------------------------------------------------- */
        ///
        /// GetContent
        ///
        /// <summary>
        /// Content を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// RSS 1.0 の content:encoded タグを利用される事が多いため、
        /// 該当タグが存在する場合はその内容を返します。
        /// RSS 2.0 の仕様には Content に相当するものが存在しないため、
        /// それ以外の場合は description の内容を返します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private static string GetContent(XElement src)
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
