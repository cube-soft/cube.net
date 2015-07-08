/* ------------------------------------------------------------------------- */
///
/// ClientHandleTester.cs
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Tests.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.Http.ClientHandleTester
    ///
    /// <summary>
    /// Http.ClientHandler のテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ClientHandleTester
    {
        #region Test methods

        /* ----------------------------------------------------------------- */
        ///
        /// TestEntityTag
        ///
        /// <summary>
        /// EntityTag (ETag) のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestEntityTag()
        {
            var uri = new Uri("http://news.cube-soft.jp/app/notice/all.json");
            var handler = new Cube.Net.Http.ClientHandler();
            handler.Proxy = null;
            handler.UseProxy = true;

            var client = new System.Net.Http.HttpClient(handler);
            var first = client.GetAsync(uri).Result;
            Assert.That(first.IsSuccessStatusCode, Is.True);

            var second = client.GetAsync(uri).Result;
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestConnectionClose
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
        public void TestConnectionClose()
        {
            Assert.DoesNotThrow(() =>
            {
                var uri = new Uri("http://www.google.co.jp/");
                var handler = new Cube.Net.Http.ClientHandler();
                handler.Proxy = null;
                handler.UseProxy = false;

                var client = new System.Net.Http.HttpClient(handler);
                client.DefaultRequestHeaders.ConnectionClose = true;
                var response = client.GetAsync(uri).Result;
                Assert.That(response.IsSuccessStatusCode, Is.True);
            });
        }

        [Test]
        public void TestDummy()
        {
            Assert.DoesNotThrow(() =>
            {
                System.Diagnostics.Debug.WriteLine("first");
                var enc = System.Text.Encoding.GetEncoding("UTF-8");
                string url = "http://www.google.co.jp/";

                System.Diagnostics.Debug.WriteLine("start");
                WebRequest req = WebRequest.Create(url);
                req.Proxy = null;
                WebResponse res = req.GetResponse();
                System.Diagnostics.Debug.WriteLine("end");

                //var st = res.GetResponseStream();
                //var sr = new System.IO.StreamReader(st, enc);
                //string html = sr.ReadToEnd();
                //sr.Close();
                //st.Close();
            });
        }

        #endregion
    }
}
