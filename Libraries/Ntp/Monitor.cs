/* ------------------------------------------------------------------------- */
///
/// Monitor.cs
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
    public class Monitor : NetworkAwareMonitor
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
        public Monitor() : this(Client.DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(string server)
            : this(server, Client.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(string server, int port)
            : base()
        {
            Interval = TimeSpan.FromHours(1);
            _server = server;
            _port = port;

            SystemEvents.TimeChanged += (s, e) => OnTimeChanged(e);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Server
        /// 
        /// <summary>
        /// サーバのアドレス (ホスト名または IP アドレス) を取得または
        /// 設定します。
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
        /// Result
        /// 
        /// <summary>
        /// NTP サーバと最後に通信した結果を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Packet Result
        {
            get { return _result; }
            private set
            {
                lock (_lock) _result = value;
                OnResultChanged(new ValueEventArgs<Packet>(value));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsValid
        /// 
        /// <summary>
        /// 最新の結果が有効なものであるかどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsValid
        {
            get { return Result != null && Result.IsValid; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LocalClockOffset
        /// 
        /// <summary>
        /// ローカル時刻とのずれを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan LocalClockOffset
        {
            get { return IsValid ? Result.LocalClockOffset : TimeSpan.Zero; }
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// ResultChanged
        /// 
        /// <summary>
        /// Result プロパティが変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<ValueEventArgs<Packet>> ResultChanged;

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

        #endregion

        #region Virtual methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnResultChanged
        /// 
        /// <summary>
        /// Result プロパティが変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnResultChanged(ValueEventArgs<Packet> e)
            => ResultChanged?.Invoke(this, e);

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
            if (PowerMode == PowerModes.Suspend) return;
            Reset();

            TimeChanged?.Invoke(this, e);
        }

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnReset
        /// 
        /// <summary>
        /// 現在の LastResult プロパティの値を破棄し、NTP サーバに
        /// 問い合わせます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnReset()
        {
            var current = State;
            base.OnReset();

            Result = null;
            FailedCount = 0;

            if (current == SchedulerState.Run)
            {
                var _ = GetAsync();
                Start();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnExecute
        /// 
        /// <summary>
        /// モニタリングのための操作を実行するタイミングになった時に
        /// 発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnExecute(EventArgs e)
        {
            base.OnExecute(e);
            if (State != SchedulerState.Run) return;
            var _ = GetAsync();
        }

        #endregion

        #region Other private methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// 非同期で NTP サーバと通信を行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task GetAsync()
        {
            for (var i = 0; i < RetryCount; ++i )
            {
                try
                {
                    var client = new Client(Server, Port);
                    client.Timeout = Timeout;
                    var packet = await client.GetAsync();
                    if (packet != null && packet.IsValid)
                    {
                        Result = packet;
                        return;
                    }
                }
                catch (Exception err) { this.LogError(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }
        }

        #endregion

        #region Fields
        private string _server = string.Empty;
        private int _port = Client.DefaultPort;
        private Packet _result = null;
        private TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private object _lock = new object();
        #endregion
    }
}
