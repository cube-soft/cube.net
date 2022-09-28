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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cube.Net.Rss.Reader;
using Cube.Tests;
using Cube.Xui.Commands.Extensions;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// RegisterViewModelTest
///
/// <summary>
/// Tests the RegisterViewModel class.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class RegisterViewModelTest : ViewModelFixture
{
    #region Tests

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewEntry
    ///
    /// <summary>
    /// Tests the NewEntry command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_NewEntry() => Execute(vm =>
    {
        var host  = new Uri("https://clown.cube-soft.jp/");
        var src   = new Uri(host, "/rss");
        var count = vm.Data.Root.Flatten().Count();

        using var dc = vm.Subscribe<RegisterViewModel>(e => RegisterCommand(e, src).Wait());
        vm.NewEntry.Execute();

        var dest = vm.Data.Root.OfType<RssEntry>().FirstOrDefault(e => e.Uri == src);

        Assert.That(dest.Title, Is.EqualTo("Cube Lilac"));
        Assert.That(dest.Link,  Is.EqualTo(host));
        Assert.That(dest.Count, Is.AtLeast(1));
        Assert.That(vm.Data.Root.Flatten().Count(), Is.GreaterThan(count));
    }, false);

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewEntry_Error
    ///
    /// <summary>
    /// Tests that fail to register new RSS entries.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_NewEntry_Error() => Execute(vm =>
    {
        var count = vm.Data.Root.Flatten().Count();

        using var dc = vm.Subscribe<RegisterViewModel>(e => RegisterError(e).Wait());
        vm.NewEntry.Execute(null);

        Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
    });

    /* --------------------------------------------------------------------- */
    ///
    /// VM_NewEntry_Exists
    ///
    /// <summary>
    /// Tests when an attempt is made to register an already existing RSS
    /// entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void VM_NewEntry_Exists() => Execute(vm =>
    {
        var src = new Uri("https://clown.cube-soft.jp/");
        var count = vm.Data.Root.Flatten().Count();

        using var dc = vm.Subscribe<RegisterViewModel>(e => RegisterCommand(e, src).Wait());
        vm.NewEntry.Execute(null);

        Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
    });

    #endregion

    #region Others

    /* --------------------------------------------------------------------- */
    ///
    /// Setup
    ///
    /// <summary>
    /// Setup some resources.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [SetUp]
    public void Setup()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterCommand
    ///
    /// <summary>
    /// Invokes the Add command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private async Task RegisterCommand(RegisterViewModel vm, Uri src)
    {
        Assert.That(vm.Execute.CanExecute(null), Is.False);
        vm.Url.Value = src.ToString();
        Assert.That(vm.Execute.CanExecute(null), Is.True);

        Assert.That(vm.Busy.Value, Is.False);
        vm.Execute.Execute(null);
        Assert.That(await Wait.ForAsync(() => vm.Busy.Value), Is.True);
        Assert.That(await Wait.ForAsync(() => !vm.Busy.Value), Is.True);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterError
    ///
    /// <summary>
    /// Invokes registration of invalid URLs.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private async Task RegisterError(RegisterViewModel vm)
    {
        var cts = new CancellationTokenSource();

        vm.Url.Value = "error";
        using var dc = vm.Subscribe<DialogMessage>(e =>
        {
            e.Value = DialogStatus.Ok;
            cts.Cancel();
        });
        vm.Execute.Execute(null);

        try { await TaskEx.Delay(1000, cts.Token).ConfigureAwait(false); }
        catch (TaskCanceledException) { /* OK */ }
    }

    #endregion
}
