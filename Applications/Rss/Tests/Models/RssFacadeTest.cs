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
            settings.Value.InitialDelay = TimeSpan.FromMinutes(1);

            var dest = new RssFacade(settings);
            dest.Setup();
            return dest;
        }

        #endregion
    }
}
