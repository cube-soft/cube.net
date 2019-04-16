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
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// BrowserSettingsTest
    ///
    /// <summary>
    /// BrowserSettings のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class BrowserSettingsTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// ブラウザのバージョンを設定するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(BrowserVersion.IE7,       ExpectedResult = BrowserVersion.IE7)]
        [TestCase(BrowserVersion.IE8Quirks, ExpectedResult = BrowserVersion.IE8Quirks)]
        [TestCase(BrowserVersion.IE8,       ExpectedResult = BrowserVersion.IE8)]
        [TestCase(BrowserVersion.Latest,    ExpectedResult = BrowserVersion.IE11)]
        public BrowserVersion Version(BrowserVersion src)
        {
            BrowserSettings.Version = src;
            return BrowserSettings.Version;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GpuRendering
        ///
        /// <summary>
        /// GPU レンダリングの有効・無効を設定するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true)]
        [TestCase(false)]
        public void GpuRendering(bool src)
        {
            BrowserSettings.GpuRendering = src;
            Assert.That(BrowserSettings.GpuRendering, Is.EqualTo(src));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NavigationSounds
        ///
        /// <summary>
        /// クリック音の有効・無効を設定するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true)]
        [TestCase(false)]
        public void NavigationSounds(bool src)
        {
            BrowserSettings.NavigationSounds = src;
            Assert.That(BrowserSettings.NavigationSounds, Is.EqualTo(src));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MaxConnections
        ///
        /// <summary>
        /// 単一アプリケーションあたりの最大コネクション数を設定する
        /// テストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(0,   ExpectedResult = 6)]
        [TestCase(1,   ExpectedResult = 6)]
        [TestCase(2,   ExpectedResult = 2)]
        [TestCase(128, ExpectedResult = 128)]
        [TestCase(129, ExpectedResult = 6)]
        public int MaxConnections(int src)
        {
            BrowserSettings.MaxConnections = 6;
            BrowserSettings.MaxConnections = src;
            return BrowserSettings.MaxConnections;
        }
    }
}
