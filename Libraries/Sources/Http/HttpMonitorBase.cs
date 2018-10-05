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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitorBase
    ///
    /// <summary>
    /// 定期的に HTTP 通信を実行するための基底クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class HttpMonitorBase<TValue> : NetworkMonitorBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitorBase
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        protected HttpMonitorBase(ContentHandler<TValue> handler)
        {
            Handler = handler;
            Timeout = TimeSpan.FromSeconds(2);
            _http   = HttpClientFactory.Create(handler);
        }

        #endregion

        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// ユーザエージェントを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public string UserAgent
        {
            get => Handler.UserAgent;
            set => Handler.UserAgent = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Handler
        ///
        /// <summary>
        /// HTTP ハンドラを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected ContentHandler<TValue> Handler { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscriptions
        ///
        /// <summary>
        /// 購読者一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected IList<Func<Uri, TValue, Task>> Subscriptions { get; } =
            new List<Func<Uri, TValue, Task>>();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// SubscribeAsync
        ///
        /// <summary>
        /// データ受信時に非同期実行する処理を登録します。
        /// </summary>
        ///
        /// <param name="action">非同期実行する処理</param>
        ///
        /// <returns>登録解除用オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable SubscribeAsync(Func<Uri, TValue, Task> action)
        {
            Subscriptions.Add(action);
            return Disposable.Create(() => Subscriptions.Remove(action));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
        ///
        /// <summary>
        /// データ受信時に実行する処理を登録します。
        /// </summary>
        ///
        /// <param name="action">実行する処理</param>
        ///
        /// <returns>登録解除用オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable Subscribe(Action<Uri, TValue> action) =>
            SubscribeAsync((u, v) =>
            {
                action(u, v);
                return Task.FromResult(0);
            });

        /* ----------------------------------------------------------------- */
        ///
        /// PublishAsync
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        ///
        /// <param name="uri">URL</param>
        /// <param name="value">通信結果</param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual async Task PublishAsync(Uri uri, TValue value)
        {
            foreach (var action in Subscriptions)
            {
                try { await action(uri, value); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PublishAsync
        ///
        /// <summary>
        /// HTTP 通信を実行し、変換結果を引数にして Publish メソッドを
        /// 実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected async Task PublishAsync(Uri uri)
        {
            var option = HttpCompletionOption.ResponseContentRead;
            this.LogWarn(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });

            using (var msg = await _http.GetAsync(uri, option))
            {
                if (msg.Content is HttpValueContent<TValue> c) await PublishAsync(uri, c.Value);
                else this.LogWarn($"{uri} ({(int)msg.StatusCode} {msg.StatusCode})");
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
        /// <param name="disposing">
        /// マネージリソースを解放するかどうかを示す値
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _http.Dispose();
                Handler.Dispose();
            }
        }

        #endregion

        #region Fields
        private readonly HttpClient _http;
        #endregion
    }
}
