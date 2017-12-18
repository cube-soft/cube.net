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

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssUriConverter
    ///
    /// <summary>
    /// RSS のフィード用 URL に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssUriConverter
    {
        #region Methods

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
            var ns  = doc.Root.GetDefaultNamespace();

            return doc
                .Descendants(ns + "link")
                .Where(e => IsRssLink(e))
                .OrderBy(e => e.Attribute("type").Value)
                .Select(e => new Uri(e.Attribute("href").Value));
        }


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
            if (e.Attribute("rel")?.Value != "alternate") return false;
            var dest = e.Attribute("type")?.Value?.ToLower() ?? "";
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
            using (var sgml = new Sgml.SgmlReader
            {
                CaseFolding = Sgml.CaseFolding.ToLower,
                DocType     = "HTML",
                IgnoreDtd   = true,
                InputStream = stream,
            }) return XDocument.Load(sgml);
        }

        #endregion
    }
}
