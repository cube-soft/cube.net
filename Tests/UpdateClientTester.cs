/* ------------------------------------------------------------------------- */
///
/// UpdateClientTester.cs
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

namespace Cube.Tests.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.UpdateClientTester
    /// 
    /// <summary>
    /// UpdateClient クラスをテストするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class UpdateClientTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestGetMessageAsync
        /// 
        /// <summary>
        /// アップデート用メッセージを取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestGetMessageAsync()
        {
            Assert.DoesNotThrow(() =>
            {
                var uri = new Uri("http://www.cube-soft.jp/cubelab/cubenews/update.php");
                var client = new Cube.Net.UpdateClient();
                client.Version = "1.0.0";
                var m = client.GetMessageAsync(uri);
                Assert.That(m.Result.Version, Is.EqualTo(client.Version));
                Assert.That(m.Result.Notify, Is.False);
                Assert.That(m.Result.Uri, Is.EqualTo(new Uri("http://www.cube-soft.jp/cubelab/#news")));
                Assert.That(m.Result.Text.Length, Is.AtLeast(1));
            });
        }
    }
}
