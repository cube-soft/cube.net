﻿/* ------------------------------------------------------------------------- */
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssClient
    ///
    /// <summary>
    /// RSS フィードを取得するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssClient : DisposableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssClient
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssClient() : this(null) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssClient
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssClient(HttpClientHandler handler)
        {
            _http = HttpClientFactory.Create(handler, TimeSpan.FromSeconds(10));
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get => _http.Timeout;
            set => _http.Timeout = value;
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Redirected
        ///
        /// <summary>
        /// リダイレクト時に発生するイベントです。
        /// </summary>
        ///
        /// <remarks>
        /// 通信結果が HTML かつ alternate メタ情報が存在する場合に
        /// 発生します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueChangedEventHandler<Uri> Redirected;

        /* ----------------------------------------------------------------- */
        ///
        /// OnRedirected
        ///
        /// <summary>
        /// リダイレクト時に発生するイベントです。
        /// </summary>
        ///
        /// <remarks>
        /// 通信結果が HTML かつ alternate メタ情報が存在する場合に
        /// 発生します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnRedirected(ValueChangedEventArgs<Uri> e) =>
            Redirected?.Invoke(this, e);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// RSS フィードを非同期で取得します。
        /// </summary>
        ///
        /// <param name="uri">フィード取得用 URL</param>
        ///
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<RssFeed> GetAsync(Uri uri)
        {
            var opt = HttpCompletionOption.ResponseContentRead;
            using (var response = await _http.GetAsync(uri, opt).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode) return null;
                await response.Content.LoadIntoBufferAsync();
                var content = await response.Content.ReadAsStringAsync();
                return await ParseAsync(uri, content).ConfigureAwait(false);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        ///
        /// <param name="disposing">マネージリソースを解放するか</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing) _http.Dispose();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ParseAsync
        ///
        /// <summary>
        /// RSS フィードを解析します。
        /// </summary>
        ///
        /// <remarks>
        /// 通信結果が HTML かつ alternate メタ情報が存在する場合、
        /// 該当の URL に対して再度 HTTP 通信を試みます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<RssFeed> ParseAsync(Uri uri, string content)
        {
            try
            {
                var root = XDocument.Parse(content).Root;
                var version = root.GetRssVersion();
                if (version != RssVersion.Unknown)
                {
                    var dest = RssParser.Parse(root, version);
                    if (dest != null) dest.Uri = uri;
                    return dest;
                }
            }
            catch { /* try redirect */ }

            var cvt = RssParser.GetRssUris(content).FirstOrDefault();
            if (cvt == null) return default;

            OnRedirected(ValueEventArgs.Create(uri, cvt));
            return await GetAsync(cvt).ConfigureAwait(false);
        }

        #endregion

        #region Fields
        private readonly HttpClient _http;
        #endregion
    }
}
