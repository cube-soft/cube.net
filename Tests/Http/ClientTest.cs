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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NUnit.Framework;
using Cube.Net.Http;


namespace Cube.Tests.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientTest
    ///
    /// <summary>
    /// System.Net.Http.HttpClient を用いたテストを行うためのクラスです。
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
    public class ClientTest
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// タイムアウト時間を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout => TimeSpan.FromSeconds(2);

        #endregion

        #region Test methods

        #region ClientFactory

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_ConnectionClose
        ///
        /// <summary>
        /// ConnectionClose を設定するテストを行います。
        /// </summary>
        /// 
        /// <remarks>
        /// NuGet に公開されている System.Net.Http for .NET 3.5 には
        /// ConnectionClose に関するバグが存在する模様。
        /// 現在は、AddRequestHeaders で Connection プロパティに値を
        /// 設定しないように修正したものを利用している。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("Close")]
        public async Task GetAsync_Connection(string expected)
        {
            var uri = new Uri("http://news.cube-soft.jp/app/notice/all.json");
            var response = await ClientFactory.Create(Timeout).GetAsync(uri);

            Assert.That(
                response.Headers.Connection.Contains(expected),
                Is.True
            );
        }

        #endregion

        #region GetXxxAsync

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
            var json = await ClientFactory.Create(Timeout)
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
            var xml = await ClientFactory.Create(Timeout)
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

        #endregion

        #endregion

        #region Internal classes

        /* ----------------------------------------------------------------- */
        ///
        /// Person
        ///
        /// <summary>
        /// 解析後のデータを格納するためのクラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        public class Person
        {
            [DataMember(Name="id")]
            [XmlAttribute("id")]
            public string Id { get; set; }

            [DataMember(Name = "name")]
            [XmlElement("name")]
            public string Name { get; set; }

            [DataMember(Name = "age")]
            [XmlElement("age")]
            public int Age { get; set; }

            [DataMember(Name = "sex")]
            [XmlElement("sex")]
            public string Sex { get; set; }

            [DataMember(Name = "contact")]
            [XmlElement("contact")]
            public string Contact { get; set; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PeopleContainer
        ///
        /// <summary>
        /// 解析後のデータを格納するためのクラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        [XmlRoot("people")]
        public class PeopleContainer
        {
            [DataMember(Name = "people")]
            [XmlElement("person")]
            public List<Person> People { get; set; }
        }

        #endregion
    }
}
