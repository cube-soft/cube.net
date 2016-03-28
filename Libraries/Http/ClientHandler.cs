/* ------------------------------------------------------------------------- */
///
/// ClientHandler.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientHandler
    /// 
    /// <summary>
    /// System.Net.Http.HttpClient を拡張するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ClientHandler : HttpClientHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ClientHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ClientHandler() : base()
        {
            Proxy    = null;
            UseProxy = false;
        }

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
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SetEntityTag(request.Headers);
            
            var response = await base.SendAsync(request, cancellationToken);

            GetEntityTag(response.Headers);

            return response;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SetEntityTag
        ///
        /// <summary>
        /// エンティティタグ (ETag) をリクエストヘッダに設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetEntityTag(HttpRequestHeaders headers)
        {
            if (string.IsNullOrEmpty(_etag)) return;
            var value = EntityTagHeaderValue.Parse(_etag);
            headers.IfNoneMatch.Add(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetEntityTag
        ///
        /// <summary>
        /// エンティティタグ (ETag) をリクエストヘッダから取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void GetEntityTag(HttpResponseHeaders headers)
        {
            _etag = headers?.ETag?.Tag ?? string.Empty;
            this.LogDebug($"ETag:{_etag}");
        }

        #endregion

        #region Fields
        private string _etag = string.Empty;
        #endregion
    }
}
