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
        public void Properties_Default()
        {
            var client = new Ntp.Client();

            Assert.That(client.Host.HostName, Is.EqualTo("ntp.cube-soft.jp"));
            Assert.That(client.Host.AddressList.Length, Is.AtLeast(1));
            Assert.That(client.Port, Is.EqualTo(123));
            Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LocalClockOffset
        /// 
        /// <summary>
        /// 非同期で時刻を取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(60)]
        public async void LocalClockOffset(int delta)
        {
            var result = await new Ntp.Client().GetAsync();
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(delta));
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
        [Ignore("NUnit for .NET 3.5 does not support async/await")]
        public void Timeout_Throws()
            => Assert.That(
            async () => await new Ntp.Client { Timeout = TimeSpan.FromMilliseconds(1) }.GetAsync(),
            Throws.TypeOf<TimeoutException>()
        );
    }
}
