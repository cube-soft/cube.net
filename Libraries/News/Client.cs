/* ------------------------------------------------------------------------- */
///
/// Client.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization;
using Cube.Conversions;
using Cube.Net.Http;

namespace Cube.Net.News
{
    /* --------------------------------------------------------------------- */
    ///
    /// Client
    /// 
    /// <summary>
    /// 新着ニュース記事をサーバに問い合わせるためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Client
    {
        #region Constructors

        /* --------------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public Client()
        {
            var handler = new ClientHandler
            {
                Proxy    = null,
                UseProxy = false,
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression =
                    System.Net.DecompressionMethods.Deflate |
                    System.Net.DecompressionMethods.GZip;
            }

            _handler = handler;
        }

        #endregion

        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// EndPoint
        /// 
        /// <summary>
        /// 新着ニュース取得用の URL を取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static Uri EndPoint
            => new Uri("http://news.cube-soft.jp/app/notice/all.json");

        /* --------------------------------------------------------------------- */
        ///
        /// Version
        /// 
        /// <summary>
        /// ソフトウェアのバージョンを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public SoftwareVersion Version { get; set; } = new SoftwareVersion();

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

        #endregion

        #region Methods

        /* --------------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// 新着記事一覧を非同期で取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// JSON フォーマット: [{"items":[{"title":"xxx","url":"yyy"},...]}]
        /// </remarks>
        ///
        /* --------------------------------------------------------------------- */
        public async Task<IList<Article>> GetAsync()
        {
            var uri = EndPoint.With("appver", Version.ToString()).With(DateTime.Now);
            var client = (_handler != null) ? new HttpClient(_handler) : new HttpClient();
            client.Timeout = Timeout;
            client.DefaultRequestHeaders.ConnectionClose = true;

            var json = await client.GetJsonAsync<List<ArticleList>>(uri);
            if (json != null && json.Count > 0 && json[0] != null && json[0].Items != null) return json[0].Items;
            else return new List<Article>();
        }

        #endregion

        #region Internal classes

        /* --------------------------------------------------------------------- */
        ///
        /// ArticleList
        /// 
        /// <summary>
        /// 記事一覧を取得するためのクラスです。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        [DataContract]
        internal class ArticleList
        {
            [DataMember(Name = "items")]
            public List<Article> Items = null;
        }

        #endregion

        #region Fields
        private HttpMessageHandler _handler = null;
        #endregion
    }
}
