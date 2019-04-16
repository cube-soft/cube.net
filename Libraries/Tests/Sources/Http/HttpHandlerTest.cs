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
using NUnit.Framework;
using System;
using System.Net;
using System.Reflection;

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
    [TestFixture]
    class HttpHandlerTest
    {
        #region Tests

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
        [Test]
        public void GetAsync_ConnectionClose()
        {
            var uri = new Uri("http://www.cube-soft.jp/");
            var h   = new Cube.Net.Http.HeaderHandler
            {
                UserAgent       = GetUserAgent(),
                ConnectionClose = true
            };

            using (var http = Cube.Net.Http.HttpClientFactory.Create(h))
            using (var response = http.GetAsync(uri).Result)
            {
                Assert.That(response.Headers.Connection.Contains("Close"));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_EntityTag()
        {
            var uri = new Uri("https://www.cube-soft.jp/favicon.ico");
            var h   = new Cube.Net.Http.HeaderHandler { UserAgent = GetUserAgent() };

            using (var http = Cube.Net.Http.HttpClientFactory.Create(h))
            {
                using (var r = http.GetAsync(uri).Result) Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var tag = h.EntityTag;
                using (var r = http.GetAsync(uri).Result)
                {
                    Assert.That(h.EntityTag,  Is.EqualTo(tag));
                    Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
                }
            }
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// GetUserAgent
        ///
        /// <summary>
        /// User-Agent を表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetUserAgent()
        {
            var asm = new AssemblyReader(Assembly.GetExecutingAssembly());
            var app = $"{asm.Product}/{asm.Version}";
            var win = Environment.OSVersion.VersionString;
            var net = $".NET {Environment.Version}";
            return $"{app} ({win}; {net})";
        }

        #endregion
    }
}
