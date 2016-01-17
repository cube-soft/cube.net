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
        /// EntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task EntityTag()
        {
            var uri = new Uri("http://news.cube-soft.jp/app/notice/all.json");
            var handler = new Cube.Net.Http.ClientHandler();
            handler.Proxy = null;
            handler.UseProxy = true;

            var client = new System.Net.Http.HttpClient(handler);
            var first = await client.GetAsync(uri);
            Assert.That(first.IsSuccessStatusCode, Is.True);

            var second = await client.GetAsync(uri);
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
        }

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
        public async Task ConnectionClose()
        {
            var uri = new Uri("http://www.google.co.jp/");
            var handler = new Cube.Net.Http.ClientHandler();
            handler.Proxy = null;
            handler.UseProxy = false;

            var client = new System.Net.Http.HttpClient(handler);
            client.DefaultRequestHeaders.ConnectionClose = true;
            var response = await client.GetAsync(uri);
            Assert.That(response.IsSuccessStatusCode, Is.True);
        }

        #endregion
    }
}
