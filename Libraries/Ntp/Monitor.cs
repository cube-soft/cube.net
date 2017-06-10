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
using Microsoft.Win32;
using Cube.Log;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Monitor
    ///
    /// <summary>
    /// NTP サーバと定期的に通信を行い、時刻のずれを監視するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Monitor : IDisposable
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
        /* ----------------------------------------------------------------- */
        public Monitor() : this(Ntp.Client.DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="server">NTP サーバ</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(string server) : this(server, Ntp.Client.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="server">NTP サーバ</param>
        /// <param name="port">ポート番号</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(string server, int port) : base()
        {
            Interval = TimeSpan.FromHours(1);

            _server = server;
            _port = port;
            _core.Subscribe(WhenTick);

            SystemEvents.TimeChanged += (s, e) => OnTimeChanged(e);
        }

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
        /// Server
        /// 
        /// <summary>
        /// NTP サーバのアドレス (ホスト名または IP アドレス) を取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Server
        {
            get { return _server; }
            set
            {
                if (_server == value) return;
                _server = value;
                Reset();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Port
        /// 
        /// <summary>
        /// ポート番号を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Port
        {
            get { return _port; }
            set
            {
                if (_port == value) return;
                _port = value;
                Reset();
            }
        }

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
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

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
        protected IList<Action<TimeSpan>> Subscriptions { get; } = new List<Action<TimeSpan>>();

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// TimeChanged
        /// 
        /// <summary>
        /// システムの時刻が変更された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler TimeChanged;

        /* ----------------------------------------------------------------- */
        ///
        /// OnTimeChanged
        /// 
        /// <summary>
        /// システムの時刻が変更された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnTimeChanged(EventArgs e)
        {
            if (_core.PowerMode == PowerModes.Suspend) return;
            Reset();
            TimeChanged?.Invoke(this, e);
        }

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
        public void Subscribe(Action<TimeSpan> action)
            => Subscriptions.Add(action);

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// リセットします。
        /// </summary>
        /// 
        /// <remarks>
        /// リセット時に直ちに NTP サーバと通信した場合、精度が悪い傾向が
        /// あります。現在は、500 ミリ秒の待機時間を設ける事で回避して
        /// います。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        public virtual void Reset()
        {
            var current = State;
            _core.Reset();
            if (current == TimerState.Run)
            {
                Stop();
                Start(TimeSpan.FromMilliseconds(500));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void Publish(TimeSpan value)
        {
            foreach (var action in Subscriptions) action(value);
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

            if (disposing) _core.Dispose();

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
        /// 一定間隔毎に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async void WhenTick()
        {
            if (State != TimerState.Run || Subscriptions.Count <= 0) return;

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    var client = new Client(Server, Port) { Timeout = Timeout };
                    var packet = await client.GetAsync();
                    if (packet != null && packet.IsValid) Publish(packet.LocalClockOffset);
                    else throw new ArgumentException("InvalidPacket");
                    break;
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
        private string _server = string.Empty;
        private int _port = Client.DefaultPort;
        private NetworkAwareTimer _core = new NetworkAwareTimer();
        #endregion

        #endregion
    }
}
