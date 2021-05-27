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
    /// Timestamp
    ///
    /// <summary>
    /// Provides mutual conversion functions between NTP timestamps and
    /// DateTime objects.
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
    /// According to RFC4330, if the most significant bit is 0,
    /// the conversion is performed assuming that the time is between 2036
    /// and 2104.
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
        /// Converts an NTP timestamp to a DateTime object.
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
        /// Converts a DateTime object to an NTP timestamp.
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
        private static readonly DateTime _BaseTerm = new(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _ReverseTerm = new(2036, 2, 7, 6, 28, 16, 0, DateTimeKind.Utc);
        #endregion
    }
}
