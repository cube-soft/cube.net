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
            var src = new RssSubscriber();
            src.FileName = Example("Sample.json");
            src.CollectionChanged += (s, e) => count++;
            src.Load();

            Assert.That(src.Categories.Count(), Is.EqualTo(2));
            Assert.That(count, Is.EqualTo(4), nameof(src.CollectionChanged));

            var uri = new Uri("https://github.com/blog.atom");
            Assert.That(src.Entries.Count(), Is.EqualTo(2));
            Assert.That(src.Entries.First().Title, Is.EqualTo("The GitHub Blog"));
            Assert.That(src.Entries.First().Uri, Is.EqualTo(uri));
            Assert.That(src.Entries.First().Parent, Is.Null);
        }
    }
}
