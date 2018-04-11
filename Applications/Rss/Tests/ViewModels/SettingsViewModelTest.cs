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

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingsViewModelTest
    ///
    /// <summary>
    /// SettingsViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class SettingsViewModelTest : ViewModelHelper
    {
        #region Tests

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

        #endregion
    }
}
