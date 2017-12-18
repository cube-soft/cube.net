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
using System;
using Cube.Net.Rss;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssClientTest
    ///
    /// <summary>
    /// RssClient のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssClientTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// RSS フィードを取得するテストを実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync()
        {
            var http = new RssClient();
            var now  = DateTime.Now;
            var rss  = http.GetAsync(new Uri("https://blog.cube-soft.jp/?feed=rss2")).Result;

            Assert.That(rss, Is.Not.Null);
            Assert.That(rss.Title.Contains("CubeSoft"), Is.True);
            Assert.That(rss.Link, Is.EqualTo(new Uri("https://blog.cube-soft.jp/")));
            Assert.That(rss.Items.Count, Is.GreaterThan(0));
            Assert.That(rss.LastChecked, Is.GreaterThanOrEqualTo(now));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Continuously
        /// 
        /// <summary>
        /// 複数のフィードを連続して取得するテストを実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_Continuously()
        {
            var http = new RssClient();
            var uris = new[]
            {
                new Uri("https://blogs.msdn.microsoft.com/dotnet/feed"),
                new Uri("https://blogs.msdn.microsoft.com/visualstudio/feed/"),
                new Uri("http://blogs.windows.com/buildingapps/feed"),
            };

            foreach (var uri in uris)
            {
                var rss = http.GetAsync(uri).Result;
                Assert.That(rss.Items.Count, Is.GreaterThan(1));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Redirect
        /// 
        /// <summary>
        /// HTML ページを示す URL を指定した時の挙動を確認します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [TestCase("https://blog.cube-soft.jp/", ExpectedResult = "https://blog.cube-soft.jp/?feed=rss2")]
        [TestCase("https://www.asahi.com/",     ExpectedResult = "http://www3.asahi.com/rss/index.rdf")]
        public string GetAsync_Redirect(string src)
        {
            var uri = default(Uri);
            var http = new RssClient();
            http.Redirected += (s, e) => uri = e.NewValue;

            var rss = http.GetAsync(new Uri(src)).Result;
            Assert.That(rss, Is.Not.Null);
            return uri.ToString();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_NotFound
        /// 
        /// <summary>
        /// RSS フィードが見つからない時の挙動を確認します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_NotFound() => Assert.That(
            new RssClient().GetAsync(new Uri("http://www.cube-soft.jp/")).Result,
            Is.Null
        );
    }
}
