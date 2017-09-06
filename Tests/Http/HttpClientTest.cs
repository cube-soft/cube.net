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
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Cube.Net.Http;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpClientTest
    ///
    /// <summary>
    /// HttpClient に関連するテストを行うためのクラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// internal class の場合 GetXmlAsync() のテストに失敗するため、
    /// public class に設定しています。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    public class HttpClientTest : NetworkHandler
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// GetJsonAsync
        /// 
        /// <summary>
        /// JSON データを非同期で取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetJsonAsync()
        {
            var uri = new Uri("http://dev.cielquis.net/tests/example.json");
            var json = await ClientFactory.Create()
                                          .GetJsonAsync<PeopleContainer>(uri);

            Assert.That(json.People, Is.Not.Null);
            Assert.That(json.People.Count,   Is.EqualTo(2));
            Assert.That(json.People[0].Id,   Is.EqualTo("0001"));
            Assert.That(json.People[0].Name, Is.EqualTo("Jack Daniel"));
            Assert.That(json.People[0].Age,  Is.EqualTo(40));
            Assert.That(json.People[1].Id,   Is.EqualTo("0002"));
            Assert.That(json.People[1].Name, Is.EqualTo("山田 花子"));
            Assert.That(json.People[1].Age,  Is.EqualTo(25));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetXmlAsync
        /// 
        /// <summary>
        /// XML データを非同期で取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetXmlAsync()
        {
            var uri = new Uri("http://dev.cielquis.net/tests/example.xml");
            var xml = await ClientFactory.Create()
                                         .GetXmlAsync<PeopleContainer>(uri);

            Assert.That(xml.People, Is.Not.Null);
            Assert.That(xml.People.Count,   Is.EqualTo(2));
            Assert.That(xml.People[0].Id,   Is.EqualTo("0001"));
            Assert.That(xml.People[0].Name, Is.EqualTo("Jack Daniel"));
            Assert.That(xml.People[0].Age,  Is.EqualTo(40));
            Assert.That(xml.People[1].Id,   Is.EqualTo("0002"));
            Assert.That(xml.People[1].Name, Is.EqualTo("山田 花子"));
            Assert.That(xml.People[1].Age,  Is.EqualTo(25));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// Handler に null を指定した場合のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetAsync_NullHandler()
        {
            var uri  = new Uri("http://www.example.com/");
            var time = TimeSpan.FromSeconds(5);

            using (var http = ClientFactory.Create(null, time))
            using (var response = await http.GetAsync(uri))
            {
                Assert.That(response.IsSuccessStatusCode);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_NotFound
        /// 
        /// <summary>
        /// 存在しない URL を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetAsync_NotFound()
        {
            var uri = new Uri("http://www.cube-soft.jp/404.html");
            using (var http = ClientFactory.Create())
            {
                var result = await http.GetAsync(uri, RawContent);
                Assert.That(result, Is.Null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_ConverterThrows
        /// 
        /// <summary>
        /// 変換用オブジェクトが例外を送出した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task GetAsync_ConverterThrows()
        {
            var uri = new Uri("http://www.example.com/");
            using (var http = ClientFactory.Create())
            {
                var result = await http.GetAsync(uri, Throws);
                Assert.That(result, Is.Null);
            }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// RawContent
        /// 
        /// <summary>
        /// 文字列を取得する関数オブジェクトです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private Func<HttpContent, Task<string>> RawContent = (s)
            => s.ReadAsStringAsync();

        /* ----------------------------------------------------------------- */
        ///
        /// Throws
        /// 
        /// <summary>
        /// 例外を発生させる関数オブジェクトです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private Func<HttpContent, Task<string>> Throws = (s) =>
        {
            throw new ArgumentException("ErrorTest");
        };

        #endregion
    }
}
