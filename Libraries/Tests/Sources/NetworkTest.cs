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
using System.Net.NetworkInformation;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkTest
    ///
    /// <summary>
    /// Network のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NetworkTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Status
        ///
        /// <summary>
        /// ネットワーク状況を取得するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Status() => Assert.That(Network.Status, Is.EqualTo(OperationalStatus.Up));

        /* ----------------------------------------------------------------- */
        ///
        /// Available
        ///
        /// <summary>
        /// ネットワークが利用可能かどうか判別するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Available() => Assert.That(Network.Available);

        /* ----------------------------------------------------------------- */
        ///
        /// DisableOptions
        ///
        /// <summary>
        /// 各種ネットワークオプションを無効にするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void DisableOptions()
        {
            Network.DisableOptions();

            Assert.That(System.Net.ServicePointManager.Expect100Continue, Is.False);
            Assert.That(System.Net.ServicePointManager.UseNagleAlgorithm, Is.False);
            Assert.That(System.Net.WebRequest.DefaultWebProxy, Is.Null);
        }
    }
}
