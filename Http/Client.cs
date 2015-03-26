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

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Http.Client
    /// 
    /// <summary>
    /// HTTP 通信を行うためのクライアント用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class Client : System.Net.WebClient
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client()
            : base()
        {
            CachePolicy = new System.Net.Cache.RequestCachePolicy(
                System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            Proxy = null;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        /// 
        /// <summary>
        /// ユーザエージェントを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableETag
        /// 
        /// <summary>
        /// ETag 機能が有効化どうかを表す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool EnableETag
        {
            get { return _enableETag; }
            set { _enableETag = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableGzip
        /// 
        /// <summary>
        /// レスポンスを gzip 圧縮する機能が有効化どうかを表す値を取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool EnableGzip
        {
            get { return _enableGZip; }
            set { _enableGZip = value; }
        }

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetWebRequest
        /// 
        /// <summary>
        /// 指定したリソースの WebRequest オブジェクトを返します。
        /// </summary>
        /// 
        /// <remarks>
        /// System.Net.WebClient から継承されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override System.Net.WebRequest GetWebRequest(Uri address)
        {
            var dest = base.GetWebRequest(address);
            System.Diagnostics.Debug.Assert(dest.Proxy == Proxy);
            System.Diagnostics.Debug.Assert(dest.CachePolicy == CachePolicy);

            var http = dest as System.Net.HttpWebRequest;
            if (http == null) return dest;

            SetUserAgent(http);
            SetEtag(http);
            SetGZip(http);

            return http;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetWebResponse
        /// 
        /// <summary>
        /// 指定した WebRequest の WebResponse を返します。
        /// </summary>
        /// 
        /// <remarks>
        /// System.Net.WebClient から継承されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override System.Net.WebResponse GetWebResponse(System.Net.WebRequest request)
        {
            var dest = base.GetWebResponse(request);
            var http = dest as System.Net.HttpWebResponse;
            if (http == null) return dest;

            GetETag(http);

            return http;
        }

        #endregion

        #region Implementations

        #region Set the HttpWebRequest object to values

        /* ----------------------------------------------------------------- */
        ///
        /// SetUserAgent
        /// 
        /// <summary>
        /// リクエストオブジェクトに UserAgent を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetUserAgent(System.Net.HttpWebRequest request)
        {
            if (string.IsNullOrEmpty(UserAgent)) return;
            request.UserAgent = UserAgent;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetEtag
        /// 
        /// <summary>
        /// リクエストオブジェクトに ETag を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetEtag(System.Net.HttpWebRequest request)
        {
            if (!EnableETag || string.IsNullOrEmpty(_etag)) return;
            request.Headers.Add(System.Net.HttpRequestHeader.IfNoneMatch, _etag);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetGZip
        /// 
        /// <summary>
        /// リクエストオブジェクトに gzip 関連の設定を行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetGZip(System.Net.HttpWebRequest request)
        {
            if (!EnableGzip) return;

            request.AutomaticDecompression =
                System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.GZip;
            request.Headers.Add(System.Net.HttpRequestHeader.AcceptEncoding, "gzip,deflate");
        }

        #endregion

        #region Get values from the HttpWebResponse object

        /* ----------------------------------------------------------------- */
        ///
        /// GetEtag
        /// 
        /// <summary>
        /// レスポンスオブジェクトか ETag の値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void GetETag(System.Net.HttpWebResponse response)
        {
            var name = System.Net.HttpResponseHeader.ETag.ToString();
            var etag = response.GetResponseHeader(name);
            _etag = (!string.IsNullOrEmpty(etag) && EnableETag) ? etag : string.Empty;
        }

        #endregion

        #endregion

        #region Fields
        private bool _enableGZip = true;
        private bool _enableETag = true;
        private string _userAgent = "Cube.Net.Http.Client";
        private string _etag = string.Empty;
        #endregion
    }
}
