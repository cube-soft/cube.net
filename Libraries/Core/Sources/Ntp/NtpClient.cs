﻿/* ------------------------------------------------------------------------- */
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpClient
    ///
    /// <summary>
    /// NTP サーバと通信するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class NtpClient : DisposableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NtpClient
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpClient() : this(DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpClient
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpClient(string server) : this(server, DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpClient
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpClient(string server, int port)
        {
            Timeout  = TimeSpan.FromSeconds(5);
            Host     = Dns.GetHostEntry(server);
            Port     = port;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultServer
        ///
        /// <summary>
        /// NTP 通信のデフォルトサーバを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly string DefaultServer = "ntp.cube-soft.jp";

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultPort
        ///
        /// <summary>
        /// NTP 通信のデフォルトポート番号を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly int DefaultPort = 123;

        /* ----------------------------------------------------------------- */
        ///
        /// Host
        ///
        /// <summary>
        /// 通信する NTP サーバのホスト情報を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IPHostEntry Host { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Port
        ///
        /// <summary>
        /// 通信する NTP サーバのポート番号を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Port { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// NTP パケット受信時のタイムアウト時間を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Task<NtpPacket> GetAsync() => Task.Run(() =>
        {
            SendTo();
            return ReceiveFrom();
        });

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
            if (disposing) _socket.Dispose();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SendTo
        ///
        /// <summary>
        /// NTP サーバへパケットを送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SendTo()
        {
            var addr   = Host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            var ep     = new IPEndPoint(addr, Port);
            var packet = new Ntp.NtpPacket();

            _socket.SendTimeout = (int)Timeout.TotalMilliseconds;
            var sent = _socket.SendTo(packet.RawData, ep);
            if (sent != packet.RawData.Length) throw new SocketException();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReceiveFrom
        ///
        /// <summary>
        /// NTP サーバからパケットを受信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private NtpPacket ReceiveFrom()
        {
            var ep  = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            var raw = new byte[48 + (32 + 128) / 8];

            _socket.ReceiveTimeout = (int)Timeout.TotalMilliseconds;
            _socket.ReceiveFrom(raw, ref ep);

            return new NtpPacket(raw);
        }

        #endregion

        #region Fields
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        #endregion
    }
}
