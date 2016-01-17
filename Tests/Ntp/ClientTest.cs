/* ------------------------------------------------------------------------- */
///
/// ClientTest.cs
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

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientTest
    ///
    /// <summary>
    /// Cube.Net.Ntp.Client のテストをするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ClientTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// NTP サーバとの通信のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetAsync()
        {
            var client = new Cube.Net.Ntp.Client("ntp.cube-soft.jp");
            Assert.That(client.Host.HostName, Is.Not.Null.Or.Empty);
            Assert.That(client.Host.AddressList.Length, Is.AtLeast(1));
            Assert.That(client.Port, Is.EqualTo(123));
            Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));

            var result = await client.GetAsync();
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(60));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TimeoutException
        /// 
        /// <summary>
        /// タイムアウト処理のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TimeoutException()
        {
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var client = new Cube.Net.Ntp.Client("ntp.cube-soft.jp");
                client.Timeout = TimeSpan.FromMilliseconds(1);
                client.GetAsync().Wait();
            });
            Assert.That(ex.InnerException, Is.TypeOf<TimeoutException>());
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SocketException
        /// 
        /// <summary>
        /// 存在しない NTP サーバを指定した時のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void SocketException()
        {
            Assert.Throws<System.Net.Sockets.SocketException>(() =>
            {
                var client = new Cube.Net.Ntp.Client("404.not.found");
            });
        }
    }
}
