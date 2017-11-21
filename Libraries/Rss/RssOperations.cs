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
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssOperations
    ///
    /// <summary>
    /// RSS に関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssOperations
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssUris
        /// 
        /// <summary>
        /// RSS フィードの URL を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<Uri> GetRssUris(this System.IO.Stream src)
            => ToXDocument(src)
            .Descendants("link")
            .Where(e => e.Attribute("rel")?.Value == "alternate")
            .Select(e => new Uri(e.Attribute("href").Value));

        #endregion

        #region Implementations

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
            using (var reader = new System.IO.StreamReader(src))
            using (var sgml = new Sgml.SgmlReader
            {
                DocType = "HTML",
                CaseFolding = Sgml.CaseFolding.ToLower,
                InputStream = reader,
                IgnoreDtd = true,
            }) return XDocument.Load(sgml);
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssAsync
        /// 
        /// <summary>
        /// RSS または Atom 形式のデータを非同期で取得します。
        /// </summary>
        ///
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        /// 
        /// <returns>RSS または Atom 形式データの変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<RssFeed> GetRssAsync(this HttpClient client, Uri uri)
            => client.GetAsync(uri, new RssContentConverter());
    }
}
