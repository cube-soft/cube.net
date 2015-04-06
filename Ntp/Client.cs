/* ------------------------------------------------------------------------- */
///
/// Client.cs
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Ntp.Client
    ///
    /// <summary>
    /// NTP でサーバと通信するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Client
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
        public Client(string host_or_ipaddr, int port = 123)
        {
            Host = Dns.GetHostEntry(host_or_ipaddr);
            _endpoint = new IPEndPoint(Host.AddressList[0], port);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Client(IPAddress ipaddr, int port = 123) : this(ipaddr.ToString(), port) { }
        
        #endregion

        #region Properties

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
        public int Port
        {
            get { return _endpoint.Port; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReceiveTimeout
        /// 
        /// <summary>
        /// NTP パケット受信時のタイムアウト時間を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan ReceiveTimeout { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Receive
        /// 
        /// <summary>
        /// NTP サーバと通信を行い、NTP パケットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<Packet> Receive()
        {
            var socket = CreateSocket();
            await SendTo(socket);
            return await ReceiveFrom(socket);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// CreateSocket
        /// 
        /// <summary>
        /// 新しい UDP ソケットを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Socket CreateSocket()
        {
            var dest = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            dest.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, (int)ReceiveTimeout.TotalMilliseconds);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SendTo
        /// 
        /// <summary>
        /// NTP サーバへパケットを送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Task SendTo(Socket socket)
        {
            return Task.Factory.StartNew(() =>
            {
                var packet = new Ntp.Packet();
                socket.SendTo(packet.RawData, _endpoint);
            });
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
        private Task<Packet> ReceiveFrom(Socket socket)
        {
            return Task<Packet>.Factory.StartNew(() =>
            {
                byte[] raw = new byte[48 + (32 + 128) / 8];
                EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                var bytes = socket.ReceiveFrom(raw, ref endpoint);
                return new Packet(raw);
            });
        }

        #endregion

        #region Fields
        private IPEndPoint _endpoint = null;
        #endregion
    }
}
