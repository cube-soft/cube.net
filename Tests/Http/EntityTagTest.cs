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
using System.Net;
using System.Net.Http;
using NUnit.Framework;

namespace Cube.Tests.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// EntityTagTest
    ///
    /// <summary>
    /// EntityTag のテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class EntityTagTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(3, HttpStatusCode.NotModified)]
        public async Task GetAsync_StatusCode(int count, HttpStatusCode expected)
        {
            var uri  = new Uri("http://news.cube-soft.jp/app/notice/all.json");
            var handler = new Cube.Net.Http.ContentHandler<object>();
            var http = Cube.Net.Http.ClientFactory.Create(handler, TimeSpan.FromSeconds(1));

            var first = await http.GetAsync(uri);
            Assert.That(handler.EntityTag, Is.Not.Null.Or.Empty);
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var n = Math.Max(count - 2, 1);
            for (var i = 0; i < n; ++i) await http.GetAsync(uri);

            var last = await http.GetAsync(uri);
            Assert.That(handler.EntityTag, Is.Not.Null.Or.Empty);
            Assert.That(last.StatusCode, Is.EqualTo(expected));
        }
    }
}
