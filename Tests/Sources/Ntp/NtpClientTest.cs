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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpClientTest
    ///
    /// <summary>
    /// NtpClient のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpClientTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Properties_Default
        ///
        /// <summary>
        /// 各種プロパティの初期値を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Properties_Default()
        {
            using (var client = new Ntp.NtpClient())
            {
                Assert.That(client.Host.HostName, Is.EqualTo("www268.ziyu.net"));
                Assert.That(client.Host.AddressList.Length, Is.AtLeast(1));
                Assert.That(client.Port, Is.EqualTo(123));
                Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// 非同期で NTP サーバと通信するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCaseSource(nameof(GetAsync_TestCases))]
        public async Task GetAsync(string src, uint version, uint poll, Cube.Net.Ntp.Stratum stratum)
        {
            using (var client = new Ntp.NtpClient(src))
            {
                var pkt = await client.GetAsync();
                Assert.That(pkt.IsValid,            Is.True);
                Assert.That(pkt.LeapIndicator,      Is.EqualTo(Cube.Net.Ntp.LeapIndicator.NoWarning));
                Assert.That(pkt.Version,            Is.EqualTo(version));
                Assert.That(pkt.Mode,               Is.EqualTo(Cube.Net.Ntp.Mode.Server));
                Assert.That(pkt.Stratum,            Is.EqualTo(stratum));
                Assert.That(pkt.PollInterval,       Is.EqualTo(poll));
                Assert.That(pkt.Precision,          Is.GreaterThan(0.0).And.LessThan(1.0));
                Assert.That(pkt.RootDelay,          Is.GreaterThanOrEqualTo(0.0));
                Assert.That(pkt.RootDispersion,     Is.GreaterThanOrEqualTo(0.0));
                Assert.That(pkt.ReferenceID,        Is.Not.Null.And.Not.Empty);
                Assert.That(pkt.ReferenceTimestamp, Is.Not.Null);
                Assert.That(pkt.KeyID,              Is.Empty);
                Assert.That(pkt.MessageDigest,      Is.Empty);
                Assert.That(pkt.NetworkDelay.TotalSeconds,     Is.GreaterThan(0.0).And.LessThan(1.0));
                Assert.That(pkt.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(60));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_TestCases
        ///
        /// <summary>
        /// GetAsync のテスト用データを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<TestCaseData> GetAsync_TestCases
        {
            get
            {
                yield return new TestCaseData(
                    "ntp.cube-soft.jp",
                    3u,
                    4u,
                    Cube.Net.Ntp.Stratum.SecondaryReference
                );
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NotFound_Throws
        ///
        /// <summary>
        /// 存在しない NTP サーバを指定した時のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void NotFound_Throws() => Assert.That(
            () => new Ntp.NtpClient("404.not.found"),
            Throws.TypeOf<System.Net.Sockets.SocketException>()
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout_Throws
        ///
        /// <summary>
        /// タイムアウト処理のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Timeout_Throws() => Assert.That(
            () =>
            {
                using (var client = new Ntp.NtpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(1);
                    client.GetAsync().Wait();
                }
            },
            Throws.TypeOf<AggregateException>()
                  .And
                  .InnerException
                  .TypeOf<TimeoutException>()
        );

        #endregion
    }
}
