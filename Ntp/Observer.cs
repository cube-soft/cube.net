/* ------------------------------------------------------------------------- */
///
/// Observer.cs
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
using System.Timers;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Ntp.Observer
    ///
    /// <summary>
    /// NTP サーバと通信を行い、時刻のずれを監視するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Observer : Cube.Net.NetworkAwareObserver
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(string server)
            : this(server, Client.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(string server, int port)
            : base()
        {
            Interval = TimeSpan.FromHours(1);
            _server = server;
            _port = port;
            _timer.Elapsed += (s, e) => { var _ = GetAsync(); };
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Enabled
        /// 
        /// <summary>
        /// NTP サーバと定期的に通信する事でローカルの時刻が監視されて
        /// いるかどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Enabled
        {
            get { return _timer.Enabled; }
        }

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
        /// Timeout
        /// 
        /// <summary>
        /// NTP サーバとの通信タイムアウト時間を設定または取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Interval
        /// 
        /// <summary>
        /// NTP サーバに問い合わせる間隔を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Interval
        {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
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
            get { return _packet; }
            private set
            {
                lock (_lock) _packet = value;
                OnResultChanged(new PacketEventArgs(value));
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

        /* ----------------------------------------------------------------- */
        ///
        /// FailedCount
        /// 
        /// <summary>
        /// サーバとの通信に失敗した回数を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// この値は Server や Port を変更した時、または Reset() を
        /// 実行した時に 0 になります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public int FailedCount { get; private set; }

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
        public event Action<object, PacketEventArgs> ResultChanged;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// 現在の LastResult プロパティの値を破棄し、NTP サーバに
        /// 問い合わせます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Reset()
        {
            Result = null;
            FailedCount = 0;

            if (Enabled)
            {
                Stop();
                Start();
            }
        }

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
        protected virtual void OnResultChanged(PacketEventArgs e)
        {
            if (ResultChanged != null) ResultChanged(this, e);
        }

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// ExecuteStart
        /// 
        /// <summary>
        /// サーバと定期的に通信し、ローカルの時刻を監視します。
        /// </summary>
        /// 
        /// <remarks>
        /// Timer を開始する前に 1 度 NTP サーバと通信を行います。そのため、
        /// Enabled プロパティが直ちに true に変更されない場合があります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override void ExecuteStart()
        {
            if (Enabled) return;
            GetAsync().ContinueWith(_ => { lock (_lock) _timer.Start(); });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ExecuteStop
        /// 
        /// <summary>
        /// 時刻の監視を中断します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void ExecuteStop()
        {
            if (!Enabled) return;
            lock (_lock) _timer.Stop();
        }

        #endregion

        #region Implementations

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
            for (var i = 0; i < _RetryCount; ++i )
            {
                try
                {
                    var client = new Client(Server, Port);
                    client.Timeout = Timeout;
                    var packet = await client.GetAsync();
                    if (packet != null || packet.IsValid)
                    {
                        Result = packet;
                        return;
                    }
                }
                catch (Exception err) { System.Diagnostics.Trace.TraceError(err.ToString()); }
                ++FailedCount;
                await Cube.TaskEx.Delay(_RetryInterval);
            }
        }

        #endregion

        #region Fields
        private string _server = string.Empty;
        private int _port = Client.DefaultPort;
        private Packet _packet = null;
        private TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private Timer _timer = new Timer();
        private object _lock = new object();
        #endregion

        #region Constant fields
        private static readonly int _RetryCount = 5;
        private static readonly int _RetryInterval = 5000; // [ms]
        #endregion
    }
}
