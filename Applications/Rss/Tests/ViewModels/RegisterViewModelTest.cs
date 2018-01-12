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
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests
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
    class RegisterViewModelTest : FileHelper
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        /// 
        /// <summary>
        /// 新規 URL を登録するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Register()
        {
            var vm = new MainViewModel(new SettingsFolder(Results, IO));
            vm.Messenger.Register<RegisterViewModel>(this, e => RegisterCommand(e).Wait());
            vm.Register.Execute(null);

            var entry = vm.Model.Items.OfType<RssEntry>().FirstOrDefault(e => e.Uri == _uri);
            Assert.That(entry, Is.Not.Null);
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
        private async Task RegisterCommand(RegisterViewModel vm)
        {
            Assert.That(vm.Execute.CanExecute(null), Is.False);
            vm.Url.Value = _uri.ToString();
            Assert.That(vm.Execute.CanExecute(null), Is.True);

            try
            {
                var cts = new CancellationTokenSource();
                vm.Messenger.Register<RegisterViewModel>(this, _ => cts.Cancel());
                vm.Execute.Execute(null);
                await Task.Delay(5000, cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException /* err */) { /* OK */ }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// OneTimeSetup
        /// 
        /// <summary>
        /// 一度だけ初期化されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var json = "Feeds.json";
            var src  = Example(json);
            var dest = IO.Combine(Results, json);

            IO.Copy(src, dest, true);
        }

        #endregion

        #region Fields
        private Uri _uri = new Uri("http://clown.hatenablog.jp/feed");
        #endregion
    }
}
