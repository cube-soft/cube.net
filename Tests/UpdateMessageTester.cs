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
using System.Collections.Generic;
using NUnit.Framework;
using Cube.Extensions.Net;

namespace Cube.Tests.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.UpdateMessageTester
    /// 
    /// <summary>
    /// UpdateMessage オブジェクトを取得するテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class UpdateMessageTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestGetUpdateMessageAsync
        /// 
        /// <summary>
        /// アップデート用メッセージを取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("1.0.2", "", "")]
        [TestCase("1.0.0", "flag", "install")]
        public void TestGetUpdateMessageAsync(string version, string key, string value)
        {
            Assert.DoesNotThrow(() =>
            {
                var query = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(key)) query.Add(key, value);
                var uri = new Uri("http://www.cube-soft.jp/cubelab/cubenews/update.php");
                var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var message = client.GetUpdateMessageAsync(uri, version, query).Result;
                Assert.That(message, Is.Not.Null);
                Assert.That(message.Version, Is.EqualTo(version));
                Assert.That(message.Notify, Is.False);
                Assert.That(message.Uri, Is.EqualTo(new Uri("http://www.cube-soft.jp/cubelab/#news")));
                Assert.That(message.Text.Length, Is.AtLeast(1));
            });
        }
    }
}
