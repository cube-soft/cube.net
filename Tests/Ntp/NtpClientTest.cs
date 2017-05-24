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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientTest
    ///
    /// <summary>
    /// Cube.Net.Ntp.Client のテストクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class NtpClientTest : NetworkResource
    {
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
            using (var client = new Ntp.Client())
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
        [Test]
        public async Task GetAsync()
        {
            var pkt = await new Ntp.Client().GetAsync();
            Assert.That(pkt.IsValid,                       Is.True);
            Assert.That(pkt.LeapIndicator,                 Is.EqualTo(Cube.Net.Ntp.LeapIndicator.NoWarning));
            Assert.That(pkt.Version,                       Is.EqualTo(3));
            Assert.That(pkt.Mode,                          Is.EqualTo(Cube.Net.Ntp.Mode.Server));
            Assert.That(pkt.Stratum,                       Is.EqualTo(Cube.Net.Ntp.Stratum.SecondaryReference));
            Assert.That(pkt.PollInterval,                  Is.EqualTo(4));
            Assert.That(pkt.Precision,                     Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(pkt.RootDelay,                     Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(pkt.RootDispersion,                Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(pkt.ReferenceID,                   Is.Not.Null.And.Not.Empty);
            Assert.That(pkt.ReferenceTimestamp,            Is.Not.Null);
            Assert.That(pkt.KeyID,                         Is.Empty);
            Assert.That(pkt.MessageDigest,                 Is.Empty);
            Assert.That(pkt.NetworkDelay.TotalSeconds,     Is.GreaterThan(0.0).And.LessThan(1.0));
            Assert.That(pkt.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(60));
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
        public void NotFound_Throws()
            => Assert.That(
            () => new Ntp.Client("404.not.found"),
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
        public void Timeout_Throws()
            => Assert.That(
            async () =>
            {
                using (var client = new Ntp.Client())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(1);
                    var result = await client.GetAsync();
                    Assert.Fail("never reached");
                }
            },
            Throws.TypeOf<TimeoutException>()
        );
    }
}
