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

namespace Cube.Tests.Net.Ntp
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
    class ClientTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Properties
        /// 
        /// <summary>
        /// 各種プロパティのテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Properties

        [Test]
        public void HostName_IsNotNullOrEmpty()
        {
            Assert.That(
                Client.Host.HostName,
                Is.Not.Null.Or.Empty
            );
        }

        [TestCase(123)]
        public void Port(int expected)
        {
            Assert.That(
                Client.Port,
                Is.EqualTo(expected)
            );
        }

        [TestCase(1)]
        public void AddressList_IsAtLeast(int expected)
        {
            Assert.That(
                Client.Host.AddressList.Length,
                Is.AtLeast(expected)
            );
        }

        [TestCase(5)]
        public void Timeout(int expected)
        {
            Assert.That(
                Client.Timeout,
                Is.EqualTo(TimeSpan.FromSeconds(expected))
            );
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Within
        /// 
        /// <summary>
        /// 非同期で時刻を取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(60)]
        public async Task GetAsync_Within(int expected)
        {
            var result = await Client.GetAsync();
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(expected));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Timeout_Throws
        /// 
        /// <summary>
        /// タイムアウト処理のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_Timeout_Throws()
        {
            Assert.That(
                async () =>
                {
                    var client = new Cube.Net.Ntp.Client();
                    client.Timeout = TimeSpan.FromMilliseconds(1);
                    await client.GetAsync();
                },
                Throws.TypeOf<TimeoutException>()
            );
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_404_Throws
        /// 
        /// <summary>
        /// 存在しない NTP サーバを指定した時のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_404_Throws()
        {
            Assert.That(
                () => new Cube.Net.Ntp.Client("404.not.found"),
                Throws.TypeOf<System.Net.Sockets.SocketException>()
            );
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// OneTimeSetUp
        /// 
        /// <summary>
        /// Ntp.Client オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Client = new Cube.Net.Ntp.Client();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Client
        /// 
        /// <summary>
        /// Ntp.Client オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Cube.Net.Ntp.Client Client { get; set; }

        #endregion
    }
}
