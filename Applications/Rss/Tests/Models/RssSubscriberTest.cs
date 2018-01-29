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
using System.Runtime.CompilerServices;
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
    class RssSubscriberTest : FileHelper
    {
        #region Tests

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
        public void LoadJson()
        {
            using (var src = new RssSubscriber { FileName = CreateJson() })
            {
                var count = 0;
                src.CollectionChanged += (s, e) => count++;
                src.Load();

                Assert.That(src.Categories.Count(), Is.EqualTo(2));
                Assert.That(count, Is.GreaterThan(3), nameof(src.CollectionChanged));

                var uri = new Uri("https://github.com/blog.atom");
                Assert.That(src.Entries.Count(),        Is.EqualTo(2));
                Assert.That(src.Entries.First().Title,  Is.EqualTo("The GitHub Blog"));
                Assert.That(src.Entries.First().Uri,    Is.EqualTo(uri));
                Assert.That(src.Entries.First().Parent, Is.Null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        /// 
        /// <summary>
        /// OPML 形式でエクスポートするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Export()
        {
            using (var src = new RssSubscriber { FileName = CreateJson() })
            {
                var dest = Result("Export.opml");
                src.Load();
                src.Export(dest);

                Assert.That(IO.Exists(dest), Is.True);
            }
        }

        #endregion

        #region Helpers

        /* ----------------------------------------------------------------- */
        ///
        /// CreateJson
        /// 
        /// <summary>
        /// JSON ファイルをコピーします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string CreateJson([CallerMemberName] string filename = null)
        {
            var dest = Result(filename + ".json");
            IO.Copy(Example("Sample.json"), dest, true);
            return dest;
        }

        #endregion
    }
}
