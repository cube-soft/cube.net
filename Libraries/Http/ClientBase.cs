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
using System.Net.Http;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientBase
    /// 
    /// <summary>
    /// HTTP 通信を行う各種クラスのベースとなるクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ClientBase
    {
        #region Constructors

        /* --------------------------------------------------------------------- */
        ///
        /// ClientBase
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <remarks>
        /// このクラスのオブジェクトを直接生成する事はできません。
        /// 各継承クラスを使用して下さい。
        /// </remarks>
        ///
        /* --------------------------------------------------------------------- */
        protected ClientBase() { }

        #endregion

        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// Version
        /// 
        /// <summary>
        /// クライアントのバージョンを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public SoftwareVersion Version { get; set; } = new SoftwareVersion();

        /* --------------------------------------------------------------------- */
        ///
        /// Handler
        /// 
        /// <summary>
        /// HttpClientHandler オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public HttpClientHandler Handler { get; set; }

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

        /* --------------------------------------------------------------------- */
        ///
        /// Http
        /// 
        /// <summary>
        /// HTTP 通信用クライアントを取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public HttpClient Http
            => Handler != null ?
               ClientFactory.Create(Handler, Timeout) :
               ClientFactory.Create(Timeout);

        #endregion
    }
}
