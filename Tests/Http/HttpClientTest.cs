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
using Cube.Net.Http;

namespace Cube.Tests.Net.Http
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
    public class HttpClientTest
    {
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
    }
}
