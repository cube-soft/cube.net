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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Cube.Conversions;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Monitor(TValue)
    ///
    /// <summary>
    /// 定期的に HTTP 通信を実行するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Monitor<TValue> : NetworkAwareTimer
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(ContentHandler<TValue> handler) : base()
        {
            _handler = handler;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(IContentConverter<TValue> converter)
            : this (new ContentHandler<TValue>(converter)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(Func<HttpContent, Task<TValue>> func)
            : this(new ContentHandler<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// HTTP 通信を行う URL を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscriptions
        ///
        /// <summary>
        /// 購読者一覧を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected IList<Action<TValue>> Subscriptions { get; } = new List<Action<TValue>>();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
        ///
        /// <summary>
        /// データ受信時に実行する処理を登録します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Subscribe(Action<TValue> action) => Subscriptions.Add(action);

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequestUri
        ///
        /// <summary>
        /// リクエスト送信先 URL を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual Uri GetRequestUri() => Uri.With(Version);

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Publish(TValue value)
        {
            foreach (var x in Subscriptions) x(value);
        }

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~Monitor()
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
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _http?.Dispose();
                _handler?.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnExecute
        ///
        /// <summary>
        /// 一定間隔で実行されます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override async void OnExecute(EventArgs e)
        {
            base.OnExecute(e);
            if (Subscriptions.Count <= 0) return;

            var uri = GetRequestUri();
            if (State != TimerState.Run || uri == null) return;

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    if (_http == null) _http = ClientFactory.Create(_handler, Timeout);
                    var response = await _http.GetAsync(uri);

                    if (response?.Content is ValueContent<TValue> content)
                    {
                        Publish(content.Value);
                        return;
                    }
                }
                catch (Exception err) { this.LogWarn(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }

            this.LogWarn($"Failed\tCount:{FailedCount}");
        }

        #region Fields
        private bool _disposed = false;
        private HttpClient _http;
        private ContentHandler<TValue> _handler;
        #endregion

        #endregion
    }
}
