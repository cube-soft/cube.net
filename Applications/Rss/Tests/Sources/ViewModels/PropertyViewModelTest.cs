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
using Cube.Net.Rss.App.Reader;
using NUnit.Framework;
using System.Linq;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// PropertyViewModelTest
    ///
    /// <summary>
    /// PropertyViewModel のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class PropertyViewModelTest : ViewModelFixture
    {
        #region Tests

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
            vm.Register<PropertyViewModel>(this, e => PropertyCommand(e, src));
            vm.Property.Execute(null);

            Assert.That(dest.Title, Is.EqualTo(nameof(PropertyCommand)));
            Assert.That(dest.Frequency, Is.EqualTo(src));
        });

        #endregion

        #region Others

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
            Assert.That(vm.Entry.Value, Is.Not.Null);
            Assert.That(vm.Frequencies.Count(), Is.EqualTo(4));

            var dest = vm.Entry.Value;
            dest.Title = nameof(PropertyCommand);
            dest.Frequency = value;

            vm.Apply.Execute(null);
        }

        #endregion
    }
}
