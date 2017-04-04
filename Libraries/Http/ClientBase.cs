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
    public class ClientBase : IDisposable
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
        /// Handler
        /// 
        /// <summary>
        /// HttpClientHandler オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public HttpClientHandler Handler
        {
            get { return _handler; }
            set
            {
                if (_handler == value) return;
                _handler = value;
            }
        }

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
        {
            get
            {
                if (_http == null) UpdateClient();
                return _http;
            }
        }

        #endregion

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~ClientBase
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~ClientBase()
        {
            Dispose(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _http?.Dispose();
                _handler?.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region Implementations

        /* --------------------------------------------------------------------- */
        ///
        /// UpdateClient
        /// 
        /// <summary>
        /// HTTP 通信用クライアントを更新します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        private void UpdateClient()
        {
            _http?.Dispose();
            _http = Handler != null ?
                    ClientFactory.Create(Handler, Timeout) :
                    ClientFactory.Create(Timeout);
        }

        #region Fields
        private bool _disposed = false;
        private HttpClient _http;
        private HttpClientHandler _handler;
        #endregion

        #endregion
    }
}
