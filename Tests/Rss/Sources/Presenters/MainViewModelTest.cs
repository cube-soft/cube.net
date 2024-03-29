﻿/* ------------------------------------------------------------------------- */
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
namespace Cube.Net.Rss.Tests;

using System;
using System.Linq;
using System.Threading;
using Cube.FileSystem;
using Cube.Net.Rss.Reader;
using Cube.Tests;
using Cube.Text.Extensions;
using Cube.Xui.Commands.Extensions;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// MainViewModelTest
///
/// <summary>
/// Tests the MainViewModel class.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class MainViewModelTest : ViewModelFixture
{
    #region Tests

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Default
    ///
    /// <summary>
    /// Tests the MainViewModel properties.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Default()
    {
        var s  = new SettingFolder(typeof(MainViewModelTest).Assembly) { AutoSave = false };
        var vm = new MainViewModel(s, new SynchronizationContext());

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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Setup
    ///
    /// <summary>
    /// Tests the Setup command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Select
    ///
    /// <summary>
    /// Tests the Select command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Select() => Execute(vm =>
    {
        var src = vm.Data.Root.OfType<RssCategory>().First().Entries.First();
        src.SkipContent = true;
        vm.Select.Execute(src);
        var dest = vm.Data.Current.Value as RssEntry;

        Assert.That(dest.Title,               Is.EqualTo("Life like a clown"));
        Assert.That(dest.Selected,            Is.True);
        Assert.That(dest.SafeItems.Count(),   Is.EqualTo(10));
        Assert.That(dest.Count,               Is.EqualTo(9), nameof(RssEntry));
        Assert.That(dest.UnreadItems.Count(), Is.EqualTo(9));
        Assert.That(dest.Parent.Count,        Is.EqualTo(9), nameof(RssCategory));

        vm.Close.Execute(null);
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Select_Category
    ///
    /// <summary>
    /// Tests the Select command with a category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Select_Empty
    ///
    /// <summary>
    /// Tests the Select command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_SelectArticle
    ///
    /// <summary>
    /// Tests the SelectArticle command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Read
    ///
    /// <summary>
    /// Tests the Read command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Read() => Execute(vm =>
    {
        var dest = vm.Data.LastEntry.Value;
        Assert.That(dest.Count, Is.EqualTo(14));
        vm.Read.Execute(null);
        Assert.That(dest.Count, Is.EqualTo(0));
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Read_Category
    ///
    /// <summary>
    /// Tests the Read command with a category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Rename
    ///
    /// <summary>
    /// Tests the Rename command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Rename() => Execute(vm =>
    {
        vm.Rename.Execute(null);
        Assert.That(vm.Data.LastEntry.Value.Editing, Is.True);
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Remove
    ///
    /// <summary>
    /// Tests the Remove command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Remove() => Execute(vm =>
    {
        var src  = vm.Data.Current.Value;
        var dest = CachePath("7ae34cce28b4272e1170fb1e4c2c87ad");

        Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
        Assert.That(Io.Exists(dest),                Is.True);
        Assert.That(src.Title,                      Is.EqualTo("The GitHub Blog"));
        Assert.That(vm.Remove.CanExecute(null),     Is.True);

        using var dc = vm.Subscribe<DialogMessage>(e => DialogMessageCommand(e, DialogStatus.Yes));
        vm.Remove.Execute(null);

        Assert.That(Wait.For(() => vm.Data.Root.Flatten().Count() == 11), "Timeout");
        Assert.That(Io.Exists(dest), Is.False);
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Remove_Category
    ///
    /// <summary>
    /// Tests the Remove command with a category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Remove_Category() => Execute(vm =>
    {
        var src  = vm.Data.Root.OfType<RssCategory>().First();
        var dest = CachePath("f5acb23c3de871999c2034f864464c92");

        vm.Select.Execute(src);

        Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(12));
        Assert.That(Io.Exists(dest),                Is.True);
        Assert.That(vm.Remove.CanExecute(null),     Is.True);

        using var dc = vm.Subscribe<DialogMessage>(e => DialogMessageCommand(e, DialogStatus.Yes));
        vm.Remove.Execute(null);

        Assert.That(Wait.For(() => vm.Data.Root.Flatten().Count() == 10), "Timeout");
        Assert.That(Io.Exists(dest), Is.False);
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Update
    ///
    /// <summary>
    /// Tests the Update command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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
        Assert.That(dest.Count, Is.GreaterThanOrEqualTo(count));
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Reset
    ///
    /// <summary>
    /// Tests the Reset command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewCategor
    ///
    /// <summary>
    /// Tests the NewCategory command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewCategory_Root
    ///
    /// <summary>
    /// Tests the NewCategory command with the root category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewCategory_Empty
    ///
    /// <summary>
    /// Tests the NewCategory command with nothing present.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_NewCategory_Empty()
    {
        var s  = new SettingFolder(typeof(MainViewModelTest).Assembly) { AutoSave = false };
        var vm = new MainViewModel(s, new SynchronizationContext());
        vm.NewCategory.Execute(null);
        var dest = vm.Data.Current.Value as RssCategory;

        Assert.That(dest.Title,          Is.EqualTo("新しいフォルダー"));
        Assert.That(dest.Parent,         Is.Null);
        Assert.That(dest.Count,          Is.EqualTo(0), nameof(dest.Count));
        Assert.That(dest.Children.Count, Is.EqualTo(0), nameof(dest.Children));
        Assert.That(dest.Editing,        Is.True, nameof(dest.Editing));
        Assert.That(dest.Expanded,       Is.False, nameof(dest.Expanded));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Hover
    ///
    /// <summary>
    /// Tests the Hover command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Navigate
    ///
    /// <summary>
    /// Tests the Navigate command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_Navigate() => Execute(vm =>
    {
        var src = new Uri("http://www.example.com/");
        Assert.That(vm.Data.Content.Value, Is.TypeOf<RssItem>());
        vm.Navigate.Execute(src);
        Assert.That(vm.Data.Content.Value, Is.EqualTo(src));
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Import
    ///
    /// <summary>
    /// Tests the Import command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestCase(true,  16)]
    [TestCase(false, 12)]
    public void VM_Import(bool done, int expected) => Execute(vm =>
    {
        using var dc = vm.Subscribe<OpenFileMessage>(e => ImportCommand(e, GetSource("Sample.opml"), done));
        vm.Import.Execute(null);

        Assert.That(Wait.For(() => vm.Data.Root.Flatten().Count() == expected), "Timeout");
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_Export
    ///
    /// <summary>
    /// Tests the Export command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestCase(true,  true )]
    [TestCase(false, false)]
    public void VM_Export(bool done, bool expected) => Execute(vm =>
    {
        var dest = Get($"{nameof(VM_Export)}_{done}.opml");
        using var dc = vm.Subscribe<SaveFileMessage>(e => ExportCommand(e, dest, done));
        vm.Export.Execute(null);

        Assert.That(Wait.For(() => Io.Exists(dest) == expected), "Timeout");
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_ReadOnly
    ///
    /// <summary>
    /// Tests in read-only mode.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_ReadOnly()
    {
        var dest = Io.Combine(RootDirectory(), LockSetting.FileName);
        Io.Copy(GetSource("Sample.lockfile"), dest, true);
        Io.Copy(GetSource("LocalSettings.json"), LocalSettingPath(), true);
        Io.Copy(GetSource("Settings.json"), SharedSettingPath(), true);
        Io.Copy(GetSource("Sample.json"), FeedsPath(), true);
        Io.CreateDirectory(CacheDirectory());

        var n   = 0;

        using (var fw = new System.IO.FileSystemWatcher())
        using (var vm = new MainViewModel(new SettingFolder(RootDirectory(), new()), new()))
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
            Assert.That(vm.Setting.CanExecute(null),     Is.True,     nameof(vm.Setting));
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
        Assert.That(Io.GetFiles(CacheDirectory()).Count(), Is.EqualTo(0), "Cache");
        Assert.That(Io.Exists(dest), Is.True, dest);
    }

    #endregion

    #region Others

    /* --------------------------------------------------------------------- */
    ///
    /// ExportCommand
    ///
    /// <summary>
    /// Represents the mock handler for the Export command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void ExportCommand(SaveFileMessage e, string dest, bool ok)
    {
        e.Value  = dest;
        e.Cancel = !ok;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ImportCommand
    ///
    /// <summary>
    /// Represents the mock handler for the Import command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void ImportCommand(OpenFileMessage e, string src, bool ok)
    {
        e.Value  = new[] { src };
        e.Cancel = !ok;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// DialogMessageCommand
    ///
    /// <summary>
    /// Represents the mock handler for the Dialog command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void DialogMessageCommand(DialogMessage e, DialogStatus status)
    {
        Assert.That(e.Text,  Is.Not.Null.And.Not.Empty);
        Assert.That(e.Title, Is.Not.Null.And.Not.Empty);

        e.Value = status;
    }

    #endregion
}
