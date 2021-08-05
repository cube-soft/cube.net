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
using Cube.Mixin.Xml;
using Cube.Net.Rss.Parsing;

namespace Cube.Net.Rss
{
    #region Rss100Parser

    /* --------------------------------------------------------------------- */
    ///
    /// Rss100Parser
    ///
    /// <summary>
    /// Provides functionality to parse the RSS 1.0 feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class Rss100Parser
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        ///
        /// <summary>
        /// Parses the specified XML object and creates a new instance of
        /// the RssFeed class.
        /// </summary>
        ///
        /// <param name="root">Root XML element.</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement root)
        {
            var e = root.GetElement("channel");
            if (e == null) return default;

            var items = ParseItems(root);
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
        /// Creates the list of RssItem objects with the specified XML
        /// element.
        /// </summary>
        ///
        /// <param name="src">Source XML element.</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IList<RssItem> ParseItems(XElement src) => src
            .Descendants(src.GetDefaultNamespace() + "item")
            .Select(e => new RssItem
            {
                Title       = e.GetTitle(),
                Summary     = e.GetRssSummary(),
                Content     = e.GetRssContent(),
                Link        = e.GetUri("link"),
                PublishTime = e.GetDateTime("dc", "date"),
                Status      = RssItemStatus.Unread,
            })
            .OrderByDescending(e => e.PublishTime)
            .ToList();

        #endregion
    }

    #endregion

    #region Rss200Parser

    /* --------------------------------------------------------------------- */
    ///
    /// Rss200Parser
    ///
    /// <summary>
    /// Provides functionality to parse the RSS 2.0 feeds.
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
        /// Parses the specified XML object and creates a new instance of
        /// the RssFeed class.
        /// </summary>
        ///
        /// <param name="root">Root XML element.</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement root)
        {
            var e = root.Element("channel");
            if (e == null) return default;

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
        /// Creates the list of RssItem objects with the specified XML
        /// element.
        /// </summary>
        ///
        /// <param name="src">Source XML element.</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IList<RssItem> ParseItems(XElement src) => src
            .Descendants("item")
            .Select(e => new RssItem
            {
                Title       = e.GetTitle(),
                Summary     = e.GetRssSummary(),
                Content     = e.GetRssContent(),
                Link        = e.GetUri("link"),
                PublishTime = e.GetDateTime("pubDate"),
                Status      = RssItemStatus.Unread,
            })
            .OrderByDescending(e => e.PublishTime)
            .ToList();

        #endregion
    }

    #endregion
}
