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
    /// EntityTag
    ///
    /// <summary>
    /// EntityTag を表すクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class EntityTag
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTag
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EntityTag() { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// EntityTag を表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Value { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetHandler
        ///
        /// <summary>
        /// EntityTag 監視用の HttpClientHandler オブジェクトを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public HttpClientHandler GetHandler()
        {
            var dest = new EntityTagHandler(Value);
            dest.Received += WhenReceived;
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ToString
        ///
        /// <summary>
        /// 文字列に変換します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public override string ToString() => Value;

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenReceived
        ///
        /// <summary>
        /// EntityTag を受信時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void WhenReceived(object sender, ValueEventArgs<string> e)
        {
            Value = e.Value;

            var handler = sender as EntityTagHandler;
            if (handler == null) return;

            handler.Received -= WhenReceived;
        }

        #endregion
    }

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
            Proxy = null;
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
        public event ValueEventHandler<string> Received;

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
            SetEntityTag(request.Headers);
            var response = await base.SendAsync(request, cancellationToken);
            _etag = GetEntityTag(response.Headers);
            Received?.Invoke(this, ValueEventArgs.Create(_etag));
            return response;
        }

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

        #region Fields
        private string _etag = string.Empty;
        #endregion

        #endregion
    }
}
