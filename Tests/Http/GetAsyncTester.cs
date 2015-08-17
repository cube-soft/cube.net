/* ------------------------------------------------------------------------- */
///
/// GetExtensionsTester.cs
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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NUnit.Framework;
using Cube.Extensions.Net;


namespace Cube.Tests.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.Http.GetAsyncTester
    ///
    /// <summary>
    /// 様々なデータの非同期取得メソッドをテストするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class GetAsyncTester
    {
        #region Test methods

        /* ----------------------------------------------------------------- */
        ///
        /// TestGetJsonAsync
        /// 
        /// <summary>
        /// JSON データを取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestGetJsonAsync()
        {
            Assert.DoesNotThrow(() =>
            {
                var uri = new Uri("http://dev.cielquis.net/tests/example.json");
                var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var xml = client.GetJsonAsync<PeopleContainer>(uri).Result;

                Assert.That(xml.People, Is.Not.Null);
                Assert.That(xml.People.Count, Is.EqualTo(2));
                Assert.That(xml.People[0].Id, Is.EqualTo("0001"));
                Assert.That(xml.People[0].Name, Is.EqualTo("Jack Daniel"));
                Assert.That(xml.People[0].Age, Is.EqualTo(40));
                Assert.That(xml.People[1].Id, Is.EqualTo("0002"));
                Assert.That(xml.People[1].Name, Is.EqualTo("山田 花子"));
                Assert.That(xml.People[1].Age, Is.EqualTo(25));
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestGetXml
        /// 
        /// <summary>
        /// XML データを取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestGetXmlAsync()
        {
            Assert.DoesNotThrow(() =>
            {
                var uri = new Uri("http://dev.cielquis.net/tests/example.xml");
                var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var xml = client.GetXmlAsync<PeopleContainer>(uri).Result;

                Assert.That(xml.People, Is.Not.Null);
                Assert.That(xml.People.Count, Is.EqualTo(2));
                Assert.That(xml.People[0].Id, Is.EqualTo("0001"));
                Assert.That(xml.People[0].Name, Is.EqualTo("Jack Daniel"));
                Assert.That(xml.People[0].Age, Is.EqualTo(40));
                Assert.That(xml.People[1].Id, Is.EqualTo("0002"));
                Assert.That(xml.People[1].Name, Is.EqualTo("山田 花子"));
                Assert.That(xml.People[1].Age, Is.EqualTo(25));
            });
        }

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
                Assert.That(message.Uri, Is.EqualTo(new Uri("http://s.cube-soft.jp/widget/")));
                Assert.That(message.Text.Length, Is.AtLeast(1));
            });
        }

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
