/* ------------------------------------------------------------------------- */
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
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// EntityTagHandler
    ///
    /// <summary>
    /// EntityTag (ETag) を扱うための HTTP クライアント用ハンドラです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal sealed class EntityTagHandler : HttpClientHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTagHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EntityTagHandler() : base()
        {
            Proxy    = null;
            UseProxy = false;

            if (SupportsAutomaticDecompression)
            {
                AutomaticDecompression =
                    DecompressionMethods.Deflate |
                    DecompressionMethods.GZip;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTagHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EntityTagHandler(string etag) : this()
        {
            _etag = etag;
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Received
        ///
        /// <summary>
        /// EntityTag 受信時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<ValueEventArgs<string>> Received;

        #endregion

        #region Override methods

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
            SetEntityTag(request.Headers);
            var response = await base.SendAsync(request, cancellationToken);
            _etag = GetEntityTag(response.Headers);
            Received?.Invoke(this, ValueEventArgs.Create(_etag));
            return response;
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// SetEntityTag
        ///
        /// <summary>
        /// リクエストヘッダに EntityTag を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetEntityTag(HttpRequestHeaders headers)
            => this.LogException(() =>
        {
            if (string.IsNullOrEmpty(_etag)) return;
            var value = EntityTagHeaderValue.Parse(_etag);
            headers.IfNoneMatch.Add(value);
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
        private string GetEntityTag(HttpResponseHeaders headers)
            => headers?.ETag?.Tag ?? string.Empty;

        #endregion

        #region Fields
        private string _etag = string.Empty;
        #endregion
    }
}
