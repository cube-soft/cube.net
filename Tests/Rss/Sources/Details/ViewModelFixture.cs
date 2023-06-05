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
namespace Cube.Net.Rss.Tests;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cube.FileSystem;
using Cube.Net.Rss.Reader;
using Cube.Reflection.Extensions;
using Cube.Tests;
using Cube.Text.Extensions;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// ViewModelFixture
///
/// <summary>
/// Provides functionality to support tests of ViewModel classes.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class ViewModelFixture : ResourceFixture
{
    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Create
    ///
    /// <summary>
    /// Creates a new instance of the MainViewModel class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected MainViewModel Create([CallerMemberName] string name = null)
    {
        Copy(name);

        var asm  = typeof(ViewModelFixture).Assembly;
        var src  = new SettingFolder(RootDirectory(name), asm.GetSoftwareVersion());
        var dest = new MainViewModel(src, new SynchronizationContext());
        var msg  = dest.Data.Message;

        src.Shared.InitialDelay = TimeSpan.FromMinutes(1);
        src.Value.Width         = 1024;
        src.Value.Height        = 768;

        msg.Value = "Test";
        dest.Setup.Execute(null);
        Assert.That(Wait.For(() => !msg.Value.HasValue()), "Timeout");
        return dest;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Execute
    ///
    /// <summary>
    /// Executes the test with the specified arguments.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected void Execute(Action<MainViewModel> action, bool stop = true,
        [CallerMemberName] string name = null)
    {
        using var fw = new System.IO.FileSystemWatcher();
        var l = Io.Combine(RootDirectory(name), LockSetting.FileName);
        var n = 0;

        using (var vm = Create(name))
        {
            Assert.That(Io.Exists(l), Is.True, l);
            Assert.That(vm.Data.Lock.Value, Is.Not.Null, nameof(vm.Data.Lock));
            Assert.That(vm.Data.Lock.Value.IsReadOnly, Is.False, nameof(vm.Data.Lock));

            var f = new Entity(FeedsPath(name));
            fw.Path = f.DirectoryName;
            fw.Filter = f.Name;
            fw.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            fw.Changed += (s, e) => ++n;
            fw.EnableRaisingEvents = true;

            if (stop) vm.Stop.Execute(null);
            action(vm);
        }

        Assert.That(Io.Exists(l), Is.False, l);
        Assert.That(Wait.For(() => n > 0), "Feeds.json is not changed");
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Setup
    ///
    /// <summary>
    /// Executes in each test.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [SetUp]
    protected void SetUp()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    #endregion
}
