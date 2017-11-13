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
            var vm = new MainViewModel();

            vm.SelectEntry.Execute(vm.Categories.First().Entries.First());

            Assert.That(vm.Feed.Value.Items.Count(), Is.EqualTo(0));
            Assert.That(await Wait(vm), Is.GreaterThan(1), "Timeout");

            vm.SelectArticle.Execute(new Cube.Xui.Behaviors.SelectionList(
                new object[0],
                new [] { vm.Feed.Value.Items.First() }
            ));

            Assert.That(vm.Content.Value, Is.Not.Null.And.Not.Empty);
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
            var filename = "Feeds.json";
            var src      = Example(filename);
            var dir      = IO.Get(AssemblyReader.Default.Location).DirectoryName;
            var dest     = IO.Combine(dir, filename);

            IO.Copy(src, dest, true);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Wait
        /// 
        /// <summary>
        /// 処理が終了するまで待機します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<int> Wait(MainViewModel vm)
        {
            for (var i = 0; i < 100; i++)
            {
                var count = vm.Feed.Value.Items.Count();
                if (count > 0) return count;
                await Task.Delay(50);
            }
            return vm.Feed.Value.Items.Count();
        }

        #endregion
    }
}
