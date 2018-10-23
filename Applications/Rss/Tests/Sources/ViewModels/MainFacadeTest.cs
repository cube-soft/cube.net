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
using Cube.Net.Rss.App.Reader;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainFacadeTest
    ///
    /// <summary>
    /// MainFacade のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class MainFacadeTest : ResourceFixture
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
            var ctx = SynchronizationContext.Current;
            var asm = Assembly.GetExecutingAssembly();
            using (var m = new MainFacade(new SettingsFolder(asm, Results, IO), ctx))
            {
                m.Setup();
                m.Stop();

                Assert.That(m.Data.Root.Count(),    Is.EqualTo(0));
                Assert.That(m.Data.Current.Value,   Is.Null, nameof(m.Data.Current));
                Assert.That(m.Data.LastEntry.Value, Is.Null, nameof(m.Data.LastEntry));
                Assert.That(m.Data.Content.Value,   Is.Null, nameof(m.Data.Content));
                Assert.That(m.Data.Message.Value,   Is.Empty, nameof(m.Data.Message));
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
        /// Move_SameCategory
        ///
        /// <summary>
        /// 同カテゴリで RSS エントリを移動するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Move_SameCategory()
        {
            using (var m = Create())
            {
                m.Stop();

                var src = m.Data.Root.First(e => e.Title == "Microsoft") as RssCategory;
                Assert.That(src.Children.Count,    Is.EqualTo(5));
                Assert.That(src.Children[0].Title, Is.EqualTo("Windows"), "Before");
                Assert.That(src.Children[1].Title, Is.EqualTo("Development"), "Before");
                Assert.That(src.Children[2].Title, Is.EqualTo("Official Microsoft Blog"), "Before");
                Assert.That(src.Children[3].Title, Is.EqualTo("Microsoft on the Issues"), "Before");
                Assert.That(src.Children[4].Title, Is.EqualTo("Internet of Things"), "Before");

                m.Move(src.Children[2], src.Children[4], 4);

                var dest = src.Entries.ToList();
                Assert.That(dest[0].Title, Is.EqualTo("Microsoft on the Issues"), "After");
                Assert.That(dest[1].Title, Is.EqualTo("Official Microsoft Blog"), "After");
                Assert.That(dest[2].Title, Is.EqualTo("Internet of Things"), "After");
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

                Assert.That(m.Data.Shared.Value.CheckUpdate, Is.True);
                Task.Delay(150).Wait();
                m.Data.Shared.Value.CheckUpdate = false;
                Assert.That(m.Data.Shared.Value.CheckUpdate, Is.False);
                Assert.That(m.Data.Shared.Value.LastCheckUpdate.HasValue, Is.True);
                m.Data.Shared.Value.CheckUpdate = true;
                Assert.That(m.Data.Shared.Value.CheckUpdate, Is.True);
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

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// RssFacade オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private MainFacade Create([CallerMemberName] string name = null)
        {
            Copy(name);

            var asm      = Assembly.GetExecutingAssembly();
            var settings = new SettingsFolder(asm, RootDirectory(name), IO);
            var context  = SynchronizationContext.Current;
            var dest     = new MainFacade(settings, context);

            settings.Shared.InitialDelay = TimeSpan.FromMinutes(1);

            dest.Setup();
            return dest;
        }

        #endregion
    }
}
