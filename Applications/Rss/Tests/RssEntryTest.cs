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
            var src = new RssSubscribeCollection();
            src.Load(Example("Feeds.json"));

            Assert.That(src.Count(), Is.EqualTo(3));

            var item = src.First(e => string.IsNullOrEmpty(e.Title));
            Assert.That(item.Categories, Is.Null);
            Assert.That(item.Items,      Is.Not.Null);

            Assert.That(item.Entries.Count(),        Is.EqualTo(1));
            Assert.That(item.Entries.First().Title,  Is.EqualTo("The GitHub Blog"));
            Assert.That(item.Entries.First().Uri,    Is.EqualTo(new Uri("https://github.com/blog.atom")));
            Assert.That(item.Entries.First().Parent, Is.EqualTo(item));
        }

        #region Helpers

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        /// 
        /// <summary>
        /// RssSubscribeCollection オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssSubscribeCollection Create()
        {
            var dest = new RssSubscribeCollection();
            dest.Load(Example("Feeds.json"));
            return dest;
        }

        #endregion
    }
}
