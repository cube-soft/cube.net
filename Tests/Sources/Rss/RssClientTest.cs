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
using Cube.Net.Http;
using Cube.Net.Rss;
using NUnit.Framework;
using System;
using System.Reflection;

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
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// RSS フィードを取得するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("https://blog.cube-soft.jp/?feed=rss2")]
        public void GetAsync(string src)
        {
            var http  = Create();
            var now   = DateTime.Now;
            var rss   = http.GetAsync(new Uri(src)).Result;

            Assert.That(rss, Is.Not.Null);
            Assert.That(rss.Title.Length, Is.AtLeast(1), nameof(rss.Title));
            Assert.That(rss.Uri, Is.Not.Null, nameof(rss.Uri));
            Assert.That(rss.Link, Is.Not.Null, nameof(rss.Link));
            Assert.That(rss.Items.Count, Is.AtLeast(1), nameof(rss.Items));
            Assert.That(rss.LastChecked, Is.GreaterThanOrEqualTo(now), nameof(rss.LastChecked));
            Assert.That(rss.LastPublished.HasValue, Is.True, nameof(rss.LastPublished));

            var item = rss.Items[0];

            Assert.That(item.Title.Length, Is.AtLeast(1), nameof(item.Title));
            Assert.That(item.Link, Is.Not.Null, nameof(item.Link));
            Assert.That(item.PublishTime.HasValue, Is.True, nameof(item.PublishTime));
            Assert.That(item.Status, Is.EqualTo(RssItemStatus.Unread));
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
        public string GetAsync_Redirect(string src)
        {
            var uri  = default(Uri);
            var http = Create();
            http.Redirected += (s, e) => uri = e.NewValue;

            var rss = http.GetAsync(new Uri(src)).Result;
            Assert.That(rss, Is.Not.Null);
            return uri.ToString();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Redirect_NoHandlers
        ///
        /// <summary>
        /// HTML ページを示す URL を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_Recirect_NoHandlers() => Assert.That(
            Create().GetAsync(new Uri("https://blog.cube-soft.jp/")).Result,
            Is.Not.Null
        );

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_NotFound
        ///
        /// <summary>
        /// RSS フィードが見つからない時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("http://www.cube-soft.jp/")]
        [TestCase("http://www.cube-soft.jp/404.html")]
        public void GetAsync_NotFound(string src) => Assert.That(
            Create().GetAsync(new Uri(src)).Result,
            Is.Null
        );

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// RssClient オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssClient Create()
        {
            var asm = Assembly.GetExecutingAssembly().GetReader();
            var h   = new HeaderHandler { UserAgent = $"{asm.Product}/{asm.Version}" };
            return new RssClient(h);
        }

        #endregion
    }
}
