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
using System.Reflection;
using Cube.Net.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingTest
    ///
    /// <summary>
    /// Setting および SettingFolder のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class SettingTest : ResourceFixture
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
            Copy();
            var src = new SettingFolder(RootDirectory(), IO);

            Assert.That(src.TryLoad(),                   Is.True);
            Assert.That(src.Value.Width,                 Is.EqualTo(1100));
            Assert.That(src.Value.Height,                Is.EqualTo(650));
            Assert.That(src.Value.DataDirectory,         Is.EqualTo(RootDirectory()));
            Assert.That(src.Shared.StartUri,             Is.EqualTo(new Uri("https://github.com/blog.atom")));
            Assert.That(src.Shared.Capacity,             Is.EqualTo(5));
            Assert.That(src.Shared.EnableNewWindow,      Is.True);
            Assert.That(src.Shared.EnableMonitorMessage, Is.True);
            Assert.That(src.Shared.LightMode,            Is.False);
            Assert.That(src.Shared.CheckUpdate,          Is.True);
            Assert.That(src.Shared.HighInterval,         Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Shared.LowInterval,          Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Shared.InitialDelay,         Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(src.Lock.IsReadOnly,             Is.False);
            Assert.That(src.Lock.IsReadOnly,             Is.False);
            Assert.That(src.Lock.UserName,               Is.EqualTo(Environment.UserName));
            Assert.That(src.Lock.MachineName,            Is.EqualTo(Environment.MachineName));
            Assert.That(src.Lock.Sid,                    Does.StartWith("S-1-5-21"));
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
            IO.Copy(GetSource("SettingsEmpty.json"), LocalSettingPath(),  true);
            IO.Copy(GetSource("SettingsEmpty.json"), SharedSettingPath(), true);
            var src = new SettingFolder(RootDirectory(), IO);

            Assert.That(src.TryLoad(),                   Is.True);
            Assert.That(src.Value.Width,                 Is.EqualTo(1100));
            Assert.That(src.Value.Height,                Is.EqualTo(650));
            Assert.That(src.Value.DataDirectory,         Is.EqualTo(RootDirectory()));
            Assert.That(src.Shared.StartUri,             Is.Null);
            Assert.That(src.Shared.Capacity,             Is.EqualTo(100));
            Assert.That(src.Shared.EnableNewWindow,      Is.False);
            Assert.That(src.Shared.EnableMonitorMessage, Is.True);
            Assert.That(src.Shared.LightMode,            Is.False);
            Assert.That(src.Shared.CheckUpdate,          Is.True);
            Assert.That(src.Shared.HighInterval,         Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(src.Shared.LowInterval,          Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(src.Shared.InitialDelay,         Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(src.Lock.IsReadOnly,             Is.False);
            Assert.That(src.Lock.IsReadOnly,             Is.False);
            Assert.That(src.Lock.UserName,               Is.EqualTo(Environment.UserName));
            Assert.That(src.Lock.MachineName,            Is.EqualTo(Environment.MachineName));
            Assert.That(src.Lock.Sid,                    Does.StartWith("S-1-5-21"));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load_NotFound
        ///
        /// <summary>
        /// 設定ファイルが存在しない場合の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load_NotFound()
        {
            var src = new SettingFolder(RootDirectory(), IO);

            Assert.That(src.TryLoad(),     Is.False);
            Assert.That(src.DataDirectory, Is.EqualTo(RootDirectory()));
            Assert.That(src.Value,         Is.Not.Null, nameof(src.Value));
            Assert.That(src.Shared,        Is.Not.Null, nameof(src.Shared));
            Assert.That(src.Lock,          Is.Not.Null, nameof(src.Lock));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load_Invalid
        ///
        /// <summary>
        /// 設定ファイルが破損している場合の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Load_Invalid()
        {
            IO.Copy(GetSource("Dummy.txt"), LocalSettingPath(),  true);
            IO.Copy(GetSource("Dummy.txt"), SharedSettingPath(), true);
            var src = new SettingFolder(RootDirectory(), IO);

            Assert.That(src.TryLoad(),     Is.False);
            Assert.That(src.DataDirectory, Is.EqualTo(RootDirectory()));
            Assert.That(src.Value,         Is.Not.Null, nameof(src.Value));
            Assert.That(src.Shared,        Is.Not.Null, nameof(src.Shared));
            Assert.That(src.Lock,          Is.Not.Null, nameof(src.Lock));
        }

        #endregion
    }
}
