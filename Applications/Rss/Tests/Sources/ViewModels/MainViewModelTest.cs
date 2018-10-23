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
using Cube.FileSystem.TestService;
using Cube.Generics;
using Cube.Net.Rss.App.Reader;
using Cube.Xui;
using Cube.Xui.Mixin;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Cube.Net.Rss.Tests
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
    class MainViewModelTest : ViewModelFixture
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

            Assert.That(vm.Property.CanExecute(), Is.False);
            Assert.That(vm.Remove.CanExecute(),   Is.False);
            Assert.That(vm.Rename.CanExecute(),   Is.False);
            Assert.That(vm.Read.CanExecute(),     Is.False);
            Assert.That(vm.Update.CanExecute(),   Is.False);
            Assert.That(vm.Reset.CanExecute(),    Is.False);

            Assert.That(vm.DropTarget,            Is.Not.Null);
            Assert.That(vm.Data.Content.Value,    Is.Null);
            Assert.That(vm.Data.Current.Value,    Is.Null);
            Assert.That(vm.Data.LastEntry.Value,  Is.Null);
            Assert.That(vm.Data.Message.Value,    Is.Null);
            Assert.That(vm.Data.Shared.Value,     Is.Not.Null);
            Assert.That(vm.Data.Lock.Value,       Is.Not.Null);
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

            Assert.That(dest,                     Is.Not.Null);
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
            Assert.That(vm.Data.Content.Value,    Is.Not.Null);
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
            var dest = CachePath("7ae34cce28b4272e1170fb1e4c2c87ad");

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
            Assert.That(IO.Exists(dest),                Is.True);
            Assert.That(src.Title,                      Is.EqualTo("The GitHub Blog"));
            Assert.That(vm.Remove.CanExecute(null),     Is.True);

            vm.Register<DialogMessage>(this, e => DialogMessageCommand(e, MessageBoxResult.OK));
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
            var dest = CachePath("872e24035276c7104afd116c2052172b");

            vm.Select.Execute(src);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
            Assert.That(IO.Exists(dest),                Is.True);
            Assert.That(vm.Remove.CanExecute(null),     Is.True);

            vm.Register<DialogMessage>(this, e => DialogMessageCommand(e, MessageBoxResult.OK));
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
            var dest  = vm.Data.Current.Value;
            var count = dest.Count;
            var msg   = vm.Data.Message;

            Assert.That(count,                 Is.EqualTo(10));
            Assert.That(vm.Data.Message.Value, Is.Null.Or.Empty);

            vm.Update.Execute(null);
            msg.Value = string.Empty;

            Assert.That(Wait.For(() => msg.Value.HasValue()), "Timeout");
            Assert.That(dest.Count, Is.GreaterThan(count));
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
            var msg = vm.Data.Message;

            vm.Select.Execute(src);
            vm.Reset.Execute(null);
            vm.Data.Message.Value = string.Empty;

            Assert.That(src.Count, Is.EqualTo(0));
            Assert.That(Wait.For(() => msg.Value.HasValue()), "Timeout");
            Assert.That(src.Count, Is.AtLeast(1));
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
            vm.Register<OpenFileMessage>(this,
                e => ImportCommand(e, GetExamplesWith("Sample.opml"), done));
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
            var dest = GetResultsWith($"{nameof(VM_Export)}_{done}.opml");
            vm.Register<SaveFileMessage>(this, e => ExportCommand(e, dest, done));
            vm.Export.Execute(null);

            Assert.That(IO.Exists(dest), Is.EqualTo(expected));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_ReadOnly
        ///
        /// <summary>
        /// 読み取り専用モードでの挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_ReadOnly()
        {
            var dest = IO.Combine(RootDirectory(), LockSettings.FileName);
            IO.Copy(GetExamplesWith("Sample.lockfile"), dest);
            IO.Copy(GetExamplesWith("LocalSettings.json"), LocalSettingsPath(), true);
            IO.Copy(GetExamplesWith("Settings.json"), SharedSettingsPath(), true);
            IO.Copy(GetExamplesWith("Sample.json"), FeedsPath(), true);
            IO.CreateDirectory(CacheDirectory());

            var asm = Assembly.GetExecutingAssembly();
            var n   = 0;

            using (var fw = new System.IO.FileSystemWatcher())
            using (var vm = new MainViewModel(new SettingsFolder(asm, RootDirectory(), IO)))
            {
                fw.Path = RootDirectory();
                fw.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                fw.Changed += (s, e) => ++n;
                fw.EnableRaisingEvents = true;

                vm.Data.Message.Value = "Test";
                vm.Setup.Execute(null);
                Assert.That(Wait.For(() => !vm.Data.Message.Value.HasValue()), "Timeout");

                Assert.That(vm.Data.Lock.Value.IsReadOnly,   Is.True, nameof(vm.Data.Lock.Value.IsReadOnly));
                Assert.That(vm.Data.Lock.Value.Sid,          Is.EqualTo("S-1-5-21-0123456789-0123456789-012345678-0123"));
                Assert.That(vm.Data.Lock.Value.UserName,     Is.EqualTo("DummyName"));
                Assert.That(vm.Data.Lock.Value.MachineName,  Is.EqualTo("DummyMachine"));

                Assert.That(vm.Data.Current.Value,           Is.Not.Null, nameof(vm.Data.Current));
                Assert.That(vm.Data.LastEntry.Value,         Is.Not.Null, nameof(vm.Data.LastEntry));
                Assert.That(vm.Property.CanExecute(null),    Is.False,    nameof(vm.Property));
                Assert.That(vm.Settings.CanExecute(null),    Is.True,     nameof(vm.Settings));
                Assert.That(vm.Import.CanExecute(null),      Is.False,    nameof(vm.Import));
                Assert.That(vm.Export.CanExecute(null),      Is.True,     nameof(vm.Export));
                Assert.That(vm.NewEntry.CanExecute(null),    Is.False,    nameof(vm.NewEntry));
                Assert.That(vm.NewCategory.CanExecute(null), Is.False,    nameof(vm.NewCategory));
                Assert.That(vm.Remove.CanExecute(null),      Is.False,    nameof(vm.Remove));
                Assert.That(vm.Rename.CanExecute(null),      Is.False,    nameof(vm.Rename));
                Assert.That(vm.Read.CanExecute(null),        Is.True,     nameof(vm.Read));
                Assert.That(vm.Update.CanExecute(null),      Is.True,     nameof(vm.Update));
                Assert.That(vm.Reset.CanExecute(null),       Is.False,    nameof(vm.Reset));

                vm.Update.Execute(null);
                Assert.That(Wait.For(() => vm.Data.Message.Value.HasValue()), "Timeout");
                Assert.That(vm.Data.LastEntry.Value.Count, Is.AtLeast(1));
            }

            Assert.That(n, Is.AtMost(2), "Write only LocalSettings.json");
            Assert.That(IO.GetFiles(CacheDirectory()).Length, Is.EqualTo(0), "Cache");
            Assert.That(IO.Exists(dest), Is.True, dest);
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// ExportCommand
        ///
        /// <summary>
        /// Export コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ExportCommand(SaveFileMessage e, string dest, bool result)
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
        private void ImportCommand(OpenFileMessage e, string src, bool result)
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
        private void DialogMessageCommand(DialogMessage e, MessageBoxResult result)
        {
            Assert.That(e.Content, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Title,   Is.Not.Null.And.Not.Empty);

            e.Result = result;
            e.Callback(e);
        }

        #endregion
    }
}
