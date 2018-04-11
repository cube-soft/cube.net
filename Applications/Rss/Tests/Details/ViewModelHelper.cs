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
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// ViewModelHelper
    ///
    /// <summary>
    /// 各種 ViewModel のテストを実行する際の補助クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ViewModelHelper : FileHelper
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// ViewModel オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected MainViewModel Create([CallerMemberName] string name = null)
        {
            Copy(name);

            var settings = new SettingsFolder(RootDirectory(name), IO);
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
        /// ViewModel のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected void Execute(Action<MainViewModel> action, bool stop = true,
            [CallerMemberName] string name = null)
        {
            using (var fw = new System.IO.FileSystemWatcher())
            {
                var l = IO.Combine(RootDirectory(name), LockSettings.FileName);
                var n = 0;

                using (var vm = Create(name))
                {
                    Assert.That(IO.Exists(l), Is.True, l);
                    Assert.That(vm.Data.Lock.HasValue, Is.True, nameof(vm.Data.Lock));
                    Assert.That(vm.Data.Lock.Value.IsReadOnly, Is.False, nameof(vm.Data.Lock));

                    var f = IO.Get(FeedsPath(name));
                    fw.Path = f.DirectoryName;
                    fw.Filter = f.Name;
                    fw.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                    fw.Changed += (s, e) => ++n;
                    fw.EnableRaisingEvents = true;

                    if (stop) vm.Stop.Execute(null);
                    action(vm);
                }

                Assert.That(IO.Exists(l), Is.False, l);
                for (var i = 0; n <= 0 && i < 20; ++i) Task.Delay(50).Wait();
                Assert.That(n, Is.AtLeast(1), "Feeds.json is not changed");
            }
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
        protected Task<bool> Wait(MainViewModel vm, bool empty = false) =>
            Wait(() => string.IsNullOrEmpty(vm.Data.Message.Value) == empty);

        #endregion
    }
}
