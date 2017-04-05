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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Cube.Tests.Net
{
    /* ----------------------------------------------------------------- */
    ///
    /// Person
    ///
    /// <summary>
    /// テストに使用するクラスです。
    /// </summary>
    ///
    /* ----------------------------------------------------------------- */
    [DataContract]
    public class Person
    {
        [DataMember(Name = "id")]
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
    /// テストに使用するクラスです。
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
}
