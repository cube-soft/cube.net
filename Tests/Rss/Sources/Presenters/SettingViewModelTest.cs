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
using System;
using System.Linq;
using Cube.Mixin.IO;
using Cube.Net.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingViewModelTest
    ///
    /// <summary>
    /// SettingViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class SettingViewModelTest : ViewModelFixture
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// VM_Setting
        ///
        /// <summary>
        /// ユーザ設定を編集するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_Setting() => Execute(vm =>
        {
            var local = vm.Data.Local.Value;
            var shred = vm.Data.Shared.Value;
            vm.Subscribe<SettingViewModel>(e => SettingCommand(e));
            vm.Setting.Execute(null);

            Assert.That(shred.CheckUpdate,          Is.False);
            Assert.That(shred.EnableNewWindow,      Is.False);
            Assert.That(shred.EnableMonitorMessage, Is.False);
            Assert.That(shred.LightMode,            Is.True);
            Assert.That(shred.HighInterval,         Is.EqualTo(TimeSpan.FromHours(2)));
            Assert.That(shred.LowInterval,          Is.EqualTo(TimeSpan.FromHours(12)));
            Assert.That(local.DataDirectory,        Is.EqualTo(Get(nameof(VM_Setting))));
        });

        /* ----------------------------------------------------------------- */
        ///
        /// VM_DataDirectory
        ///
        /// <summary>
        /// データディレクトリを変更するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void VM_DataDirectory()
        {
            var dest = RootDirectory(nameof(ChangeDataFolder));

            Execute(vm =>
            {
                vm.Subscribe<SettingViewModel>(e => ChangeDataFolder(e));
                vm.Setting.Execute(null);
                Assert.That(vm.Data.Local.Value.DataDirectory, Is.EqualTo(dest), nameof(SettingViewModel));
            });

            Assert.That(IO.Exists(dest), Is.False);

            var setting = new SettingFolder(RootDirectory(), IO);
            setting.Load();
            Assert.That(setting.DataDirectory, Is.EqualTo(dest), nameof(SettingFolder));

            var facade = new MainFacade(setting, Dispatcher.Vanilla);
            Assert.That(facade.Data.Root.Flatten().Count(), Is.EqualTo(0));
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// SettingCommand
        ///
        /// <summary>
        /// Setting コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SettingCommand(SettingViewModel vm)
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

            vm.Apply.Execute(null);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ChangeDataFolder
        ///
        /// <summary>
        /// Setting コマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ChangeDataFolder(SettingViewModel vm)
        {
            var dest = string.Empty;

            vm.Subscribe<OpenDirectoryMessage>(e =>
            {
                e.Value  = RootDirectory();
                e.Cancel = false;
                dest     = e.Value;
            });

            vm.Subscribe<ApplyMessage>(
                e => vm.Local.Value.DataDirectory = dest
            );

            vm.SelectDataDirectory.Execute(null);
            vm.Apply.Execute(null);
        }

        #endregion
    }
}
