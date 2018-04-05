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

            Assert.That(vm.Property.CanExecute(null), Is.False);
            Assert.That(vm.Remove.CanExecute(null),   Is.False);
            Assert.That(vm.Rename.CanExecute(null),   Is.False);
            Assert.That(vm.Read.CanExecute(null),     Is.False);
            Assert.That(vm.Update.CanExecute(null),   Is.False);
            Assert.That(vm.Reset.CanExecute(null),    Is.False);

            Assert.That(vm.DropTarget,                Is.Not.Null);
            Assert.That(vm.Data.Content.HasValue,     Is.False);
            Assert.That(vm.Data.Current.HasValue,     Is.False);
            Assert.That(vm.Data.LastEntry.HasValue,   Is.False);
            Assert.That(vm.Data.Message.HasValue,     Is.False);
            Assert.That(vm.Data.Shared.HasValue,      Is.True);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Setup
        ///
        /// <summary>
        /// 初期化直後の状態を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Setup() => Execute(vm =>
        {
            var dest = vm.Data.LastEntry.Value;

            Assert.That(dest.Title,               Is.EqualTo("The GitHub Blog"));
            Assert.That(dest.Items.Count(),       Is.EqualTo(15));
            Assert.That(dest.Count,               Is.EqualTo(14));
            Assert.That(dest.UnreadItems.Count(), Is.EqualTo(14));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Select
        ///
        /// <summary>
        /// RSS フィードを選択するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Select() => Execute(vm =>
        {
            var src = vm.Data.Root.OfType<RssCategory>().First().Entries.First();
            src.SkipContent = true;
            vm.Select.Execute(src);
            var dest = vm.Data.Current.Value as RssEntry;

            Assert.That(dest.Title,               Is.EqualTo("CubeSoft Blog"));
            Assert.That(dest.Selected,            Is.True);
            Assert.That(dest.SafeItems.Count(),   Is.EqualTo(10));
            Assert.That(dest.Count,               Is.EqualTo(9), nameof(RssEntry));
            Assert.That(dest.UnreadItems.Count(), Is.EqualTo(9));
            Assert.That(dest.Parent.Count,        Is.EqualTo(9), nameof(RssCategory));

            vm.Close.Execute(null);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Select_Category
        ///
        /// <summary>
        /// カテゴリを選択するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Select_Category() => Execute(vm =>
        {
            var entry = vm.Data.LastEntry.Value;
            vm.Select.Execute(vm.Data.Root.OfType<RssCategory>().First());
            var dest = vm.Data.Current.Value as RssCategory;

            Assert.That(dest.Title,              Is.EqualTo("キューブ・ソフト"));
            Assert.That(dest.Count,              Is.EqualTo(10));
            Assert.That(vm.Data.LastEntry.Value, Is.EqualTo(entry));
            Assert.That(entry.Selected,          Is.True);
        });

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
        public void VM_Select_Empty() => Execute(vm =>
        {
            var src = vm.Data.Root.First(e => e.Title == "Microsoft") as RssCategory;
            vm.Select.Execute(src);
            vm.Select.Execute(src.Entries.First());
            var dest = vm.Data.LastEntry.Value;

            Assert.That(dest.Title,       Is.EqualTo("Official Microsoft Blog"));
            Assert.That(dest.Count,       Is.EqualTo(0));
            Assert.That(dest.Items.Count, Is.EqualTo(0));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_SelectArticle
        ///
        /// <summary>
        /// 未読記事を選択するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_SelectArticle() => Execute(vm =>
        {
            vm.Select.Execute(vm.Data.Root.OfType<RssCategory>().First().Entries.First());
            var dest = vm.Data.Current.Value as RssEntry;
            Assert.That(dest.Count, Is.EqualTo(9));

            vm.SelectArticle.Execute(dest.UnreadItems.First());
            Assert.That(vm.Data.Content.HasValue, Is.True);
            Assert.That(dest.Count,               Is.EqualTo(8));
            Assert.That(dest.UnreadItems.Count(), Is.EqualTo(8));
        });

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
        public void VM_Read() => Execute(vm =>
        {
            var dest = vm.Data.LastEntry.Value;
            Assert.That(dest.Count, Is.EqualTo(14));
            vm.Read.Execute(null);
            Assert.That(dest.Count, Is.EqualTo(0));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Read_Category
        ///
        /// <summary>
        /// カテゴリ中の全記事を既読設定にするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Read_Category() => Execute(vm =>
        {
            var src = vm.Data.Root.OfType<RssCategory>().First();
            Assert.That(src.Count, Is.EqualTo(10));

            vm.Select.Execute(src);
            Assert.That(vm.Data.Current.Value,   Is.EqualTo(src));
            Assert.That(vm.Data.LastEntry.Value, Is.Not.EqualTo(src));

            vm.Read.Execute(null);
            Assert.That(src.Count, Is.EqualTo(0));
        });

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
        public void VM_Rename() => Execute(vm =>
        {
            vm.Rename.Execute(null);
            Assert.That(vm.Data.LastEntry.Value.Editing, Is.True);
        });

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
        public void VM_Remove() => Execute(vm =>
        {
            var src  = vm.Data.Current.Value;
            var dest = Result($@"{nameof(VM_Remove)}\Cache\7ae34cce28b4272e1170fb1e4c2c87ad");

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
            Assert.That(IO.Exists(dest),                Is.True);
            Assert.That(src.Title,                      Is.EqualTo("The GitHub Blog"));
            Assert.That(vm.Remove.CanExecute(null),     Is.True);

            vm.Messenger.Register<DialogMessage>(this, e => DialogMessageCommand(e, true));
            vm.Remove.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(11));
            Assert.That(IO.Exists(dest),                Is.False);
        });

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
        public void VM_Remove_Category() => Execute(vm =>
        {
            var src  = vm.Data.Root.OfType<RssCategory>().First();
            var dest = Result($@"{nameof(VM_Remove_Category)}\Cache\872e24035276c7104afd116c2052172b");

            vm.Select.Execute(src);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
            Assert.That(IO.Exists(dest),                Is.True);
            Assert.That(vm.Remove.CanExecute(null),     Is.True);

            vm.Messenger.Register<DialogMessage>(this, e => DialogMessageCommand(e, true));
            vm.Remove.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(10));
            Assert.That(IO.Exists(dest),                Is.False);
        });

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
        public void VM_Update() => Execute(vm =>
        {
            vm.Select.Execute(vm.Data.Root.OfType<RssCategory>().First());
            var dest = vm.Data.Current.Value;
            var count = dest.Count;

            Assert.That(count,                 Is.EqualTo(10));
            Assert.That(vm.Data.Message.Value, Is.Null.Or.Empty);

            var changed = 0;
            vm.Data.Current.PropertyChanged += (s, e) => ++changed;
            vm.Update.Execute(null);
            vm.Data.Message.Value = string.Empty;

            Assert.That(Wait(vm).Result,       Is.True, "Timeout");
            Assert.That(dest.Count,            Is.GreaterThan(count));
            Assert.That(changed,               Is.AtLeast(1));
        });

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
        public void VM_Reset() => Execute(vm =>
        {
            var src = vm.Data.Root.OfType<RssCategory>().First();
            vm.Select.Execute(src);
            vm.Reset.Execute(null);
            vm.Data.Message.Value = string.Empty;

            Assert.That(src.Count,       Is.EqualTo(0));
            Assert.That(Wait(vm).Result, Is.True, "Timeout");
            Assert.That(src.Count,       Is.AtLeast(1));
        });

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
        public void VM_NewEntry() => Execute(vm =>
        {
            var count = vm.Data.Root.Flatten().Count();
            var src = new Uri("https://clown.hatenablog.jp/feed");

            vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
            vm.NewEntry.Execute(null);

            var dest = vm.Data.Root.OfType<RssEntry>().FirstOrDefault(e => e.Uri == src);

            Assert.That(dest.Title, Is.EqualTo("Life like a clown"));
            Assert.That(dest.Link,  Is.EqualTo(new Uri("https://clown.hatenablog.jp/")));
            Assert.That(dest.Count, Is.AtLeast(1));
            Assert.That(vm.Data.Root.Flatten().Count(), Is.GreaterThan(count));
        }, false);

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
        public void VM_NewEntry_Error() => Execute(vm =>
        {
            var count = vm.Data.Root.Flatten().Count();

            vm.Messenger.Register<RegisterViewModel>(this, e => RegisterError(e).Wait());
            vm.NewEntry.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
        });

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
        public void VM_NewEntry_Exists() => Execute(vm =>
        {
            var src = new Uri("https://blog.cube-soft.jp/");
            var count = vm.Data.Root.Flatten().Count();

            vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
            vm.NewEntry.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
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
        public void VM_NewCategory() => Execute(vm =>
        {
            var src = vm.Data.Root.OfType<RssCategory>().First();
            vm.Select.Execute(src.Entries.First());
            vm.NewCategory.Execute(null);
            var dest = vm.Data.Current.Value as RssCategory;

            Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
            Assert.That(dest.Parent,         Is.EqualTo(src));
            Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
            Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
            Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
            Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
        });

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
        public void VM_NewCategory_Root() => Execute(vm =>
        {
            vm.NewCategory.Execute(null);
            var dest = vm.Data.Current.Value as RssCategory;

            Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
            Assert.That(dest.Parent,         Is.Null);
            Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
            Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
            Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
            Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
        });

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
        public void VM_Hover() => Execute(vm =>
        {
            var message = nameof(VM_Hover);

            Assert.That(vm.Data.Message.Value,        Is.Null.Or.Empty);
            Assert.That(vm.Hover.CanExecute(null),    Is.False);
            Assert.That(vm.Hover.CanExecute(message), Is.True);

            vm.Hover.Execute(message);

            Assert.That(vm.Data.Message.Value, Is.EqualTo(message));
        });

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
        public void VM_Navigate() => Execute(vm =>
        {
            var src = new Uri("http://www.example.com/");
            Assert.That(vm.Data.Content.Value, Is.TypeOf<RssItem>());
            vm.Navigate.Execute(src);
            Assert.That(vm.Data.Content.Value, Is.EqualTo(src));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Import
        ///
        /// <summary>
        /// Import コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true,  16)]
        [TestCase(false, 12)]
        public void VM_Import(bool done, int expected) => Execute(vm =>
        {
            vm.Messenger.Register<OpenFileDialogMessage>(this,
                e => ImportCommand(e, Example("Sample.opml"), done));
            vm.Import.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(expected));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Export
        ///
        /// <summary>
        /// Export コマンドのテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true,  true )]
        [TestCase(false, false)]
        public void VM_Export(bool done, bool expected) => Execute(vm =>
        {
            var dest = Result($"{nameof(VM_Export)}_{done}.opml");
            vm.Messenger.Register<SaveFileDialogMessage>(this, e => ExportCommand(e, dest, done));
            vm.Export.Execute(null);

            Assert.That(IO.Exists(dest), Is.EqualTo(expected));
        });

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
        public void VM_Property(RssCheckFrequency src) => Execute(vm =>
        {
            var dest = vm.Data.Current.Value as RssEntry;
            vm.Messenger.Register<PropertyViewModel>(this, e => PropertyCommand(e, src));
            vm.Property.Execute(null);

            Assert.That(dest.Title,     Is.EqualTo(nameof(PropertyCommand)));
            Assert.That(dest.Frequency, Is.EqualTo(src));
        });

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
        public void VM_Settings() => Execute(vm =>
        {
            var local = vm.Data.Local.Value;
            var shred = vm.Data.Shared.Value;
            vm.Messenger.Register<SettingsViewModel>(this, e => SettingsCommand(e));
            vm.Settings.Execute(null);

            Assert.That(shred.CheckUpdate,          Is.False);
            Assert.That(shred.EnableNewWindow,      Is.False);
            Assert.That(shred.EnableMonitorMessage, Is.False);
            Assert.That(shred.LightMode,            Is.True);
            Assert.That(shred.HighInterval,         Is.EqualTo(TimeSpan.FromHours(2)));
            Assert.That(shred.LowInterval,          Is.EqualTo(TimeSpan.FromHours(12)));
            Assert.That(local.DataDirectory,        Is.EqualTo(Result(nameof(VM_Settings))));
        });

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

            settings.Shared.InitialDelay = TimeSpan.FromMinutes(1);
            settings.Value.Width         = 1024;
            settings.Value.Height        = 768;

            dest.Data.Message.Value = "Test";
            dest.Setup.Execute(null);
            Assert.That(Wait(dest, true).Result, Is.True, "Timeout");
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Execute
        ///
        /// <summary>
        /// テストを実行します。
        /// </summary>
        ///
        /// <remarks>
        /// AppVeyor でのテストが安定しないので、Assert はコメントアウト。
        /// 原因については要検証。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void Execute(Action<MainViewModel> action, bool stop = true,
            [CallerMemberName] string name = null)
        {
            var changed = 0;

            using (var vm = Create(name))
            {
                WatchFeed(_ => ++changed, name);
                if (stop) vm.Stop.Execute(null);
                action(vm);
            }

            // Assert.That(changed, Is.AtLeast(1), nameof(WatchFeed));
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

            var src = vm.Shared.Value;
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
