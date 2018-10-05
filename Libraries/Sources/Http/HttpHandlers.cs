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
using Cube.Log;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HeaderHandler
    ///
    /// <summary>
    /// HTTP 通信のリクエストヘッダおよびレスポンスヘッダを扱うための
    /// ハンドラです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HeaderHandler : HttpClientHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HeaderHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HeaderHandler()
        {
            Proxy    = null;
            UseProxy = false;

            if (SupportsAutomaticDecompression)
            {
                AutomaticDecompression = DecompressionMethods.Deflate |
                                         DecompressionMethods.GZip;
            }
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// ConnectionClose
        ///
        /// <summary>
        /// ConnectionClose ヘッダに設定する値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool ConnectionClose { get; set; } = true;

        /* ----------------------------------------------------------------- */
        ///
        /// UseEntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) を利用するかどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool UseEntityTag { get; set; } = true;

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string EntityTag { get; protected set; }

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// User-Agent を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SendAsync
        ///
        /// <summary>
        /// HTTP リクエストを非同期で送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SetConnectionClose(request.Headers);
            SetEntityTag(request.Headers);
            SetUserAgent(request.Headers);

            var response = await base.SendAsync(request, cancellationToken);

            GetEntityTag(response.Headers);

            return response;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetConnectionClose
        ///
        /// <summary>
        /// リクエストヘッダに ConnectionClose を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetConnectionClose(HttpRequestHeaders headers) => this.LogWarn(() =>
        {
            if (headers.ConnectionClose.HasValue &&
                headers.ConnectionClose == ConnectionClose) return;
            headers.ConnectionClose = ConnectionClose;
        });

        /* ----------------------------------------------------------------- */
        ///
        /// SetUserAgent
        ///
        /// <summary>
        /// リクエストヘッダに User-Agent を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetUserAgent(HttpRequestHeaders headers) => this.LogWarn(() =>
        {
            if (string.IsNullOrEmpty(UserAgent)) return;
            headers.UserAgent.ParseAdd(UserAgent);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// SetEntityTag
        ///
        /// <summary>
        /// リクエストヘッダに EntityTag を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetEntityTag(HttpRequestHeaders headers) => this.LogWarn(() =>
        {
            if (!UseEntityTag || string.IsNullOrEmpty(EntityTag)) return;
            var etag = EntityTagHeaderValue.Parse(EntityTag);
            headers.IfNoneMatch.Add(etag);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// GetEntityTag
        ///
        /// <summary>
        /// レスポンスヘッダから EntityTag を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void GetEntityTag(HttpResponseHeaders headers) =>
            EntityTag = headers?.ETag?.Tag ?? string.Empty;

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ContentHandler(TValue)
    ///
    /// <summary>
    /// HttpContent を TValue オブジェクトに変換するための HTTP
    /// ハンドラです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ContentHandler<TValue> : HeaderHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ContentHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ContentHandler() { }

        /* ----------------------------------------------------------------- */
        ///
        /// ContentHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public ContentHandler(IContentConverter<TValue> converter) : this()
        {
            Converter = converter;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ContentHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="func">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public ContentHandler(Func<Stream, TValue> func) :
            this(new ContentConverter<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Converter
        ///
        /// <summary>
        /// 変換用オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IContentConverter<TValue> Converter { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SendAsync
        ///
        /// <summary>
        /// HTTP リクエストを非同期で送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response == null || Converter == null) return response;
            if (response.IsSuccessStatusCode)
            {
                await response.Content.LoadIntoBufferAsync();
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                response.Content = new HttpValueContent<TValue>(
                    response.Content,
                    Converter.Convert(stream)
                );
            }
            return response;
        }

        #endregion
    }
}
