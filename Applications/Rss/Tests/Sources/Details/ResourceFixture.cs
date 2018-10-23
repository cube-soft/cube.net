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
using System.Runtime.CompilerServices;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// ResourceFixture
    ///
    /// <summary>
    /// 各種テストファイルを扱う際の補助クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ResourceFixture : FileFixture
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// RootDirectory
        ///
        /// <summary>
        /// 各種データファイルのルートディレクトリを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string RootDirectory([CallerMemberName] string name = null) =>
            GetResultsWith(name);

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectory
        ///
        /// <summary>
        /// キャッシュファイルを格納するディレクトリのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string CacheDirectory([CallerMemberName] string name = null) =>
            IO.Combine(RootDirectory(name), LocalSettings.CacheDirectoryName);

        /* ----------------------------------------------------------------- */
        ///
        /// FeedsPath
        ///
        /// <summary>
        /// フィード情報を保持するファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string FeedsPath([CallerMemberName] string name = null) =>
            IO.Combine(RootDirectory(name), LocalSettings.FeedFileName);

        /* ----------------------------------------------------------------- */
        ///
        /// LocalSettingsPath
        ///
        /// <summary>
        /// ローカル設定情報を保持するファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string LocalSettingsPath([CallerMemberName] string name = null) =>
            IO.Combine(RootDirectory(name), LocalSettings.FileName);

        /* ----------------------------------------------------------------- */
        ///
        /// SharedSettingsPath
        ///
        /// <summary>
        /// 設定情報を保持するファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string SharedSettingsPath([CallerMemberName] string name = null) =>
            IO.Combine(RootDirectory(name), SharedSettings.FileName);

        /* ----------------------------------------------------------------- */
        ///
        /// CachePath
        ///
        /// <summary>
        /// キャッシュファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected string CachePath(string filename, [CallerMemberName] string name = null) =>
            IO.Combine(CacheDirectory(name), filename);

        /* ----------------------------------------------------------------- */
        ///
        /// Copy
        ///
        /// <summary>
        /// 必要なテストファイルをコピーします。
        /// </summary>
        ///
        /// <param name="name">ディレクトリ名</param>
        ///
        /// <returns>コピー先のルートディレクトリ</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected void Copy([CallerMemberName] string name = null)
        {
            IO.Copy(GetExamplesWith("LocalSettings.json"), LocalSettingsPath(name), true);
            IO.Copy(GetExamplesWith("Settings.json"), SharedSettingsPath(name), true);
            IO.Copy(GetExamplesWith("Sample.json"), FeedsPath(name), true);

            foreach (var f in IO.GetFiles(GetExamplesWith("Cache")))
            {
                IO.Copy(f, CachePath(IO.Get(f).Name, name), true);
            }
        }

        #endregion
    }
}
