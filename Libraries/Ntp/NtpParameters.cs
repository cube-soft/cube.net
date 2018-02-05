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

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// LeapIndicator
    ///
    /// <summary>
    /// 閏秒指示子 (LI: Leap Indicator) の状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum LeapIndicator : uint
    {
        /// <summary>警告なし</summary>
        NoWarning = 0,
        /// <summary>61 秒ある分</summary>
        LastMinute61 = 1,
        /// <summary>59 秒ある分</summary>
        LastMinute59 = 2,
        /// <summary>警告</summary>
        Alarm = 3,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Mode
    ///
    /// <summary>
    /// 動作モードの状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum Mode : uint
    {
        /// <summary>不明 (Reserved)</summary>
        Unknown = 0,   // 0, 6, 7 - Reserved
        /// <summary>Symmetric active</summary>
        SymmetricActive = 1,
        /// <summary>Symmetric pasive</summary>
        SymmetricPassive = 2,
        /// <summary>クライアント</summary>
        Client = 3,
        /// <summary>サーバ</summary>
        Server = 4,
        /// <summary>ブロードキャスト</summary>
        Broadcast = 5,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Stratum
    ///
    /// <summary>
    /// 階層の状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum Stratum
    {
        /// <summary>Unspecified or Unavailable</summary>
        Unspecified,
        /// <summary>Primary reference (e.g. radio-clock)</summary>
        PrimaryReference,
        /// <summary>Secondary reference (via NTP or SNTP)</summary>
        SecondaryReference,
        /// <summary>reserved (16-255)</summary>
        Reserved,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Timestamp
    ///
    /// <summary>
    /// NTP タイムスタンプと DateTime タイムオブジェクトの相互変換機能を
    /// 提供するためのクラスです。
    /// </summary>
    ///
    /// <remarks>
    /// NTP Timestamp Format (as described in RFC 2030)
    ///                         1                   2                   3
    ///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                           Seconds                             |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                  Seconds Fraction (0-padded)                  |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///
    /// 尚、RFC 4330 にしたがって最上位ビットが 0 の場合は時刻が 2036 年
    /// から 2104 年の間であると見なして変換を行います。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public static class Timestamp
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// NTP タイムスタンプから DateTime オブジェクトへ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime Convert(long timestamp)
        {
            var seconds = (uint)(timestamp >> 32);
            var fraction = (double)(timestamp & uint.MaxValue);

            var milliseconds = (double)seconds * 1000 + (fraction * 1000) / (_CompensatingRate32);
            var origin = ((seconds & _ConpensatingRate31) == 0) ? _ReverseTerm : _BaseTerm;

            return origin + TimeSpan.FromMilliseconds(milliseconds);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// DateTime オブジェクトから NTP タイムスタンプへ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static long Convert(DateTime datetime)
        {
            var origin = (_ReverseTerm <= datetime) ? _ReverseTerm : _BaseTerm;
            var ticks = (datetime - origin).TotalMilliseconds;

            var seconds = (uint)((datetime - origin).TotalSeconds);
            var fraction = (ulong)((ticks % 1000) * _CompensatingRate32 / 1000);

            return (long)(((ulong)seconds << 32) | fraction);
        }

        #endregion

        #region Fields
        private static readonly ulong _CompensatingRate32 = 0x100000000L;
        private static readonly uint _ConpensatingRate31 = 0x80000000u;
        private static readonly DateTime _BaseTerm = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _ReverseTerm = new DateTime(2036, 2, 7, 6, 28, 16, 0, DateTimeKind.Utc);
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// FixedPoint
    ///
    /// <summary>
    /// 符号付き 32bit 固定小数点数から double への変換機能を提供するための
    /// クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class FixedPoint
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ToDouble
        ///
        /// <summary>
        /// 符号付き 32bit 固定小数点数から double へ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static double ToDouble(Int32 value)
        {
            var number = (Int16)(value >> 16);
            var fraction = (UInt16)(value & Int16.MaxValue);
            return number + fraction / _CompensatingRate16;
        }

        #endregion

        #region Fields
        private static readonly double _CompensatingRate16 = 0x10000d;
        #endregion
    }
}
