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
using Cube.Net.Rss.App.Reader;
using Cube.Xui;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RegisterViewModelTest
    ///
    /// <summary>
    /// RegisterViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RegisterViewModelTest : ViewModelFixture
    {
        #region Tests

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
            var host  = new Uri("https://clown.hatenablog.jp/");
            var src   = new Uri(host, "/feed");
            var count = vm.Data.Root.Flatten().Count();

            vm.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
            vm.NewEntry.Execute(null);

            var dest = vm.Data.Root.OfType<RssEntry>().FirstOrDefault(e => e.Uri == src);

            Assert.That(dest.Title, Is.EqualTo("Life like a clown"));
            Assert.That(dest.Link,  Is.EqualTo(host));
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

            vm.Register<RegisterViewModel>(this, e => RegisterError(e).Wait());
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

            vm.Register<RegisterViewModel>(this, e => RegisterCommand(e, src).Wait());
            vm.NewEntry.Execute(null);

            Assert.That(vm.Data.Root.Flatten().Count(), Is.EqualTo(count));
        });

        #endregion

        #region Others

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
            Assert.That(await Wait.ForAsync(() => vm.Busy.Value), Is.True);
            Assert.That(await Wait.ForAsync(() => !vm.Busy.Value), Is.True);
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
            vm.Register<DialogMessage>(this, e =>
            {
                e.Result = MessageBoxResult.OK;
                cts.Cancel();
            });
            vm.Execute.Execute(null);

            try { await Task.Delay(1000, cts.Token).ConfigureAwait(false); }
            catch (TaskCanceledException) { /* OK */ }
        }

        #endregion
    }
}
