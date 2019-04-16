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
using Cube.FileSystem.TestService;
using Cube.Net.Rss;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFeedTest
    ///
    /// <summary>
    /// RssFeed に関するテスト用クラスです。
    /// </summary>
    ///
    /// <remarks>
    /// Parse に使用するサンプルファイルの一部は下記 Web ページ、および
    /// リンク先から取得しました。
    /// https://validator.w3.org/feed/docs/rss1.html
    /// https://validator.w3.org/feed/docs/rss2.html
    /// https://validator.w3.org/feed/docs/atom.html
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssFeedTest : FileFixture
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssUris
        ///
        /// <summary>
        /// RSS フィードの取得用 URL を解析するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("Sample.html",      ExpectedResult = 2)]
        [TestCase("SampleNoRss.html", ExpectedResult = 0)]
        public int GetRssUris(string src)
        {
            using (var stream = IO.OpenRead(GetExamplesWith(src)))
            {
                return stream.GetRssUris().Count();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ParseFeed
        ///
        /// <summary>
        /// フィードを解析するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCaseSource(nameof(TestCases))]
        public int ParseFeed(string src, RssFeed expected)
        {
            using (var stream = File.OpenRead(GetExamplesWith(src)))
            {
                var actual = RssParser.Parse(stream);

                Assert.That(actual.Title, Is.EqualTo(expected.Title), "Title");
                Assert.That(actual.Description, Is.EqualTo(expected.Description), "Description");
                Assert.That(actual.Link, Is.EqualTo(expected.Link), "Link");

                return actual.Items.Count();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ParseFeed_Content
        ///
        /// <summary>
        /// RSS フィードの最初の Item から Content を取得するテストを
        /// 実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("SampleRss20-02.xml", ExpectedResult = 873)]
        [TestCase("SampleAtom-01.xml",  ExpectedResult =  24)]
        public int ParseFeed_Content(string filename)
        {
            using (var stream = File.OpenRead(GetExamplesWith(filename)))
            {
                return RssParser.Parse(stream).Items.First().Content.Length;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ParseFeed_UnreadItems
        ///
        /// <summary>
        /// UnreadItems の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ParseFeed_UnreadItems()
        {
            using (var stream = File.OpenRead(GetExamplesWith("SampleRss20-01.xml")))
            {
                var feed  = RssParser.Parse(stream);
                var count = feed.Items.Count();

                Assert.That(feed.UnreadItems.Count(), Is.EqualTo(count));
                feed.Items[0].Status = RssItemStatus.Read;
                Assert.That(feed.UnreadItems.Count(), Is.EqualTo(count - 1));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ParseFeed_Categories
        ///
        /// <summary>
        /// Categories の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ParseFeed_Categories()
        {
            using (var stream = File.OpenRead(GetExamplesWith("SampleRss20-01.xml")))
            {
                var feed = RssParser.Parse(stream);
                var item = feed.Items[0];
                Assert.That(item.Categories.Count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ParseFeed_NotRss
        ///
        /// <summary>
        /// RSS フィードではない XML ファイルを指定した時の挙動を
        /// 確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ParseFeed_NotRss()
        {
            using (var stream = File.OpenRead(GetExamplesWith("Sample.xml")))
            {
                Assert.That(RssParser.Parse(stream), Is.Null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Strip_Summary
        ///
        /// <summary>
        /// Summary の解析結果を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Strip_Summary()
        {
            using (var stream = File.OpenRead(GetExamplesWith("SampleRss20-03.xml")))
            {
                var dest = RssParser.Parse(stream);
                Assert.That(dest.Items[0].Summary, Is.EqualTo("この画像はテスト画像です。"));
            }
        }

        #endregion

        #region TestCases

        /* ----------------------------------------------------------------- */
        ///
        /// TestCases
        ///
        /// <summary>
        /// テスト用データを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData("SampleRss091.xml", new RssFeed
                {
                    Title       = "WriteTheWeb",
                    Description = "News for web users that write back",
                    Link        = new Uri("http://writetheweb.com"),
                }).Returns(6);

                yield return new TestCaseData("SampleRss092.xml", new RssFeed
                {
                    Title       = "Dave Winer: Grateful Dead",
                    Description = "A high-fidelity Grateful Dead song every day. This is where we're experimenting with enclosures on RSS news items that download when you're not using your computer. If it works (it will) it will be the end of the Click-And-Wait multimedia experience on the Internet.",
                    Link        = new Uri("http://www.scripting.com/blog/categories/gratefulDead.html"),
                }).Returns(22);

                yield return new TestCaseData("SampleRss10-01.xml", new RssFeed
                {
                    Title       = "XML.com",
                    Description = "XML.com features a rich mix of information and services for the XML community.",
                    Link        = new Uri("http://xml.com/pub"),
                }).Returns(2);

                yield return new TestCaseData("SampleRss20-01.xml", new RssFeed
                {
                    Title       = "Scripting News",
                    Description = "A weblog about scripting and stuff like that.",
                    Link        = new Uri("http://www.scripting.com/"),
                }).Returns(9);

                yield return new TestCaseData("SampleRss20-02.xml", new RssFeed
                {
                    Title       = "SoGap",
                    Description = "SoGap is a service for searching for popular news by using social signals.",
                    Link        = new Uri("http://sogap.cielquis.net/"),
                }).Returns(3);

                yield return new TestCaseData("SampleAtom-01.xml", new RssFeed
                {
                    Title       = "Example Feed",
                    Description = "",
                    Link        = new Uri("http://example.org/"),
                }).Returns(1);

                yield return new TestCaseData("SampleAtom-02.xml", new RssFeed
                {
                    Title       = "Sample for Atom 0.3",
                    Description = "",
                    Link        = new Uri("http://www.example.com/"),
                }).Returns(3);
            }
        }

        #endregion
    }
}
