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
    public class Observer
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
        public Observer(string hostNameOrAddress)
            : this(hostNameOrAddress, Client.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(string hostNameOrAddress, int port)
        {
            _timer.Elapsed += (s, e) => { var _ = GetAsync(); };
            Interval = TimeSpan.FromHours(1);
            ResetEndPoint(hostNameOrAddress, port);
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
        /// Timeout
        /// 
        /// <summary>
        /// NTP サーバとの通信タイムアウト時間を設定または取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
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
        /// Start
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
        public void Start()
        {
            if (Enabled) return;
            GetAsync().ContinueWith(_ => { lock (_lock) _timer.Start(); });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        /// 
        /// <summary>
        /// 時刻の監視を中断します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop()
        {
            lock (_lock) _timer.Stop();
        }

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
            if (Enabled)
            {
                Stop();
                Start();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ResetEndPoint
        /// 
        /// <summary>
        /// 通信する NTP サーバを再設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void ResetEndPoint(string hostNameOrAddress)
        {
            ResetEndPoint(hostNameOrAddress, Client.DefaultPort);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ResetEndPoint
        /// 
        /// <summary>
        /// 通信する NTP サーバを再設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void ResetEndPoint(string hostNameOrAddress, int port)
        {
            lock (_lock) _client = new Client(hostNameOrAddress, port);
            Reset();
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
                    var packet = await _client.GetAsync();
                    if (packet == null || !packet.IsValid) continue;
                    Result = packet;
                    break;
                }
                catch (Exception err) { System.Diagnostics.Trace.TraceError(err.ToString()); }
                await Cube.TaskEx.Delay(_RetryInterval);
            }
        }

        #endregion

        #region Fields
        private Client _client = null;
        private Timer _timer = new Timer();
        private Packet _packet = null;
        private object _lock = new object();
        #endregion

        #region Constant fields
        private static readonly int _RetryCount = 5;
        private static readonly int _RetryInterval = 2000; // [ms]
        #endregion
    }
}
