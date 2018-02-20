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
using System.Threading.Tasks;
using Cube.Net.App.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFacadeTest
    ///
    /// <summary>
    /// RssFacade のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssFacadeTest : FileHelper
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Setup_Empty
        ///
        /// <summary>
        /// Feeds.json が見つからない場合の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Setup_Empty()
        {
            IO.Delete(Result("Feeds.json"));

            using (var m = Create())
            {
                m.Stop();
                Assert.That(m.Data.Root.Count(),       Is.EqualTo(0));
                Assert.That(m.Data.Current.HasValue,   Is.False, nameof(m.Data.Current));
                Assert.That(m.Data.LastEntry.HasValue, Is.False, nameof(m.Data.LastEntry));
                Assert.That(m.Data.Content.HasValue,   Is.False, nameof(m.Data.Content));
                Assert.That(m.Data.Message.Value,      Is.Empty, nameof(m.Data.Message));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// RSS エントリを移動するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Move()
        {
            using (var m = Create())
            {
                m.Stop();

                var src = m.Data.Current.Value;
                var dest = m.Data.Root.OfType<RssCategory>().First();

                Assert.That(src.Parent, Is.Null);
                Assert.That(dest.Entries.Count(), Is.EqualTo(1));

                m.Move(src, dest, 0);

                Assert.That(src.Parent, Is.EqualTo(dest));
                Assert.That(dest.Entries.Count(), Is.EqualTo(2));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CheckUpdate
        ///
        /// <summary>
        /// アップデート確認の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void CheckUpdate()
        {
            using (var m = Create())
            {
                m.Stop();

                Assert.That(m.Data.User.Value.CheckUpdate, Is.True);
                Task.Delay(150).Wait();
                m.Data.User.Value.CheckUpdate = false;
                Assert.That(m.Data.User.Value.CheckUpdate, Is.False);
                Assert.That(m.Data.User.Value.LastCheckUpdate.HasValue, Is.True);
                m.Data.User.Value.CheckUpdate = true;
                Assert.That(m.Data.User.Value.CheckUpdate, Is.True);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Import_Error
        ///
        /// <summary>
        /// Import に失敗した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Import_Error()
        {
            using (var m = Create())
            {
                m.Stop();
                m.Import("NotFound.opml");
                Assert.That(m.Data.Root.Count(), Is.AtLeast(1));
            }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Setup
        ///
        /// <summary>
        /// テスト直前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [SetUp]
        public void Setup()
        {
            IO.Copy(Example("Sample.json"), Result("Feeds.json"), true);
            IO.Delete(Result("Settings.json"));

            var cache = "Cache";
            foreach (var file in IO.GetFiles(Example(cache)))
            {
                var info = IO.Get(file);
                IO.Copy(file, Result($@"{cache}\{info.Name}"), true);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// ViewModel オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFacade Create()
        {
            var settings = new SettingsFolder(Results, IO);
            var dest = new RssFacade(settings);

            settings.Value.InitialDelay = TimeSpan.FromMinutes(1);

            dest.Setup();
            return dest;
        }

        #endregion
    }
}
