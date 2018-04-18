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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpMonitor
    ///
    /// <summary>
    /// NTP サーバと定期的に通信を行い、時刻のずれを監視するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class NtpMonitor : NetworkMonitorBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor() : this(NtpClient.DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="server">NTP サーバ</param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor(string server) : this(server, NtpClient.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="server">NTP サーバ</param>
        /// <param name="port">ポート番号</param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor(string server, int port)
        {
            Interval = TimeSpan.FromHours(1);
            _server  = server;
            _port    = port;

            Timer.SubscribeAsync(WhenTick);
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
        protected IList<Func<TimeSpan, Task>> Subscriptions { get; } =
            new List<Func<TimeSpan, Task>>();

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
            if (Power.Mode == PowerModes.Suspend) return;
            Reset();
            TimeChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// SubscribeAsync
        ///
        /// <summary>
        /// データ受信時に非同期で実行する処理を登録します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable SubscribeAsync(Func<TimeSpan, Task> action)
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
        /* ----------------------------------------------------------------- */
        public IDisposable Subscribe(Action<TimeSpan> action) =>
            SubscribeAsync(e =>
            {
                action(e);
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
        /* ----------------------------------------------------------------- */
        protected virtual async Task PublishAsync(TimeSpan value)
        {
            foreach (var action in Subscriptions)
            {
                try { await action(value).ConfigureAwait(false); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
            }
        }

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
        /// <remarks>
        /// アプリケーションの起動直後等に NTP 通信を実行すると精度の
        /// 悪い傾向が確認されています。そのため、毎回の実行時の最初に
        /// 500 ミリ秒の待機時間を設ける事としています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private async Task WhenTick()
        {
            if (Subscriptions.Count <= 0) return;

            await Task.Delay(500).ConfigureAwait(false); // see ramarks

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    if (State != TimerState.Run) return;
                    var client = new NtpClient(Server, Port) { Timeout = Timeout };
                    var packet = await client.GetAsync();
                    if (packet != null && packet.IsValid)
                    {
                        await PublishAsync(packet.LocalClockOffset).ConfigureAwait(false);
                    }
                    else throw new ArgumentException("InvalidPacket");
                    break;
                }
                catch (Exception err)
                {
                    this.LogWarn(err.ToString(), err);
                    await Task.Delay(RetryInterval).ConfigureAwait(false);
                    this.LogDebug($"Retry\tCount:{i + 1}\tServer:{Server}");
                }
            }
        }

        #endregion

        #region Fields
        private string _server = string.Empty;
        private int _port = NtpClient.DefaultPort;
        #endregion
    }
}
