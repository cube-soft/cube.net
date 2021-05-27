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
using System.Net;
using System.Text;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpPacket
    ///
    /// <summary>
    /// Represents the NTP packet.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class NtpPacket
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NtpPacket
        ///
        /// <summary>
        /// Initializes a new instance of the NtpPacket class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpPacket()
        {
            CreationTime = DateTime.Now;
            RawData = CreateNewPacket();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NtpPacket
        ///
        /// <summary>
        /// Initializes a new instance of the NtpPacket class with the
        /// specified raw data.
        /// </summary>
        ///
        /// <param name="src">Raw data.</param>
        ///
        /* ----------------------------------------------------------------- */
        public NtpPacket(byte[] src)
        {
            if (src.Length < _MinimumPacketSize)
            {
                throw new ArgumentException($"packet should be more than {_MinimumPacketSize} bytes");
            }
            CreationTime = DateTime.Now;
            RawData = src;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// LeapIndicator
        ///
        /// <summary>
        /// Gets the leap indicator.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LeapIndicator LeapIndicator => (byte)(RawData[0] >> 6) switch
        {
            0 => LeapIndicator.NoWarning,
            1 => LeapIndicator.LastMinute61,
            2 => LeapIndicator.LastMinute59,
            3 => LeapIndicator.Alarm,
            _ => LeapIndicator.Alarm,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// Gets the version number.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public uint Version => (uint)((RawData[0] & 0x38) >> 3);

        /* ----------------------------------------------------------------- */
        ///
        /// Mode
        ///
        /// <summary>
        /// Get the operation mode.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpMode Mode => (byte)(RawData[0] & 0x7) switch
        {
            0 => NtpMode.Unknown,
            1 => NtpMode.SymmetricActive,
            2 => NtpMode.SymmetricPassive,
            3 => NtpMode.Client,
            4 => NtpMode.Server,
            5 => NtpMode.Broadcast,
            6 => NtpMode.Unknown,
            7 => NtpMode.Unknown,
            _ => NtpMode.Unknown
        };

        /* ----------------------------------------------------------------- */
        ///
        /// Stratum
        ///
        /// <summary>
        /// Gets the Stratum value.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Stratum Stratum => RawData[1] switch
        {
            0     => Stratum.Unspecified,
            1     => Stratum.PrimaryReference,
            <= 15 => Stratum.SecondaryReference,
            _     => Stratum.Reserved
        };

        /* ----------------------------------------------------------------- */
        ///
        /// PollInterval
        ///
        /// <summary>
        /// Gets the polling interval in seconds.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public uint PollInterval
        {
            get
            {
                int value = (sbyte)RawData[2];
                return value > 0 ? (uint)Math.Pow(2, value - 1) : 0;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Precision
        ///
        /// <summary>
        /// Get the precision of the clock in seconds.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double Precision => Math.Pow(2, (sbyte)RawData[3]);

        /* ----------------------------------------------------------------- */
        ///
        /// RootDelay
        ///
        /// <summary>
        /// 一次参照源までの往復遅延時間の合計を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double RootDelay
        {
            get
            {
                var value = BitConverter.ToInt32(RawData, 4);
                return FixedPoint.ToDouble(IPAddress.NetworkToHostOrder(value));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RootDispersion
        ///
        /// <summary>
        /// Gets the relative error to the primary reference source.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double RootDispersion => FixedPoint.ToDouble(
            IPAddress.NetworkToHostOrder(BitConverter.ToInt32(RawData, 8))
        );

        /* ----------------------------------------------------------------- */
        ///
        /// ReferenceID
        ///
        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string ReferenceID => Stratum switch
        {
            Stratum.Unspecified or
            Stratum.PrimaryReference   => Encoding.ASCII.GetString(RawData, 12, 4).TrimEnd(new char()),
            Stratum.SecondaryReference => GetSecondaryReferenceID(),
            _ => string.Empty,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// ReferenceTimestamp
        ///
        /// <summary>
        /// Get the reference timestamp (last clock reference time of the
        /// primary time source).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime ReferenceTimestamp => Timestamp.Convert(
            IPAddress.NetworkToHostOrder(BitConverter.ToInt64(RawData, 16))
        ).ToLocalTime();

        /* ----------------------------------------------------------------- */
        ///
        /// OriginateTimestamp
        ///
        /// <summary>
        /// Get the originate timestamp (the time the request was sent out
        /// from the client to the server).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime OriginateTimestamp => Timestamp.Convert(
            IPAddress.NetworkToHostOrder(BitConverter.ToInt64(RawData, 24))
        ).ToLocalTime();

        /* ----------------------------------------------------------------- */
        ///
        /// ReceiveTimestamp
        ///
        /// <summary>
        /// Get the receive timestamp (the time when the request arrived
        /// at the server).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime ReceiveTimestamp => Timestamp.Convert(
            IPAddress.NetworkToHostOrder(BitConverter.ToInt64(RawData, 32))
        ).ToLocalTime();

        /* ----------------------------------------------------------------- */
        ///
        /// TransmitTimestamp
        ///
        /// <summary>
        /// 送信タイムスタンプ（サーバからクライアントに応答が発信された
        /// 時刻）を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime TransmitTimestamp => Timestamp.Convert(
            IPAddress.NetworkToHostOrder(BitConverter.ToInt64(RawData, 40))
        ).ToLocalTime();

        /* ----------------------------------------------------------------- */
        ///
        /// KeyID
        ///
        /// <summary>
        /// Gets the key ID.
        /// 鍵識別子を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string KeyID => RawData.Length >= 52 ?
            Encoding.ASCII.GetString(RawData, 48, 4).TrimEnd(new char()) :
            string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// MessageDigest
        ///
        /// <summary>
        /// Gets the message digest.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string MessageDigest => RawData.Length >= 68 ?
            Encoding.ASCII.GetString(RawData, 52, 16).TrimEnd(new char()) :
            string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// Valid
        ///
        /// <summary>
        /// Gets a value indicating whether it is a valid NTP packet.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Valid => RawData != null && LeapIndicator != LeapIndicator.Alarm;

        /* ----------------------------------------------------------------- */
        ///
        /// RawData
        ///
        /// <summary>
        /// Gets the raw packet data.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public byte[] RawData { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// CreationTime
        ///
        /// <summary>
        /// Get the creation time of the packet object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime CreationTime { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkDelay
        ///
        /// <summary>
        /// Gets the communication delay time (RTT).
        /// </summary>
        ///
        /// <remarks>
        /// If the time when the client sends the request is t_s,
        /// the time when the server receives the client's request is T_r,
        /// the time when the server sends the response is T_s, and
        /// the time when the client receives the server's response is t_r,
        /// the communication delay time δ is expressed by the following
        /// equation:
        ///
        /// δ = (t_r - t_s) - (T_s - T_r)
        ///
        /// This is equivalent to the RTT of the packet minus the processing
        /// time of the server.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan NetworkDelay =>
            (CreationTime - OriginateTimestamp) - (TransmitTimestamp - ReceiveTimestamp);

        /* ----------------------------------------------------------------- */
        ///
        /// LocalClockOffset
        ///
        /// <summary>
        /// Gets the difference (delayed time) from the NTP server.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// The time when the client sent the request is t_s,
        /// the time when the server received the client's request is T_r,
        /// the time when the server sent the response is T_s, and
        /// the time when the client received the server's response is t_r.
        /// Assuming that there is no difference in the communication time
        /// between the outward and the inward transmissions,
        /// the delay time θ of the client's clock can be expressed by
        /// the following equation:
        ///
        /// θ = (T_s + T_r) / 2 - (t_s + t_r) / 2
        ///
        /// This is the average difference between the sending and receiving
        /// times of packets from the server and client.
        /// For implementation purposes, we will transform the formula as
        /// follows:
        ///
        /// θ = ((T_r - t_s) + (T_s - t_r)) / 2
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan LocalClockOffset
        {
            get
            {
                var ticks = ((ReceiveTimestamp - OriginateTimestamp) + (TransmitTimestamp - CreationTime)).Ticks;
                return new TimeSpan(ticks / 2);
            }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// CreateNewPacket
        ///
        /// <summary>
        /// Creates a new packet for transmission.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private byte[] CreateNewPacket()
        {
            var dest = new byte[_MinimumPacketSize];
            dest[0] = (byte)(_ClientLeapIndicator | _ClientVersion << 3 | _ClientMode);

            var timestamp = Timestamp.Convert(CreationTime.ToUniversalTime());
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timestamp));
            Array.ConstrainedCopy(bytes, 0, dest, 40, 8);

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetSecondaryReferenceID
        ///
        /// <summary>
        /// Gets the Reference Identifier when the hierarchy is 2-15th order
        /// reference (referenced via NTP or SNTP server).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetSecondaryReferenceID()
        {
            switch (Version)
            {
                case 3:
                    var ipaddr = new IPAddress(new[] { RawData[12], RawData[13], RawData[14], RawData[15] });
                    return ipaddr.ToString();
                case 4:
                    return string.Empty;
                default:
                    break;
            }
            return string.Empty;
        }

        #endregion

        #region Fields
        private static readonly byte _ClientLeapIndicator = 0x00;
        private static readonly byte _ClientMode = 0x03;
        private static readonly int _ClientVersion = 3;
        private static readonly int _MinimumPacketSize = 48;
        #endregion
    }
}
