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
using System.Threading;
using System.Threading.Tasks;
using Cube.Net.Rss;
using Cube.Net.App.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests.ViewModels
{
    /* --------------------------------------------------------------------- */
    ///
    /// AddViewModelTest
    ///
    /// <summary>
    /// AddViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class AddViewModelTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// 新しい RSS フィードを追加するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task Add()
        {
            var result = default(RssFeed);
            var vm = new AddViewModel(e => result = e);

            Assert.That(vm.Add.CanExecute(null), Is.False);
            vm.Url.Value = "https://github.com/blog.atom";
            Assert.That(vm.Add.CanExecute(null), Is.True);

            try
            {
                var cts = new CancellationTokenSource();
                vm.Messenger.Register<AddViewModel>(this, _ => cts.Cancel());
                vm.Add.Execute(null);
                await Task.Delay(5000, cts.Token);
            }
            catch (TaskCanceledException /* err */) { /* OK */ }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items.Count(), Is.GreaterThan(1));
        }
    }
}
