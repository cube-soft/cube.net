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
using System.Net.Http;
using System.Threading.Tasks;
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssContentConverter
    ///
    /// <summary>
    /// HttpContent を RssFeed に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssContentConverter : Cube.Net.Http.ContentConverter<RssFeed>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// RssContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssContentConverter() : base(async (src) =>
        {
            if (src == null) return default(RssFeed);
            var stream = await src.ReadAsStreamAsync();
            return RssParser.Create(stream);
        }) { }
    }

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
