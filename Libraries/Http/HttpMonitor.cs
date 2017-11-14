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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitor
    ///
    /// <summary>
    /// 定期的に HTTP 通信を実行するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HttpMonitor<TValue> : NetworkMonitorBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(ContentHandler<TValue> handler) : base()
        {
            _http   = HttpClientFactory.Create(handler);
            Handler = handler;
            Timeout = TimeSpan.FromSeconds(2);
            Timer.Subscribe(WhenTick);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(IContentConverter<TValue> converter)
            : this(new ContentHandler<TValue>(converter)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(Func<Stream, TValue> func)
            : this(new ContentConverter<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// Uris の最初の項目を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

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
        protected IList<Action<Uri, TValue>> Subscriptions { get; }
            = new List<Action<Uri, TValue>>();

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
        public IDisposable Subscribe(Action<Uri, TValue> action)
        {
            Subscriptions.Add(action);
            return Disposable.Create(() => Subscriptions.Remove(action));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// 監視を停止します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public override void Stop()
        {
            base.Stop();
            _http.CancelPendingRequests();
        }

        #region Protected

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequestUris
        ///
        /// <summary>
        /// リクエスト送信先 URL 一覧を取得します。
        /// </summary>
        /// 
        /// <returns>URL 一覧</returns>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual Uri GetRequestUri() => Uri;

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Publish(Uri uri, TValue value)
        {
            foreach (var action in Subscriptions)
            {
                try { action(uri, value); }
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
            using (var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseContentRead))
            {
                var status = response.StatusCode;
                var code   = (int)status;
                var digit  = code / 100;

                if (response.Content is HttpValueContent<TValue> content) Publish(uri, content.Value); // OK
                else if (digit == 3) this.LogDebug($"HTTP:{code} {status}");
                else if (digit == 4) LogWarn(uri, $"HTTP:{code} {status}");
                else if (digit == 5) throw new HttpRequestException($"HTTP:{code} {status}");
                else LogWarn(uri, $"Content is not {nameof(TValue)} ({code})");
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

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenTick
        ///
        /// <summary>
        /// 一定間隔で実行されます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private async void WhenTick()
        {
            if (State != TimerState.Run || Subscriptions.Count <= 0) return;

            SetTimeout();
            var uri = GetRequestUri();

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    await PublishAsync(uri);
                    break;
                }
                catch (Exception err)
                {
                    this.LogWarn(err.ToString(), err);
                    await Task.Delay(RetryInterval);
                    this.LogDebug($"Retry\tCount:{i + 1}\tUrl:{uri}");
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetTimeout
        ///
        /// <summary>
        /// タイムアウト時間を設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void SetTimeout()
        {
            try { if (_http.Timeout != Timeout) _http.Timeout = Timeout; }
            catch (Exception /* err */) { this.LogWarn("Timeout cannot be applied"); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LogWarn
        ///
        /// <summary>
        /// エラー内容をログに出力します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void LogWarn(Uri uri, string message)
        {
            this.LogWarn(uri.ToString());
            this.LogWarn(message);
        }

        #region Fields
        private HttpClient _http;
        #endregion

        #endregion
    }
}
