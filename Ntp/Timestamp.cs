/* ------------------------------------------------------------------------- */
///
/// Timestamp.cs
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
    /// Cube.Net.Ntp.Timestamp
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
    public abstract class Timestamp
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ToDateTime
        /// 
        /// <summary>
        /// NTP タイムスタンプから DateTime オブジェクトへ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime ToDateTime(Int64 timestamp)
        {
            var seconds = (UInt32)(timestamp >> 32);
            var fraction = (double)((UInt64)(timestamp & UInt32.MaxValue));

            var milliseconds = (double)seconds * 1000 + (fraction * 1000) / (_CompensatingRate32);
            var origin = ((seconds & _ConpensatingRate31) == 0) ? _ReverseTerm : _BaseTerm;

            return origin + TimeSpan.FromMilliseconds(milliseconds);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ToTimestamp
        /// 
        /// <summary>
        /// DateTime オブジェクトから NTP タイムスタンプへ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static Int64 ToTimestamp(DateTime datetime)
        {
            var origin = (_ReverseTerm <= datetime) ? _ReverseTerm : _BaseTerm;
            var ticks = (datetime - origin).TotalMilliseconds;

            var seconds = (UInt32)((datetime - origin).TotalSeconds);
            var fraction = (UInt64)((ticks % 1000) * _CompensatingRate32 / 1000);

            return (Int64)(((UInt64)seconds << 32) | fraction);
        }

        #region Constant variables
        private static readonly UInt64 _CompensatingRate32 = 0x100000000L;
        private static readonly UInt32 _ConpensatingRate31 = 0x80000000u;
        private static readonly DateTime _BaseTerm = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _ReverseTerm = new DateTime(2036, 2, 7, 6, 28, 16, 0, DateTimeKind.Utc);
        #endregion
    }
}
