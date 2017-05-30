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
using System.Net;
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
    public class Monitor<TValue> : IDisposable
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
            _core.Subscribe(WhenTick);
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
            : this(new ContentConverter<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAvailable
        /// 
        /// <summary>
        /// ネットワークが使用可能な状態かどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool NetworkAvailable => _core.NetworkAvailable;

        /* ----------------------------------------------------------------- */
        ///
        /// State
        /// 
        /// <summary>
        /// オブジェクトの状態を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimerState State => _core.State;

        /* ----------------------------------------------------------------- */
        ///
        /// Interval
        /// 
        /// <summary>
        /// 実行間隔を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Interval
        {
            get { return _core.Interval; }
            set { _core.Interval = value; }
        }

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
        /// FailedCount
        /// 
        /// <summary>
        /// サーバとの通信に失敗した回数を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public int FailedCount { get; protected set; } = 0;

        /* --------------------------------------------------------------------- */
        ///
        /// RetryCount
        /// 
        /// <summary>
        /// 通信失敗時に再試行する最大回数を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public int RetryCount { get; set; } = 5;

        /* --------------------------------------------------------------------- */
        ///
        /// RetryInterval
        /// 
        /// <summary>
        /// 通信失敗時に再試行する間隔を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

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

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// アプリケーションのバージョンを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SoftwareVersion Version { get; set; } = new SoftwareVersion();

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
        /// Start
        ///
        /// <summary>
        /// 監視を実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Start() => Start(TimeSpan.Zero);

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 監視を実行します。
        /// </summary>
        /// 
        /// <param name="delay">初期遅延時間</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Start(TimeSpan delay)
        {
            var state = _core.State;
            _core.Start(delay);
            if (state != TimerState.Stop) return;
            this.LogDebug($"Start\tInterval:{Interval}\tInitialDelay:{delay}");
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
        public void Stop()
        {
            var state = _core.State;
            _core.Stop();
            if (state == TimerState.Stop) return;
            this.LogDebug($"Stop\tLast:{_core.LastExecuted}\tFaild:{FailedCount}");
        }

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
        /// Reset
        ///
        /// <summary>
        /// リセットします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public virtual void Reset()
        {
            var current = State;
            _core.Reset();
            if (current == TimerState.Run)
            {
                Stop();
                Start();
            }
        }

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
            foreach (var action in Subscriptions)
            {
                try { action(value); }
                catch (Exception err) { this.LogWarn(err.ToString()); }
            }
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
        /// <param name="disposing">
        /// マネージリソースを解放するかどうかを示す値
        /// </param>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _http?.Dispose();
                _handler.Dispose();
            }

            _disposed = true;
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

            var uri = GetRequestUri();
            if (uri == null) return;

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    if (_http == null) _http = ClientFactory.Create(_handler, Timeout);

                    using (var response = await _http.GetAsync(uri))
                    {
                        var status = response.StatusCode;
                        var code   = (int)status;
                        var digit  = code / 100;

                        if (response.Content is ValueContent<TValue> content) Publish(content.Value); // OK
                        else if (digit == 3) this.LogDebug($"HTTP:{code} {status}");
                        else if (digit == 4) Fail($"HTTP:{code} {status}");
                        else if (digit == 5) throw new HttpRequestException($"HTTP:{code} {status}");
                        else Fail($"Content is not {nameof(TValue)} ({code})");
                        break;
                    }
                }
                catch (Exception err)
                {
                    Fail(err.ToString());
                    await Task.Delay(RetryInterval);
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Fail
        ///
        /// <summary>
        /// 通信に失敗した事を伝える処理を実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Fail(string message)
        {
            ++FailedCount;
            this.LogWarn(message);
            this.LogWarn($"Failed\tCount:{FailedCount}");
        }

        #region Fields
        private bool _disposed = false;
        private HttpClient _http;
        private ContentHandler<TValue> _handler;
        private NetworkAwareTimer _core = new NetworkAwareTimer();
        #endregion

        #endregion
    }
}
