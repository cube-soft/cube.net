/* ------------------------------------------------------------------------- */
///
/// PacketTester.cs
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
using NUnit.Framework;

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientTester
    ///
    /// <summary>
    /// Client クラスのテストをするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ClientTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestGetAsync
        /// 
        /// <summary>
        /// NTP サーバとの通信のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestGetAsync()
        {
            Assert.DoesNotThrow(() =>
            {
                var client = new Cube.Net.Ntp.Client("ntp.cube-soft.jp");
                Assert.That(client.Host.HostName, Is.Not.Null.Or.Empty);
                Assert.That(client.Host.AddressList.Length, Is.AtLeast(1));
                Assert.That(client.Port, Is.EqualTo(123));
                Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));

                var task = client.GetAsync();
                Assert.That(task.Result.IsValid, Is.True);
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestTimeout
        /// 
        /// <summary>
        /// タイムアウト処理のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestTimeout()
        {
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var client = new Cube.Net.Ntp.Client("ntp.cube-soft.jp");
                client.Timeout = TimeSpan.FromMilliseconds(1);
                client.GetAsync().Wait();
            });
            Assert.That(ex.InnerException, Is.TypeOf<TimeoutException>());
        }
    }
}
