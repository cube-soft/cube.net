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
using NUnit.Framework;
using System;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpTimestampTest
    ///
    /// <summary>
    /// Ntp.Timestamp のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpTimestampTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ToDateTime
        ///
        /// <summary>
        /// NTP タイムスタンプを DateTime オブジェクトに変更するテストを
        /// 行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(2036, 2, 7, 6, 28, 15, -0x100000000L)]
        [TestCase(2036, 2, 7, 6, 28, 16, 0)]
        public void ToDateTime(int y, int m, int d, int hh, int mm, int ss, long ts)
        {
            var converted = Ntp.Timestamp.Convert(ts);
            Assert.That(converted.Year,   Is.EqualTo(y));
            Assert.That(converted.Month,  Is.EqualTo(m));
            Assert.That(converted.Day,    Is.EqualTo(d));
            Assert.That(converted.Hour,   Is.EqualTo(hh));
            Assert.That(converted.Minute, Is.EqualTo(mm));
            Assert.That(converted.Second, Is.EqualTo(ss));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ToTimestamp
        ///
        /// <summary>
        /// DateTime オブジェクトを NTP タイムスタンプに変更するテストを
        /// 行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(2036, 2, 7, 6, 28, 15, -0x100000000L)]
        [TestCase(2036, 2, 7, 6, 28, 16, 0)]
        public void ToTimestamp(int y, int m, int d, int hh, int mm, int ss, long ts)
        {
            var src = new DateTime(y, m, d, hh, mm, ss, DateTimeKind.Utc);
            var converted = Ntp.Timestamp.Convert(src);
            Assert.That(converted, Is.EqualTo(ts));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// 引数に指定された日時をいったん NTP タイムスタンプに変換し、
        /// 再度 DateTime オブジェクトに変換するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(1970,  1,  1,  0,  0,  0,   0)]
        [TestCase(1999, 12, 31, 23, 59, 59, 999)]
        [TestCase(2000,  1,  1,  0,  0,  0,   0)]
        [TestCase(2036,  2,  7,  6, 28, 15, 999)]
        [TestCase(2036,  2,  7,  6, 28, 16,   0)]
        [TestCase(2104,  1,  1,  0,  0,  0,   0)]
        public void Convert(int y, int m, int d, int hh, int mm, int ss, int ms)
        {
            var src = new DateTime(y, m, d, hh, mm, ss, ms, DateTimeKind.Utc);
            var tmp = Ntp.Timestamp.Convert(src);
            var converted = Ntp.Timestamp.Convert(tmp);
            Assert.That(converted.Year,        Is.EqualTo(y));
            Assert.That(converted.Month,       Is.EqualTo(m));
            Assert.That(converted.Day,         Is.EqualTo(d));
            Assert.That(converted.Hour,        Is.EqualTo(hh));
            Assert.That(converted.Minute,      Is.EqualTo(mm));
            Assert.That(converted.Second,      Is.EqualTo(ss));
            Assert.That(converted.Millisecond, Is.EqualTo(ms));
        }
    }
}
