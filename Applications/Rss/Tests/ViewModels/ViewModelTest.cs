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
    /// ViewModelTest
    ///
    /// <summary>
    /// MainViewModel および関連する ViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class ViewModelTest : ViewModelHelper
    {
        #region Tests

        #region MainViewModel

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Default
        ///
        /// <summary>
        /// MainViewModel のプロパティの初期値を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Default()
        {
            var vm = new MainViewModel();

            Assert.That(vm.Data.Article.HasValue,   Is.False);
            Assert.That(vm.Data.Current.HasValue,   Is.False);
            Assert.That(vm.Data.LastEntry.HasValue, Is.False);
            Assert.That(vm.Data.Message.HasValue,   Is.False);
            Assert.That(vm.Data.User.HasValue,      Is.True);
            Assert.That(vm.DropTarget,              Is.Not.Null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Select
        ///
        /// <summary>
        /// RSS フィードから新着記事を選択するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Select()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);

                var first = vm.Data.LastEntry.Value;
                Assert.That(first.Title,               Is.EqualTo("The GitHub Blog"));
                Assert.That(first.Items.Count(),       Is.EqualTo(15));
                Assert.That(first.Count,               Is.EqualTo(14));
                Assert.That(first.UnreadItems.Count(), Is.EqualTo(14));

                var src = vm.Data.Root.OfType<RssCategory>().First();
                vm.SelectEntry.Execute(src.Entries.First());

                var dest = vm.Data.LastEntry.Value;
                Assert.That(dest.Title, Is.EqualTo("CubeSoft Blog"));
                Assert.That(dest.Items.Count(),       Is.EqualTo(10));
                Assert.That(dest.Count,               Is.EqualTo(9), nameof(RssEntry));
                Assert.That(dest.UnreadItems.Count(), Is.EqualTo(9));
                Assert.That(src.Count,                Is.EqualTo(9), nameof(RssCategory));

                vm.SelectArticle.Execute(dest.UnreadItems.First());
                Assert.That(vm.Data.Article.Value,    Is.Not.Null);
                Assert.That(dest.Count,               Is.EqualTo(8), nameof(RssEntry));
                Assert.That(dest.UnreadItems.Count(), Is.EqualTo(8));
                Assert.That(src.Count,                Is.EqualTo(8), nameof(RssCategory));

                vm.Close.Execute(null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Remove
        ///
        /// <summary>
        /// RSS エントリを削除するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Remove()
        {
            using (var vm = Create())
            {
                var mock = new MockMessagReceiver(vm.Messenger);
                var src  = vm.Data.Root.OfType<RssCategory>().First();

                vm.Stop.Execute(null);
                vm.SelectEntry.Execute(src.Entries.First());

                Assert.That(src.Entries.Count, Is.EqualTo(1));
                vm.Remove.Execute(null);
                Assert.That(src.Entries.Count, Is.EqualTo(0));
            }
        }

        #endregion

        #region RegisterViewModel

        /* ----------------------------------------------------------------- */
        ///
        /// VM_NewEntry
        ///
        /// <summary>
        /// 新しい RSS エントリを登録するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewEntry()
        {
            using (var vm = Create())
            {
                var src = new Uri("http://clown.hatenablog.jp/feed");
                vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
                vm.NewEntry.Execute(null);

                var entry = vm.Data.Root
                              .OfType<RssEntry>()
                              .FirstOrDefault(e => e.Uri == src);

                Assert.That(entry.Title, Is.EqualTo("Life like a clown"));
                Assert.That(entry.Link,  Is.EqualTo(new Uri("http://clown.hatenablog.jp/")));
                Assert.That(entry.Count, Is.AtLeast(1));
            }
        }

        #endregion

        #region PropertyViewModel

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Property
        ///
        /// <summary>
        /// RSS エントリの情報を編集するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Property()
        {
            using (var vm = Create())
            {
                var src = vm.Data.Root.OfType<RssCategory>().First();

                vm.Stop.Execute(null);
                vm.Messenger.Register<PropertyViewModel>(this, e => PropertyCommand(e));
                vm.SelectEntry.Execute(src.Entries.First());
                vm.Property.Execute(null);

                var dest = vm.Data.Current.Value as RssEntry;
                Assert.That(dest, Is.Not.Null);
                Assert.That(dest.Title, Is.EqualTo(nameof(PropertyCommand)));
                Assert.That(dest.Frequency, Is.EqualTo(RssCheckFrequency.None));
            }
        }

        #endregion

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

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterCommand
        ///
        /// <summary>
        /// Add コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task RegisterCommand(RegisterViewModel vm, Uri src)
        {
            Assert.That(vm.Execute.CanExecute(null), Is.False);
            vm.Url.Value = src.ToString();
            Assert.That(vm.Execute.CanExecute(null), Is.True);

            Assert.That(vm.Busy.Value, Is.False);
            vm.Execute.Execute(null);
            Assert.That(vm.Busy.Value, Is.True);

            for (var i = 0; i < 100 && vm.Busy.Value; ++i)
            {
                await Task.Delay(50).ConfigureAwait(false);
            }
            Assert.That(vm.Busy.Value, Is.False);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PropertyCommand
        ///
        /// <summary>
        /// Property コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void PropertyCommand(PropertyViewModel vm)
        {
            Assert.That(vm.Entry.HasValue, Is.True);
            Assert.That(vm.Frequencies.Count(), Is.EqualTo(4));

            var dest = vm.Entry.Value;
            dest.Title = nameof(PropertyCommand);
            dest.Frequency = RssCheckFrequency.None;

            vm.Apply.Execute(null);
        }

        #endregion
    }
}
