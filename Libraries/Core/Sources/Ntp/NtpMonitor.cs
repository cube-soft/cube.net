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
using System.Threading.Tasks;
using Cube.Collections;
using Cube.Mixin.Logging;
using Microsoft.Win32;

namespace Cube.Net.Ntp
{
    #region NtpMonitor

    /* --------------------------------------------------------------------- */
    ///
    /// NtpMonitor
    ///
    /// <summary>
    /// Provides functionality to periodically communicate with the NTP
    /// server.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class NtpMonitor : NetworkMonitor
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the NtpMonitor class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor() : this(NtpClient.DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the NtpMonitor class with the
        /// specified NTP server.
        /// </summary>
        ///
        /// <param name="server">NTP server.</param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor(string server) : this(server, NtpClient.DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the NtpMonitor class with the
        /// specified arguments.
        /// </summary>
        ///
        /// <param name="server">NTP server.</param>
        /// <param name="port">
        /// Port number to communicate with the NTP server.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMonitor(string server, int port)
        {
            Interval = TimeSpan.FromHours(1);
            _server  = server;
            _port    = port;

            SystemEvents.TimeChanged += (s, e) => OnTimeChanged();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Server
        ///
        /// <summary>
        /// Gets or sets the NTP server (host name or IP address).
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
        /// Gets or sets the port number.
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

        #endregion

        #region Events

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
        ///
        /// <summary>
        /// Sets the specified asynchronous action to the monitor.
        /// </summary>
        ///
        /// <param name="callback">Asynchronous user action.</param>
        ///
        /// <returns>Object to remove from the subscription.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable Subscribe(NtpAsyncAction callback) =>
            _subscription.Subscribe(callback);

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// Publishes the time gap to the subscribers.
        /// </summary>
        ///
        /// <param name="value">
        /// Time difference between local and NTP server.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual async Task Publish(TimeSpan value)
        {
            foreach (var cb in _subscription)
            {
                try { await cb(value).ConfigureAwait(false); }
                catch (Exception err) { GetType().LogWarn(err); }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnTimeChanged
        ///
        /// <summary>
        /// Occurs when the system clock is changed.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnTimeChanged()
        {
            if (Power.Mode != PowerModes.Suspend) Reset();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnTick
        ///
        /// <summary>
        /// Occurs when the timer is expired.
        /// </summary>
        ///
        /// <remarks>
        /// アプリケーションの起動直後等に NTP 通信を実行すると精度の
        /// 悪い傾向が確認されています。そのため、毎回の実行時の最初に
        /// 500 ミリ秒の待機時間を設ける事としています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task OnTick()
        {
            if (_subscription.Count <= 0) return;
            if (!Network.Available)
            {
                GetType().LogDebug("Network not available");
                return;
            }

            await Task.Delay(500).ConfigureAwait(false); // see remarks

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    if (State != TimerState.Run) return;
                    var client = new NtpClient(Server, Port) { Timeout = Timeout };
                    var packet = await client.GetAsync();
                    if (packet != null && packet.Valid)
                    {
                        await Publish(packet.LocalClockOffset).ConfigureAwait(false);
                    }
                    else throw new ArgumentException("InvalidPacket");
                    return;
                }
                catch (Exception err)
                {
                    GetType().LogWarn(Server, $"{err.Message} ({i + 1}/{RetryCount})");
                    await Task.Delay(RetryInterval).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Fields
        private readonly Subscription<NtpAsyncAction> _subscription = new();
        private string _server = string.Empty;
        private int _port = NtpClient.DefaultPort;
        #endregion
    }

    #endregion

    #region NtpAsyncAction

    /* --------------------------------------------------------------------- */
    ///
    /// NtpAsyncAction
    ///
    /// <summary>
    /// Represents the method to invoke as an asynchronous method.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public delegate Task NtpAsyncAction(TimeSpan value);

    #endregion
}
