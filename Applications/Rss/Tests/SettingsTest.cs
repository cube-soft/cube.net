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
            IO.Copy(Example("Settings.json"), IO.Combine(dir, "Settings.json"), true);
            var src = new SettingsFolder(dir, IO);
            src.Load();

            Assert.That(src.Value.Width,                    Is.EqualTo(1100));
            Assert.That(src.Value.Height,                   Is.EqualTo(650));
            Assert.That(src.Value.StartUri,                 Is.EqualTo(new Uri("https://github.com/blog.atom")));
            Assert.That(src.Value.DataDirectory,            Is.EqualTo(dir));
            Assert.That(src.Value.Capacity,                 Is.EqualTo(5));
            Assert.That(src.Value.EnableNewWindow,          Is.True);
            Assert.That(src.Value.EnableMonitorMessage,     Is.True);
            Assert.That(src.Value.LightMode,                Is.False);
            Assert.That(src.Value.CheckUpdate,              Is.True);
            Assert.That(src.Value.HighInterval.Value,       Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Value.LowInterval.Value,        Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Value.InitialDelay.Value,       Is.EqualTo(TimeSpan.FromSeconds(3)));
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
            Assert.That(src.Value.StartUri,                 Is.Null);
            Assert.That(src.Value.DataDirectory,            Is.EqualTo(dir));
            Assert.That(src.Value.Capacity,                 Is.EqualTo(1000));
            Assert.That(src.Value.EnableNewWindow,          Is.False);
            Assert.That(src.Value.EnableMonitorMessage,     Is.True);
            Assert.That(src.Value.LightMode,                Is.False);
            Assert.That(src.Value.CheckUpdate,              Is.True);
            Assert.That(src.Value.HighInterval.Value,       Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Value.LowInterval.Value,        Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Value.InitialDelay.Value,       Is.EqualTo(TimeSpan.FromSeconds(3)));
        }

        #endregion
    }
}
