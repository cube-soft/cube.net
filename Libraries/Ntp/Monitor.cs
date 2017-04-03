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
    public class Monitor : NetworkAwareTimer
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

            SystemEvents.TimeChanged += (s, e) => OnTimeChanged(e);
        }

        #endregion

        #region Properties

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
        /// IsValid
        /// 
        /// <summary>
        /// 最新の結果が有効なものであるかどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsValid => _packet?.IsValid ?? false;

        /* ----------------------------------------------------------------- */
        ///
        /// Result
        /// 
        /// <summary>
        /// ローカル時刻とのずれを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Result => IsValid ? Packet.LocalClockOffset : TimeSpan.Zero;

        /* ----------------------------------------------------------------- */
        ///
        /// Packet
        /// 
        /// <summary>
        /// 最新の Packet オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected Packet Packet
        {
            get { return _packet; }
            set
            {
                _packet = value;
                OnResultChanged(ValueEventArgs.Create(Result));
            }
        }

        #endregion

        #region Events

        #region ResultChanged

        /* ----------------------------------------------------------------- */
        ///
        /// ResultChanged
        /// 
        /// <summary>
        /// Result プロパティが変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueEventHandler<TimeSpan> ResultChanged;

        /* ----------------------------------------------------------------- */
        ///
        /// OnResultChanged
        /// 
        /// <summary>
        /// ResultChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnResultChanged(ValueEventArgs<TimeSpan> e)
            => ResultChanged?.Invoke(this, e);

        #endregion

        #region TimeChanged

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
            if (PowerMode == PowerModes.Suspend) return;
            Reset();

            TimeChanged?.Invoke(this, e);
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnReset
        /// 
        /// <summary>
        /// 現在の状態を破棄し、NTP サーバに問い合わせます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnReset()
        {
            var current = State;
            base.OnReset();

            Packet = null;
            if (current == TimerState.Run) RaiseExecute();
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
        protected override async void OnExecute(EventArgs e)
        {
            base.OnExecute(e);
            if (State != TimerState.Run) return;
            await GetAsync();
        }

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
                    var client = new Client(Server, Port)
                    {
                        Timeout = Timeout,
                    };

                    var packet = await client.GetAsync();
                    if (packet != null && packet.IsValid)
                    {
                        Packet = packet;
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
        private string _server = string.Empty;
        private int _port = Client.DefaultPort;
        private Packet _packet = null;
        #endregion

        #endregion
    }
}
