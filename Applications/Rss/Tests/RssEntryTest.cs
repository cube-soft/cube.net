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
using System.Linq;
using Cube.Net.App.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientTest
    ///
    /// <summary>
    /// Cube.Net.Ntp.Client のテストクラスです。
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

            var category = src.First(e => e.Default);
            Assert.That(category.Title,                  Is.EqualTo("未分類"));
            Assert.That(category.Entries.Count(),        Is.EqualTo(1));
            Assert.That(category.Categories,             Is.Null);
            Assert.That(category.Items,                  Is.Not.Null);
            Assert.That(category.Entries.First().Parent, Is.EqualTo(category));
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
