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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cube.Tasks;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Client
    ///
    /// <summary>
    /// NTP サーバと通信するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Client : IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client() : this(DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(string server) : this(server, DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(string server, int port)
        {
            Timeout = TimeSpan.FromSeconds(5);
            Host = Dns.GetHostEntry(server);
            Port = port;
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

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~Client
        /// 
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~Client()
        {
            Dispose(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        /// 
        /// <summary>
        /// オブジェクトを破棄します。
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
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            //if (disposing) _socket.Dispose();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Task<Packet> GetAsync()
            => GetAsyncCore().Timeout(Timeout);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsyncCore
        /// 
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Task<Packet> GetAsyncCore() => TaskEx.Run(() =>
        {
            SendTo();
            return ReceiveFrom();
        });

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
            var addr = Host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            var endpoint = new IPEndPoint(addr, Port);
            var packet = new Ntp.Packet();
            var sent = _socket.SendTo(packet.RawData, endpoint);
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
        private Packet ReceiveFrom()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            var raw = new byte[48 + (32 + 128) / 8];
            var bytes = _socket.ReceiveFrom(raw, ref endpoint);
            return new Packet(raw);
        }

        #region Fields
        private bool _disposed = false;
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        #endregion

        #endregion
    }
}
