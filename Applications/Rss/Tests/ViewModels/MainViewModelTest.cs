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
    class MainViewModelTest : FileHelper
    {
        #region Tests

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
        public async Task Select()
        {
            var vm = new MainViewModel(new SettingsFolder(Results, IO));

            var entry = vm.Data.Root.OfType<RssCategory>().First().Entries.First();
            Assert.That(entry.Uri, Is.EqualTo(new Uri("http://blog.cube-soft.jp/?feed=rss2")));
            vm.SelectEntry.Execute(entry);

            Assert.That(vm.Data.Feed.Value.Items.Count(), Is.EqualTo(0));
            for (var i = 0; i < 100; i++)
            {
                if (vm.Data.Feed.Value.Items.Count() > 0) break;
                await Task.Delay(50);
            }
            Assert.That(vm.Data.Feed.Value.Items.Count(), Is.GreaterThan(1), "Timeout");

            vm.SelectItem.Execute(new Cube.Xui.Behaviors.SelectionList(
                new object[0],
                new[] { vm.Data.Feed.Value.Items.First() }
            ));

            Assert.That(vm.Data.Content.Value, Is.Not.Null.And.Not.Empty);
        }

        #endregion

        #region Helpers

        /* ----------------------------------------------------------------- */
        ///
        /// OneTimeSetup
        /// 
        /// <summary>
        /// 一度だけ初期化されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var json = "Feeds.json";
            var src  = Example(json);
            var dest = IO.Combine(Results, json);

            IO.Copy(src, dest, true);
        }

        #endregion
    }
}
