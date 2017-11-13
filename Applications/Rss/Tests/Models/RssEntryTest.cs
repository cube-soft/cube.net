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
using System.Linq;
using Cube.Net.App.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntryTest
    ///
    /// <summary>
    /// RssEntry およびそれに付随するクラスのテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssEntryTest : FileHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// JSON ファイルをロードするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load()
        {
            var count = 0;
            var src = new RssSubscribeCollection();
            src.CollectionChanged += (s, e) => count++;
            src.Load(Example("Feeds.json"));

            Assert.That(src.Categories.Count(), Is.EqualTo(3));
            Assert.That(count, Is.EqualTo(3), nameof(src.CollectionChanged));

            var item = src.Categories.First(e => string.IsNullOrEmpty(e.Title));
            Assert.That(item.Categories, Is.Null);
            Assert.That(item.Items, Is.Not.Null);

            var uri = new Uri("https://github.com/blog.atom");
            Assert.That(item.Entries.Count(), Is.EqualTo(1));
            Assert.That(item.Entries.First().Title, Is.EqualTo("The GitHub Blog"));
            Assert.That(item.Entries.First().Uri, Is.EqualTo(uri));
            Assert.That(item.Entries.First().Parent, Is.EqualTo(item));

            Assert.That(src.Lookup(uri), Is.Not.Null);
            src.Clear();
            Assert.That(src.Lookup(uri), Is.Null);

            Assert.That(src.Categories.Count(), Is.EqualTo(0));
            Assert.That(count, Is.EqualTo(4));
        }
    }
}
