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
        NoWarning    = 0,   // 0 - No warning
        LastMinute61 = 1,   // 1 - Last minute has 61 seconds
        LastMinute59 = 2,   // 2 - Last minute has 59 seconds
        Alarm        = 3    // 3 - Alarm condition (clock not synchronized)
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
        Unknown          = 0,   // 0, 6, 7 - Reserved
        SymmetricActive  = 1,   // 1 - Symmetric active
        SymmetricPassive = 2,   // 2 - Symmetric pasive
        Client           = 3,   // 3 - Client
        Server           = 4,   // 4 - Server
        Broadcast        = 5,   // 5 - Broadcast
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
        Unspecified,            // 0 - unspecified or unavailable
        PrimaryReference,       // 1 - primary reference (e.g. radio-clock)
        SecondaryReference,     // 2-15 - secondary reference (via NTP or SNTP)
        Reserved                // 16-255 - reserved
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

        #region Constant variables
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

        #region Constant variables
        private static readonly double _CompensatingRate16 = 0x10000d;
        #endregion
    }
}
