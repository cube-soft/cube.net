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
using Cube.Net.Rss.App.Reader;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cube.Net.Rss.Tests
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
    class RssSubscriberTest : ResourceFixture
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
                Assert.That(src.Capacity,       Is.EqualTo(100));
                Assert.That(src.CacheDirectory, Is.Null, nameof(src.CacheDirectory));
                Assert.That(src.UserAgent,      Is.Null, nameof(src.UserAgent));

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
            using (var src = new RssSubscriber { FileName = GetResultsWith("NotFound.json") })
            {
                src.Load();
                Assert.That(src.Count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Backup_Delete
        ///
        /// <summary>
        /// 規定数を超えたバックアップファイルを削除するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Backup_Delete()
        {
            var dir  = GetResultsWith(nameof(Backup_Delete), "Backup");
            var open = IO.Combine(dir, "20010101.json");

            for (var d = new DateTime(2001, 1, 1); d.Month < 2; d = d.AddDays(1))
            {
                var backup = IO.Combine(dir, $"{d.ToString("yyyyMMdd")}.json");
                IO.Copy(GetExamplesWith("Sample.json"), backup, true);
            }

            using (var _ = IO.OpenRead(open))
            using (var src = Create())
            {
                src.Load();
                Task.Delay(200).Wait();
            }

            Assert.That(IO.GetFiles(dir).Length, Is.EqualTo(31));
            Assert.That(IO.Exists(open), Is.True); // delete failed
            Assert.That(IO.Exists(IO.Combine(dir, "20010102.json")), Is.False);
            Assert.That(IO.Exists(IO.Combine(dir, "20010103.json")), Is.True);
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

                var asm = Assembly.GetExecutingAssembly().GetReader();
                src.UserAgent = $"{asm.Product}/{asm.Version}";

                Assert.That(dest.Count, Is.EqualTo(0));
                src.Update(dest);
                Assert.That(Wait.For(() => dest.Count > 0), "Timeout");
                Assert.That(dest.Count, Is.AtLeast(1));
            }
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// RssSubscriber オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssSubscriber Create([CallerMemberName] string name = null)
        {
            Copy(name);
            return new RssSubscriber { FileName = FeedsPath(name) };
        }

        #endregion
    }
}
