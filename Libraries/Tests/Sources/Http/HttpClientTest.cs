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
using Cube.Net.Http;
using NUnit.Framework;
using System;
using System.IO;

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
    [TestFixture]
    public class HttpClientTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// GetJsonAsync
        ///
        /// <summary>
        /// JSON データを非同期で取得するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetJsonAsync()
        {
            var uri    = new Uri("http://dev.cielquis.net/tests/example.json");
            var client = HttpClientFactory.Create();
            var json   = client.GetJsonAsync<PeopleContainer>(uri).Result;

            Assert.That(json.People,         Is.Not.Null);
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
        /// XML データを非同期で取得するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetXmlAsync()
        {
            var uri    = new Uri("http://dev.cielquis.net/tests/example.xml");
            var client = HttpClientFactory.Create();
            var xml    = client.GetXmlAsync<PeopleContainer>(uri).Result;

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
        public void GetAsync_NullHandler()
        {
            var uri  = new Uri("http://www.example.com/");
            var time = TimeSpan.FromSeconds(5);

            using (var http = HttpClientFactory.Create(null, time))
            using (var response = http.GetAsync(uri).Result)
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
        public void GetAsync_NotFound()
        {
            var uri = new Uri("https://www.cube-soft.jp/404.html");
            using (var http = HttpClientFactory.Create())
            {
                var result = http.GetAsync(uri, s => s.ReadByte()).Result;
                Assert.That(result, Is.EqualTo(0L));
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
        public void GetAsync_ConverterThrows()
        {
            var uri = new Uri("http://www.example.com/");
            using (var http = HttpClientFactory.Create())
            {
                Assert.That(
                    () => http.GetAsync(uri, Throws).Result,
                    NUnit.Framework.Throws.TypeOf<AggregateException>()
                          .And
                          .InnerException
                          .InstanceOf<ArgumentException>()
                );
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_IgnoreException
        ///
        /// <summary>
        /// 変換用オブジェクトが例外を無視する時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_IgnoreException()
        {
            var uri = new Uri("http://www.example.com/");
            using (var http = HttpClientFactory.Create())
            {
                var result = http.GetAsync(
                    uri,
                    new ContentConverter<long>(Throws) { IgnoreException = true }
                ).Result;
                Assert.That(result, Is.EqualTo(0L));
            }
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Throws
        ///
        /// <summary>
        /// 例外を発生させる関数オブジェクトです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private readonly Func<Stream, long> Throws = (_) =>
        {
            throw new ArgumentException("ErrorTest");
        };

        #endregion
    }
}
