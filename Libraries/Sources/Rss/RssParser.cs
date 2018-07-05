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
using Cube.Conversions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssParser
    ///
    /// <summary>
    /// RSS および Atom 情報を解析するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssParser
    {
        #region Methods

        #region Parse

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        ///
        /// <summary>
        /// ストリームからデータを読み込み、RssFeed オブジェクトを
        /// 生成します。
        /// </summary>
        ///
        /// <param name="src">ストリーム</param>
        ///
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(System.IO.Stream src) =>
            Parse(XDocument.Load(src).Root);

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
        public static RssFeed Parse(XElement root) =>
            Parse(root, root.GetRssVersion());

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        ///
        /// <summary>
        /// XML オブジェクトから RssFeed オブジェクトを生成します。
        /// </summary>
        ///
        /// <param name="root">XML のルート要素</param>
        /// <param name="version">RSS バージョン</param>
        ///
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement root, RssVersion version)
        {
            switch (version)
            {
                case RssVersion.Atom:   return AtomParser.Parse(root);
                case RssVersion.Rss091: return Rss200Parser.Parse(root);
                case RssVersion.Rss092: return Rss200Parser.Parse(root);
                case RssVersion.Rss100: return Rss100Parser.Parse(root);
                case RssVersion.Rss200: return Rss200Parser.Parse(root);
                default: break;
            }
            return default(RssFeed);
        }

        #endregion

        #region GetRssUris

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssUris
        ///
        /// <summary>
        /// RSS フィードの URL 一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<Uri> GetRssUris(this System.IO.Stream src)
        {
            var doc = ToXDocument(src);
            var ns = doc.Root.GetDefaultNamespace();

            return doc.Descendants(ns + "link")
                      .Where(e => IsRssLink(e))
                      .OrderBy(e => (string)e.Attribute("type"))
                      .Select(e => ((string)e.Attribute("href")).ToUri());
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// IsRssLink
        ///
        /// <summary>
        /// RSS のリンクを示す要素かどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static bool IsRssLink(XElement e)
        {
            if ((string)e.Attribute("rel") != "alternate") return false;
            var dest = ((string)e.Attribute("type") ?? "").ToLowerInvariant();
            return dest.Contains("rss") || dest.Contains("atom");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ToXDocument
        ///
        /// <summary>
        /// XDocument オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static XDocument ToXDocument(System.IO.Stream src)
        {
            using (var stream = new System.IO.StreamReader(src, System.Text.Encoding.UTF8))
            using (var reader = new Sgml.SgmlReader
            {
                CaseFolding = Sgml.CaseFolding.ToLower,
                DocType     = "HTML",
                IgnoreDtd   = true,
                InputStream = stream,
            }) return XDocument.Load(reader);
        }

        #endregion
    }
}
