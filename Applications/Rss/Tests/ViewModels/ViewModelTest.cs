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
using System.Threading;
using System.Threading.Tasks;
using Cube.Net.App.Rss.Reader;
using Cube.Xui;
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
    class ViewModelTest : FileHelper
    {
        #region Tests

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
                Assert.That(dest.Title,               Is.EqualTo("CubeSoft Blog"));
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
                var src = vm.Data.Root.OfType<RssCategory>().First();

                vm.Stop.Execute(null);
                vm.Messenger.Register<DialogMessage>(this, e => DialogMessageCommand(e));
                vm.SelectEntry.Execute(src.Entries.First());

                Assert.That(src.Entries.Count, Is.EqualTo(1));
                vm.Remove.Execute(null);
                Assert.That(src.Entries.Count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Rename
        ///
        /// <summary>
        /// RSS エントリの名前を変更するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Rename()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.Rename.Execute(null);
                Assert.That(vm.Data.LastEntry.Value.Editing, Is.True);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Read
        ///
        /// <summary>
        /// RSS エントリの名前を変更するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Read()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                Assert.That(vm.Data.LastEntry.Value.Count, Is.EqualTo(14));
                vm.Read.Execute(null);
                Assert.That(vm.Data.LastEntry.Value.Count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Update
        ///
        /// <summary>
        /// RSS エントリを更新するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Update()
        {
            using (var vm = Create())
            {
                var src  = vm.Data.Root.OfType<RssCategory>().First();
                var dest = src.Entries.First();

                vm.Stop.Execute(null);
                vm.SelectEntry.Execute(dest);

                var count = dest.Count;
                Assert.That(count, Is.EqualTo(9));
                Assert.That(vm.Data.Message.HasValue, Is.False);
                vm.Update.Execute(null);
                WaitMessage(vm);
                Assert.That(dest.Count, Is.AtLeast(count));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Update
        ///
        /// <summary>
        /// RSS エントリを更新するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Reset()
        {
            using (var vm = Create())
            {
                var src = vm.Data.Root.OfType<RssCategory>().First();
                vm.Stop.Execute(null);
                vm.SelectEntry.Execute(src);
                Assert.That(vm.Reset.CanExecute(null), Is.False);

                var dest = src.Entries.First();
                vm.SelectEntry.Execute(dest);
                Assert.That(vm.Reset.CanExecute(null), Is.True);

                vm.Reset.Execute(null);
                Assert.That(dest.Count, Is.EqualTo(0));
                WaitMessage(vm);
                Assert.That(dest.Count, Is.AtLeast(1));
            }
        }

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

        /* ----------------------------------------------------------------- */
        ///
        /// VM_NewEntry_Error
        ///
        /// <summary>
        /// 新しい RSS エントリの登録に失敗する時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewEntry_Error() => Assert.DoesNotThrow(() =>
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.Messenger.Register<RegisterViewModel>(this, e => RegisterError(e).Wait());
                vm.NewEntry.Execute(null);
            }
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_NewCategor
        ///
        /// <summary>
        /// 新しいカテゴリを追加するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewCategory()
        {
            using (var vm = Create())
            {
                var src = vm.Data.Root.OfType<RssCategory>().First();

                vm.Stop.Execute(null);
                vm.SelectEntry.Execute(src.Entries.First());
                vm.NewCategory.Execute(null);

                var dest = vm.Data.Current.Value as RssCategory;
                Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
                Assert.That(dest.Parent,         Is.EqualTo(src));
                Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
                Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
                Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
                Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Hover
        ///
        /// <summary>
        /// Hover コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Hover()
        {
            using (var vm = Create())
            {
                var message = nameof(VM_Hover);

                vm.Stop.Execute(null);
                Assert.That(vm.Data.Message.HasValue, Is.False);
                Assert.That(vm.Hover.CanExecute(null), Is.False);
                Assert.That(vm.Hover.CanExecute(message), Is.True);
                vm.Hover.Execute(message);
                Assert.That(vm.Data.Message.Value, Is.EqualTo(message));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Navigate
        ///
        /// <summary>
        /// Navigate コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Navigate()
        {
            using (var vm = Create())
            {
                var uri = new Uri("http://www.example.com/");

                vm.Stop.Execute(null);
                Assert.That(vm.Data.Uri.HasValue, Is.False);
                vm.Navigate.Execute(uri);
                Assert.That(vm.Data.Uri.Value, Is.EqualTo(uri));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Export
        ///
        /// <summary>
        /// Export コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Export()
        {
            using (var vm = Create())
            {
                var dest = Result($"{nameof(VM_Export)}.opml");
                vm.Messenger.Register<SaveFileDialogMessage>(this, e => ExportCommand(e, dest, true));
                vm.Export.Execute(null);

                var info = IO.Get(dest);
                Assert.That(info.Exists, Is.True);
                Assert.That(info.Length, Is.AtLeast(1));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Export_Cancel
        ///
        /// <summary>
        /// Export コマンドをキャンセルした時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Export_Cancel()
        {
            using (var vm = Create())
            {
                var dest = Result($"{nameof(VM_Export_Cancel)}.opml");
                vm.Messenger.Register<SaveFileDialogMessage>(this, e => ExportCommand(e, dest, false));
                vm.Export.Execute(null);
                Assert.That(IO.Exists(dest), Is.False);
            }
        }

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
                vm.Stop.Execute(null);
                vm.Messenger.Register<PropertyViewModel>(this, e => PropertyCommand(e));
                vm.Property.Execute(null);

                var dest = vm.Data.Current.Value as RssEntry;
                Assert.That(dest, Is.Not.Null);
                Assert.That(dest.Title, Is.EqualTo(nameof(PropertyCommand)));
                Assert.That(dest.Frequency, Is.EqualTo(RssCheckFrequency.None));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Settings
        ///
        /// <summary>
        /// ユーザ設定を編集するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Settings()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.Messenger.Register<SettingsViewModel>(this, e => SettingsCommand(e));
                vm.Settings.Execute(null);

                var dest = vm.Data.User.Value;
                Assert.That(dest.CheckUpdate,          Is.False);
                Assert.That(dest.EnableMonitorMessage, Is.False);
                Assert.That(dest.LightMode,            Is.True);
                Assert.That(dest.HighInterval,         Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(dest.LowInterval,          Is.EqualTo(TimeSpan.FromHours(12)));
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
        private MainViewModel Create()
        {
            var dest = new MainViewModel(new SettingsFolder(Results, IO));
            dest.Setup.Execute(null);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AwaitMessage
        ///
        /// <summary>
        /// メッセージを受信するまで待機します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WaitMessage(MainViewModel vm) => Task.Run(async () =>
        {
            for (var i = 0; i < 100; ++i)
            {
                if (vm.Data.Message.HasValue) return;
                await Task.Delay(50);
            }
        }).Wait();

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
        /// RegisterError
        ///
        /// <summary>
        /// 無効な URL を登録しようとします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task RegisterError(RegisterViewModel vm)
        {
            var cts = new CancellationTokenSource();

            vm.Url.Value = "error";
            vm.Messenger.Register<DialogMessage>(this, e =>
            {
                e.Result = true;
                cts.Cancel();
            });
            vm.Execute.Execute(null);

            try { await Task.Delay(1000, cts.Token).ConfigureAwait(false); }
            catch (TaskCanceledException /* err */) { /* OK */ }
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

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsCommand
        ///
        /// <summary>
        /// Settings コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SettingsCommand(SettingsViewModel vm)
        {
            Assert.That(vm.Logo,      Is.Not.Null);
            Assert.That(vm.Copyright, Does.StartWith("Copyright"));
            Assert.That(vm.Framework, Does.StartWith("Microsoft .NET Framework"));
            Assert.That(vm.Product,   Does.StartWith("Cube"));
            Assert.That(vm.Version,   Does.StartWith("Version"));
            Assert.That(vm.Windows,   Does.StartWith("Microsoft Windows"));

            var src = vm.Data.Value;
            src.CheckUpdate          = false;
            src.EnableMonitorMessage = false;
            src.LightMode            = true;
            src.HighInterval         = TimeSpan.FromHours(2);
            src.LowInterval          = TimeSpan.FromHours(12);

            vm.Apply.Execute(null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ExportCommand
        ///
        /// <summary>
        /// Export コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ExportCommand(SaveFileDialogMessage e, string dest, bool result)
        {
            e.FileName = dest;
            e.Result   = result;
            e.Callback(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessageCommand
        ///
        /// <summary>
        /// メッセージボックスを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void DialogMessageCommand(DialogMessage e)
        {
            Assert.That(e.Content, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Title,   Is.Not.Null.And.Not.Empty);

            e.Result = true;
            e.Callback(e);
        }

        #endregion
    }
}
