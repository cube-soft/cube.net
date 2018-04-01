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
    /// SettingsTest
    ///
    /// <summary>
    /// Settings および SettingsFolder のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class SettingsTest : FileHelper
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// 設定ファイルを読み込むテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load()
        {
            var dir = Result(nameof(Load));
            IO.Copy(Example("LocalSettings.json"), IO.Combine(dir, "LocalSettings.json"), true);
            IO.Copy(Example("Settings.json"), IO.Combine(dir, "Settings.json"), true);
            var src = new SettingsFolder(dir, IO);
            src.Load();

            Assert.That(src.Value.Width,                    Is.EqualTo(1100));
            Assert.That(src.Value.Height,                   Is.EqualTo(650));
            Assert.That(src.Value.DataDirectory,            Is.EqualTo(dir));
            Assert.That(src.Shared.StartUri,                Is.EqualTo(new Uri("https://github.com/blog.atom")));
            Assert.That(src.Shared.Capacity,                Is.EqualTo(5));
            Assert.That(src.Shared.EnableNewWindow,         Is.True);
            Assert.That(src.Shared.EnableMonitorMessage,    Is.True);
            Assert.That(src.Shared.LightMode,               Is.False);
            Assert.That(src.Shared.CheckUpdate,             Is.True);
            Assert.That(src.Shared.HighInterval.Value,      Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Shared.LowInterval.Value,       Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Shared.InitialDelay.Value,      Is.EqualTo(TimeSpan.FromSeconds(3)));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load_Empty
        ///
        /// <summary>
        /// 空の設定ファイルを読み込むテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load_Empty()
        {
            var dir = Result(nameof(Load_Empty));
            IO.Copy(Example("SettingsEmpty.json"), IO.Combine(dir, "Settings.json"), true);
            var src = new SettingsFolder(dir, IO);
            src.Load();

            Assert.That(src.Value.Width,                    Is.EqualTo(1100));
            Assert.That(src.Value.Height,                   Is.EqualTo(650));
            Assert.That(src.Value.DataDirectory,            Is.EqualTo(dir));
            Assert.That(src.Shared.StartUri,                Is.Null);
            Assert.That(src.Shared.Capacity,                Is.EqualTo(1000));
            Assert.That(src.Shared.EnableNewWindow,         Is.False);
            Assert.That(src.Shared.EnableMonitorMessage,    Is.True);
            Assert.That(src.Shared.LightMode,               Is.False);
            Assert.That(src.Shared.CheckUpdate,             Is.True);
            Assert.That(src.Shared.HighInterval.Value,      Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Shared.LowInterval.Value,       Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Shared.InitialDelay.Value,      Is.EqualTo(TimeSpan.FromSeconds(3)));
        }

        #endregion
    }
}
