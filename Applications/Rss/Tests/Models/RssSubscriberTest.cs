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
using System.Threading.Tasks;
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
        public void Load()
        {
            using (var src = Create())
            {
                Assert.That(src.CacheDirectory, Is.Null);

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
        /// Load_NotFound
        ///
        /// <summary>
        /// 存在しない JSON ファイルをロードした時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load_NotFound()
        {
            using (var src = new RssSubscriber { FileName = "NotFound.json" })
            {
                src.Load();
                Assert.That(src.Count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Find
        ///
        /// <summary>
        /// Find メソッドの挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Find()
        {
            using (var src = Create())
            {
                src.Load();

                Assert.That(src.Find(new Uri("https://github.com/blog.atom")), Is.Not.Null);
                Assert.That(src.Find(new Uri("http://www.example.com/")), Is.Null);
                Assert.That(src.Find(default(Uri)), Is.Null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update_Frequency
        ///
        /// <summary>
        /// RssCheckFrequency の値に関わらず Update が成功する事を
        /// 確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(RssCheckFrequency.Auto)]
        [TestCase(RssCheckFrequency.High)]
        [TestCase(RssCheckFrequency.Low)]
        [TestCase(RssCheckFrequency.None)]
        public void Update_Frequency(RssCheckFrequency value)
        {
            var uri = new Uri("https://blog.cube-soft.jp/?feed=rss2");

            using (var src = Create())
            {
                src.Load();
                var dest = src.Find(uri);
                src.Received += (s, e) =>
                {
                    Assert.That(e.Value.Uri, Is.EqualTo(uri));
                    dest.Count = e.Value.UnreadItems.Count();
                };
                dest.Frequency = value;
                dest.Count = 0; // hack for tests.
                src.Reschedule(dest);

                Assert.That(dest.Count, Is.EqualTo(0));
                src.Update(dest);
                Assert.That(Wait(dest).Result, Is.True, "Timeout");
                Assert.That(dest.Count, Is.AtLeast(1));
            }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// CreateJson
        ///
        /// <summary>
        /// JSON ファイルをコピーします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssSubscriber Create([CallerMemberName] string filename = null)
        {
            var dest = Result(filename + ".json");
            IO.Copy(Example("Sample.json"), dest, true);
            return new RssSubscriber { FileName = dest };
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Wait
        ///
        /// <summary>
        /// 新着記事を受信するまで待機します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<bool> Wait(RssEntry src)
        {
            for (var i = 0; i < 100; ++i)
            {
                if (src.Count > 0) return true;
                await Task.Delay(50);
            }
            return false;
        }


        #endregion
    }
}
