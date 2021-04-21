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
    /// Provides functionality to communicate with the NTP server.
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
        /// Initializes a new instance of the NtpClient class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpClient() : this(DefaultServer) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpClient
        ///
        /// <summary>
        /// Initializes a new instance of the NtpClient class with the
        /// specified server.
        /// </summary>
        ///
        /// <param name="server">NTP server.</param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpClient(string server) : this(server, DefaultPort) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpClient
        ///
        /// <summary>
        /// Initializes a new instance of the NtpClient class with the
        /// specified arguments.
        /// </summary>
        ///
        /// <param name="server">NTP server.</param>
        /// <param name="port">
        /// Port number to communicate with the NTP server.
        /// </param>
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
        /// Gets the default NTP server.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly string DefaultServer = "time.windows.com";

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultPort
        ///
        /// <summary>
        /// Gets the default NTP port number.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly int DefaultPort = 123;

        /* ----------------------------------------------------------------- */
        ///
        /// Host
        ///
        /// <summary>
        /// Gets the host information of the NTP server.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IPHostEntry Host { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Port
        ///
        /// <summary>
        /// Gets the port number to communicate with the NTP server.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Port { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// Gets or sets the receive timeout for a NTP packet.
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
        /// Gets the NTP response from the provided NTP server.
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
        /// Releases the unmanaged resources used by the WakeableTimer
        /// and optionally releases the managed resources.
        /// </summary>
        ///
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
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
        /// Sends the NTP request to the provided NTP server.
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
        /// Receives the NTP response from the provided NTP server.
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
        private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        #endregion
    }
}
