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
using Cube.Net.App.Rss.Reader;
using Cube.Net.Rss;
using Cube.Xui;
using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

            Assert.That(vm.Property.CanExecute(null),  Is.False);
            Assert.That(vm.Remove.CanExecute(null),    Is.False);
            Assert.That(vm.Rename.CanExecute(null),    Is.False);
            Assert.That(vm.Read.CanExecute(null),      Is.False);
            Assert.That(vm.Update.CanExecute(null),    Is.False);
            Assert.That(vm.Reset.CanExecute(null),     Is.False);

            Assert.That(vm.DropTarget,                 Is.Not.Null);
            Assert.That(vm.Data.Content.HasValue,      Is.False);
            Assert.That(vm.Data.Current.HasValue,      Is.False);
            Assert.That(vm.Data.LastEntry.HasValue,    Is.False);
            Assert.That(vm.Data.Message.HasValue,      Is.False);
            Assert.That(vm.Data.User.HasValue,         Is.True);

            var user = vm.Data.User.Value;

            Assert.That(user.Width,                    Is.AtLeast(1));
            Assert.That(user.Height,                   Is.AtLeast(1));
            Assert.That(user.StartUri,                 Is.Null);
            Assert.That(user.DataDirectory,            Is.Not.Null.And.Not.Empty);
            Assert.That(user.EnableNewWindow,          Is.False);
            Assert.That(user.EnableMonitorMessage,     Is.True);
            Assert.That(user.LightMode,                Is.False);
            Assert.That(user.CheckUpdate,              Is.True);
            Assert.That(user.HighInterval.Value,       Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(user.LowInterval.Value,        Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(user.LastCheckUpdate.HasValue, Is.False);
            Assert.That(user.InitialDelay.HasValue,    Is.True);
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
                src.Entries.First().SkipContent = true;
                vm.Select.Execute(src.Entries.First());

                var dest = vm.Data.Current.Value as RssEntry;
                Assert.That(dest.Title,               Is.EqualTo("CubeSoft Blog"));
                Assert.That(dest.Selected,            Is.True);
                Assert.That(dest.SafeItems.Count(),   Is.EqualTo(10));
                Assert.That(dest.Count,               Is.EqualTo(9), nameof(RssEntry));
                Assert.That(dest.UnreadItems.Count(), Is.EqualTo(9));
                Assert.That(src.Count,                Is.EqualTo(9), nameof(RssCategory));

                vm.Select.Execute(src);
                var category = vm.Data.Current.Value as RssCategory;
                Assert.That(category.Title,           Is.EqualTo("キューブ・ソフト"));
                Assert.That(category.Count,           Is.EqualTo(9));
                Assert.That(vm.Data.LastEntry.Value,  Is.EqualTo(dest));
                Assert.That(dest.Selected,            Is.True);

                vm.SelectArticle.Execute(dest.UnreadItems.First());
                Assert.That(vm.Data.Content.HasValue, Is.True);
                Assert.That(dest.Count,               Is.EqualTo(8), nameof(RssEntry));
                Assert.That(dest.UnreadItems.Count(), Is.EqualTo(8));
                Assert.That(src.Count,                Is.EqualTo(8), nameof(RssCategory));

                vm.Close.Execute(null);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Select_Empty
        ///
        /// <summary>
        /// 新着記事のない RSS フィードを選択した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Select_Empty()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);

                var src = vm.Data.Root.First(e => e.Title == "Microsoft") as RssCategory;
                vm.Select.Execute(src);
                vm.Select.Execute(src.Entries.First());

                var dest = vm.Data.LastEntry.Value;
                Assert.That(dest.Title, Is.EqualTo("Official Microsoft Blog"));
                Assert.That(dest.Count, Is.EqualTo(0));
                Assert.That(dest.Items.Count, Is.EqualTo(0));
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
                vm.Stop.Execute(null);
                vm.Messenger.Register<DialogMessage>(this, e => DialogMessageCommand(e, true));

                var dir  = Result($@"{nameof(VM_Remove)}\Cache");
                var src  = vm.Data.Current.Value;
                var dest = IO.Combine(dir, "7ae34cce28b4272e1170fb1e4c2c87ad");

                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
                Assert.That(IO.Exists(dest), Is.True);
                Assert.That(src.Title, Is.EqualTo("The GitHub Blog"));
                Assert.That(vm.Remove.CanExecute(null), Is.True);
                vm.Remove.Execute(null);
                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(11));
                Assert.That(IO.Exists(dest), Is.False);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Remove_Category
        ///
        /// <summary>
        /// カテゴリおよびカテゴリ中の RSS エントリを削除するテストを
        /// 実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Remove_Category()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.Messenger.Register<DialogMessage>(this, e => DialogMessageCommand(e, true));

                var dir  = Result($@"{nameof(VM_Remove_Category)}\Cache");
                var src  = vm.Data.Root.OfType<RssCategory>().First();
                var dest = IO.Combine(dir, "872e24035276c7104afd116c2052172b");

                vm.Select.Execute(src);
                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
                Assert.That(IO.Exists(dest), Is.True);
                Assert.That(vm.Remove.CanExecute(null), Is.True);
                vm.Remove.Execute(null);
                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(10));
                Assert.That(IO.Exists(dest), Is.False);
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
        /// RSS エントリの全記事を既読設定にするテストを実行します。
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
        /// VM_Read
        ///
        /// <summary>
        /// カテゴリ中の全記事を既読設定にするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Read_Category()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);

                var src = vm.Data.Root.OfType<RssCategory>().First();
                Assert.That(src.Count, Is.EqualTo(10));

                vm.Select.Execute(src);
                Assert.That(vm.Data.Current.Value,   Is.EqualTo(src));
                Assert.That(vm.Data.LastEntry.Value, Is.Not.EqualTo(src));

                vm.Read.Execute(null);
                Assert.That(src.Count, Is.EqualTo(0));
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
                var src = vm.Data.Root.OfType<RssCategory>().First();
                vm.Stop.Execute(null);
                vm.Select.Execute(src);

                var dest  = vm.Data.Current.Value;
                var count = dest.Count;
                Assert.That(count, Is.EqualTo(10));
                Assert.That(vm.Data.Message.Value, Is.Null.Or.Empty);
                vm.Update.Execute(null);
                vm.Data.Message.Value = string.Empty;
                Assert.That(Wait(vm).Result, Is.True, "Timeout");
                Assert.That(dest.Count, Is.AtLeast(count));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Reset
        ///
        /// <summary>
        /// キャッシュを削除して更新するテストを実行します。
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
                vm.Select.Execute(src);
                vm.Reset.Execute(null);
                vm.Data.Message.Value = string.Empty;

                Assert.That(src.Count, Is.EqualTo(0));
                Assert.That(Wait(vm).Result, Is.True, "Timeout");
                Assert.That(src.Count, Is.AtLeast(1));
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
                var count = vm.Data.Root.Flatten().Count();
                var src   = new Uri("https://clown.hatenablog.jp/feed");

                vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
                vm.NewEntry.Execute(null);

                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count + 1));

                var entry = vm.Data.Root
                              .OfType<RssEntry>()
                              .FirstOrDefault(e => e.Uri == src);

                Assert.That(entry.Title, Is.EqualTo("Life like a clown"));
                Assert.That(entry.Link,  Is.EqualTo(new Uri("https://clown.hatenablog.jp/")));
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
        public void VM_NewEntry_Error()
        {
            using (var vm = Create())
            {
                var count = vm.Data.Root.Flatten().Count();

                vm.Stop.Execute(null);
                vm.Messenger.Register<RegisterViewModel>(this, e => RegisterError(e).Wait());
                vm.NewEntry.Execute(null);

                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_NewEntry_Exists
        ///
        /// <summary>
        /// 既に存在する RSS エントリの登録を試みた時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewEntry_Exists()
        {
            using (var vm = Create())
            {
                var count = vm.Data.Root.Flatten().Count();
                var src   = new Uri("https://blog.cube-soft.jp/");

                vm.Stop.Execute(null);
                vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
                vm.NewEntry.Execute(null);

                Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
            }
        }

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
                vm.Select.Execute(src.Entries.First());
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
        /// VM_NewCategory_Root
        ///
        /// <summary>
        /// ルートに新しいカテゴリを追加するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewCategory_Root()
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.NewCategory.Execute(null);

                var dest = vm.Data.Current.Value as RssCategory;
                Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
                Assert.That(dest.Parent,         Is.Null);
                Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
                Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
                Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
                Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_NewCategory_Empty
        ///
        /// <summary>
        /// 何も存在しない状態で新しいカテゴリを追加した時の挙動を
        /// 確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_NewCategory_Empty()
        {
            var vm = new MainViewModel();
            vm.NewCategory.Execute(null);

            var dest = vm.Data.Current.Value as RssCategory;
            Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
            Assert.That(dest.Parent,         Is.Null);
            Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
            Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
            Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
            Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
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
                Assert.That(vm.Data.Message.Value, Is.Null.Or.Empty);
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
                Assert.That(vm.Data.Content.Value, Is.TypeOf<RssItem>());
                vm.Navigate.Execute(uri);
                Assert.That(vm.Data.Content.Value, Is.EqualTo(uri));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Import
        ///
        /// <summary>
        /// Import コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Import()
        {
            using (var vm = Create())
            {
                var src = Example("Sample.opml");
                vm.Stop.Execute(null);
                var count = vm.Data.Root.Flatten().Count();

                vm.Messenger.Register<OpenFileDialogMessage>(this, e => ImportCommand(e, src, true));
                vm.Import.Execute(null);

                var dest = vm.Data.Root.Flatten();
                Assert.That(dest.Count(), Is.EqualTo(count + 4));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Import_Cancel
        ///
        /// <summary>
        /// Import コマンドをキャンセルした時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Import_Cancel()
        {
            using (var vm = Create())
            {
                var src = Example("Sample.opml");
                vm.Stop.Execute(null);
                var count = vm.Data.Root.Flatten().Count();

                vm.Messenger.Register<OpenFileDialogMessage>(this, e => ImportCommand(e, src, false));
                vm.Import.Execute(null);

                var dest = vm.Data.Root.Flatten();
                Assert.That(dest.Count(), Is.EqualTo(count));
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
                vm.Stop.Execute(null);
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
        [TestCase(RssCheckFrequency.Auto)]
        [TestCase(RssCheckFrequency.High)]
        [TestCase(RssCheckFrequency.Low)]
        [TestCase(RssCheckFrequency.None)]
        public void VM_Property(RssCheckFrequency src)
        {
            using (var vm = Create())
            {
                vm.Stop.Execute(null);
                vm.Messenger.Register<PropertyViewModel>(this, e => PropertyCommand(e, src));
                vm.Property.Execute(null);

                var dest = vm.Data.Current.Value as RssEntry;
                Assert.That(dest, Is.Not.Null);
                Assert.That(dest.Title, Is.EqualTo(nameof(PropertyCommand)));
                Assert.That(dest.Frequency, Is.EqualTo(src));
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
                Assert.That(dest.EnableNewWindow,      Is.False);
                Assert.That(dest.EnableMonitorMessage, Is.False);
                Assert.That(dest.LightMode,            Is.True);
                Assert.That(dest.HighInterval,         Is.EqualTo(TimeSpan.FromHours(2)));
                Assert.That(dest.LowInterval,          Is.EqualTo(TimeSpan.FromHours(12)));
                Assert.That(dest.DataDirectory,        Is.EqualTo(Result(nameof(VM_Settings))));
            }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// ViewModel オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private MainViewModel Create([CallerMemberName] string name = null)
        {
            var root     = Copy(name);
            var settings = new SettingsFolder(root, IO);
            var dest     = new MainViewModel(settings);

            settings.Value.InitialDelay = TimeSpan.FromMinutes(1);
            settings.Value.Width        = 1024;
            settings.Value.Height       = 768;

            dest.Data.Message.Value = "Test";
            dest.Setup.Execute(null);
            Assert.That(Wait(dest, true).Result, Is.True, "Timeout");
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Wait
        ///
        /// <summary>
        /// メッセージを受信するまで待機します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<bool> Wait(MainViewModel vm, bool empty = false)
        {
            for (var i = 0; i < 100; ++i)
            {
                if (string.IsNullOrEmpty(vm.Data.Message.Value) == empty) return true;
                await Task.Delay(50).ConfigureAwait(false);
            }
            return false;
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
        private void PropertyCommand(PropertyViewModel vm, RssCheckFrequency value)
        {
            Assert.That(vm.Entry.HasValue, Is.True);
            Assert.That(vm.Frequencies.Count(), Is.EqualTo(4));

            var dest = vm.Entry.Value;
            dest.Title = nameof(PropertyCommand);
            dest.Frequency = value;

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
            src.EnableNewWindow      = false;
            src.EnableMonitorMessage = false;
            src.LightMode            = true;
            src.HighInterval         = TimeSpan.FromHours(2);
            src.LowInterval          = TimeSpan.FromHours(12);

            vm.SelectDataDirectory.Execute(null);
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
        /// ImportCommand
        ///
        /// <summary>
        /// Import コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ImportCommand(OpenFileDialogMessage e, string src, bool result)
        {
            e.FileName = src;
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
        private void DialogMessageCommand(DialogMessage e, bool result)
        {
            Assert.That(e.Content, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Title,   Is.Not.Null.And.Not.Empty);

            e.Result = result;
            e.Callback(e);
        }

        #endregion
    }
}
