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
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpHandlerTest
    ///
    /// <summary>
    /// 各種 HTTP ハンドラのテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class HttpHandlerTest : NetworkResource
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ConnectionClose
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
        [Test]
        public async void ConnectionClose()
        {
            var uri     = new Uri("http://www.example.com/");
            var handler = new Cube.Net.Http.HeaderHandler { ConnectionClose = true };
            var http    = Cube.Net.Http.ClientFactory.Create(handler);

            var response = await http.GetAsync(uri);
            Assert.That(response.Headers.Connection.Contains("Close"));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async void EntityTag()
        {
            var uri     = new Uri("http://www.example.com/");
            var handler = new Cube.Net.Http.HeaderHandler();
            var http    = Cube.Net.Http.ClientFactory.Create(handler);

            var _ = await http.GetAsync(uri);
            Assert.That(handler.EntityTag, Is.Not.Null.Or.Empty);

            var etag = handler.EntityTag;
            for (var i = 0; i < 5; ++i)
            {
                var response = await http.GetAsync(uri);
                if (handler.EntityTag == etag)
                {
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
                    return;
                }
            }

            Assert.Fail();
        }
    }
}
