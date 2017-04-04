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
using System.Net;
using System.Net.Http;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientFactory
    /// 
    /// <summary>
    /// HTTP クライアントを生成するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class ClientFactory
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// HTTP クライアントを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(HttpClientHandler handler, TimeSpan timeout)
        {
            var http = (handler != null) ? new HttpClient(handler) : new HttpClient();
            http.DefaultRequestHeaders.ConnectionClose = true;
            http.Timeout = timeout;
            return http;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// HTTP クライアントを生成します。
        /// タイムアウト時間は 2 秒に設定されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(HttpClientHandler handler)
            => Create(handler, TimeSpan.FromSeconds(2));

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// HTTP クライアントを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(TimeSpan timeout)
        {
            var handler = new HttpClientHandler
            {
                Proxy    = null,
                UseProxy = false,
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate |
                                                 DecompressionMethods.GZip;
            }

            return Create(handler, timeout);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// HTTP クライアントを生成します。
        /// タイムアウト時間は 2 秒に設定されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create() => Create(TimeSpan.FromSeconds(2));
    }
}
