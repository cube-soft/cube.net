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
    /// MainViewModelTest
    ///
    /// <summary>
    /// MainViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class MainViewModelTest : ViewModelHelper
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultProperties
        ///
        /// <summary>
        /// MainViewModel のプロパティの初期値を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void DefaultProperties()
        {
            var vm = new MainViewModel();
            Assert.That(vm.Data.Article.HasValue, Is.False);
            Assert.That(vm.Data.Current.HasValue,   Is.False);
            Assert.That(vm.Data.LastEntry.HasValue,    Is.False);
            Assert.That(vm.Data.Message.HasValue, Is.False);
            Assert.That(vm.Data.User.HasValue,    Is.True);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// RSS フィードから新着記事を選択するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Select()
        {
            var vm = Create();
            vm.SelectEntry.Execute(vm.Data.Root.OfType<RssCategory>().First().Entries.First());

            var feed = vm.Data.LastEntry.Value;
            Assert.That(feed.Items.Count(), Is.EqualTo(10));
            Assert.That(feed.UnreadItems.Count(), Is.EqualTo(9));

            vm.SelectArticle.Execute(feed.UnreadItems.First());
            Assert.That(vm.Data.Article.Value, Is.Not.Null);
            Assert.That(feed.UnreadItems.Count(), Is.EqualTo(8));
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
        private MainViewModel Create()
        {
            var dest = new MainViewModel(new SettingsFolder(Results, IO));
            dest.Setup.Execute(null);
            return dest;
        }

        #endregion
    }
}
