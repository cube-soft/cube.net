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
    /// NTP パケットを表すクラスです。
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
        /// オブジェクトを初期化します。
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
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NtpPacket(byte[] rawdata)
        {
            if (rawdata.Length < _MinimumPacketSize)
            {
                throw new ArgumentException($"packet should be more than {_MinimumPacketSize} bytes");
            }
            CreationTime = DateTime.Now;
            RawData = rawdata;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// LeapIndicator
        ///
        /// <summary>
        /// 閏秒指示子を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LeapIndicator LeapIndicator
        {
            get
            {
                var value = (byte)(RawData[0] >> 6);
                switch (value)
                {
                    case 0: return Ntp.LeapIndicator.NoWarning;
                    case 1: return Ntp.LeapIndicator.LastMinute61;
                    case 2: return Ntp.LeapIndicator.LastMinute59;
                    case 3: return Ntp.LeapIndicator.Alarm;
                    default: break;
                }
                return Ntp.LeapIndicator.Alarm;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// バージョン番号を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public uint Version
        {
            get { return (uint)((RawData[0] & 0x38) >> 3); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Mode
        ///
        /// <summary>
        /// 動作モードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Mode Mode
        {
            get
            {
                var value = (byte)(RawData[0] & 0x7);
                switch (value)
                {
                    case 0: return Ntp.Mode.Unknown;
                    case 1: return Ntp.Mode.SymmetricActive;
                    case 2: return Ntp.Mode.SymmetricPassive;
                    case 3: return Ntp.Mode.Client;
                    case 4: return Ntp.Mode.Server;
                    case 5: return Ntp.Mode.Broadcast;
                    case 6: return Ntp.Mode.Unknown;
                    case 7: return Ntp.Mode.Unknown;
                    default: break;
                }
                return Ntp.Mode.Unknown;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stratum
        ///
        /// <summary>
        /// 階層を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Stratum Stratum
        {
            get
            {
                var value = (byte)RawData[1];
                if (value == 0) return Ntp.Stratum.Unspecified;
                else if (value == 1) return Ntp.Stratum.PrimaryReference;
                else if (value <= 15) return Ntp.Stratum.SecondaryReference;
                else return Ntp.Stratum.Reserved;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PollInterval
        ///
        /// <summary>
        /// 連続するメッセージの最大間隔を秒単位でを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public uint PollInterval
        {
            get
            {
                int value = (sbyte)RawData[2];
                if (value <= 0) return 0;
                else return (uint)Math.Pow(2, value - 1);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Precision
        ///
        /// <summary>
        /// 時計の精度を秒単位で取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double Precision
        {
            get { return Math.Pow(2, (sbyte)RawData[3]); }
        }

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
        /// 一次参照源までの相対的な誤差を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double RootDispersion
        {
            get
            {
                var value = BitConverter.ToInt32(RawData, 8);
                return FixedPoint.ToDouble(IPAddress.NetworkToHostOrder(value));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReferenceID
        ///
        /// <summary>
        /// 参照識別子 (Reference Identifier) を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string ReferenceID
        {
            get
            {
                switch (Stratum)
                {
                    case Ntp.Stratum.Unspecified:
                    case Ntp.Stratum.PrimaryReference:
                        return Encoding.ASCII.GetString(RawData, 12, 4).TrimEnd(new char());
                    case Ntp.Stratum.SecondaryReference:
                        return GetSecondaryReferenceID();
                    default:
                        break;
                }
                return string.Empty;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReferenceTimestamp
        ///
        /// <summary>
        /// 参照タイムスタンプ（一次時刻情報源の最終時計参照時刻）を取得
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime ReferenceTimestamp
        {
            get
            {
                var value = BitConverter.ToInt64(RawData, 16);
                return Timestamp.Convert(IPAddress.NetworkToHostOrder(value)).ToLocalTime();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OriginateTimestamp
        ///
        /// <summary>
        /// 開始タイムスタンプ（クライアントからサーバへリクエストを発信した
        /// 時刻）を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime OriginateTimestamp
        {
            get
            {
                var value = BitConverter.ToInt64(RawData, 24);
                return Timestamp.Convert(IPAddress.NetworkToHostOrder(value)).ToLocalTime();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReceiveTimestamp
        ///
        /// <summary>
        /// 受信タイムスタンプ（サーバへリクエストが到着した時刻）を取得
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime ReceiveTimestamp
        {
            get
            {
                var value = BitConverter.ToInt64(RawData, 32);
                return Timestamp.Convert(IPAddress.NetworkToHostOrder(value)).ToLocalTime();
            }
        }

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
        public DateTime TransmitTimestamp
        {
            get
            {
                var value = BitConverter.ToInt64(RawData, 40);
                return Timestamp.Convert(IPAddress.NetworkToHostOrder(value)).ToLocalTime();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// KeyID
        ///
        /// <summary>
        /// 鍵識別子を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string KeyID
        {
            get
            {
                if (RawData.Length < 52) return string.Empty;
                else return Encoding.ASCII.GetString(RawData, 48, 4).TrimEnd(new char());
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MessageDigest
        ///
        /// <summary>
        /// メッセージダイジェストを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string MessageDigest
        {
            get
            {
                if (RawData.Length < 68) return string.Empty;
                else return Encoding.ASCII.GetString(RawData, 52, 16).TrimEnd(new char());
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsValid
        ///
        /// <summary>
        /// NTP パケットとして有効なものであるかどうか表す値を判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsValid
        {
            get { return (RawData != null && LeapIndicator != Ntp.LeapIndicator.Alarm); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RawData
        ///
        /// <summary>
        /// 生のパケットデータを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public byte[] RawData { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// CreationTime
        ///
        /// <summary>
        /// この Packet オブジェクトが生成された時刻を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime CreationTime { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkDelay
        ///
        /// <summary>
        /// 通信遅延時間 (RTT) を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// クライアントがリクエストを送信した時刻を t_s 、サーバが
        /// クライアントのリクエストを受信した時刻を T_r 、サーバが
        /// レスポンスを送信した時刻を T_s 、クライアントがサーバの
        /// レスポンスを受信した時刻を t_r とすると、通信遅延時間 δ は、
        /// 以下の式で表されます。
        ///
        /// δ = (t_r - t_s) - (T_s - T_r)
        ///
        /// これは、パケットの往復時間からサーバの処理時間を引いたものに
        /// 相当します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan NetworkDelay
        {
            get { return (CreationTime - OriginateTimestamp) - (TransmitTimestamp - ReceiveTimestamp); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LocalClockOffset
        ///
        /// <summary>
        /// NTP サーバーとの差異（遅延時刻）を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// クライアントがリクエストを送信した時刻を t_s 、サーバが
        /// クライアントのリクエストを受信した時刻を T_r 、サーバが
        /// レスポンスを送信した時刻を T_s 、クライアントがサーバの
        /// レスポンスを受信した時刻を t_r とします。
        /// この時、往路と復路の通信時間に差がないと仮定すれば、クライアント
        /// の時計の遅延時間 θ は、以下の式で表される。
        ///
        /// θ = (T_s + T_r) / 2 - (t_s + t_r) / 2
        ///
        /// これは、サーバ、クライアントの、パケットの送信時刻、受信時刻の
        /// 平均の差を表します。尚、実装上の都合として、以下のように式変形
        /// して計算を行います。
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
        /// 送信用の新しいパケットを生成します。
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
        /// 階層が 2-15 次参照 (NTP、SNTP サーバを経由して参照している) の
        /// 場合の参照識別子 (Reference Identifier) を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// TODO: バージョン 4 の場合の変換を実装する。RFC 2030 によると、
        /// NTP バージョン 4の従属的なサーバーでは基準源の最終送信タイム
        /// スタンプの下位 32 ビットになるとの事。
        /// </remarks>
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
