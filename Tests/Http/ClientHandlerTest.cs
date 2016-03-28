/* ------------------------------------------------------------------------- */
///
/// ClientHandlerTest.cs
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Tests.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientHandlerTest
    ///
    /// <summary>
    /// Http.ClientHandler のテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ClientHandlerTest
    {
        #region Test methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(HttpStatusCode.NotModified)]
        public async Task GetAsync_StatusCode(HttpStatusCode expected)
        {
            var http = Create();
            var uri = new Uri("http://news.cube-soft.jp/app/notice/all.json");

            var dummy  = await http.GetAsync(uri);
            var result = await http.GetAsync(uri);
            Assert.That(result.StatusCode, Is.EqualTo(expected));
        }

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
            var http = Create();
            http.DefaultRequestHeaders.ConnectionClose = true;
            var uri = new Uri("http://news.cube-soft.jp/app/notice/all.json");

            var result = await http.GetAsync(uri);
            Assert.That(result.Headers.Connection.Contains(expected), Is.True);
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Cube.Net.Http.ClientHandler を用いて初期化した HttpClient
        /// オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private HttpClient Create()
            => new HttpClient(new Cube.Net.Http.ClientHandler());

        #endregion
    }
}
