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
using System.Runtime.Serialization;
using Cube.FileSystem;
using Cube.Settings;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// Settings
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class Settings : ObservableProperty { }

    /* --------------------------------------------------------------------- */
    ///
    /// SettingsFolder
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SettingsFolder : SettingsFolder<Settings>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder() : this(System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AssemblyReader.Default.Company,
            AssemblyReader.Default.Product
        ), new Operator()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="root">設定用フォルダのルートパス</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(string root, Operator io) : base(SettingsType.Json)
        {
            AutoSave = false;
            Path = System.IO.Path.Combine(root, "Settings.json");
            IO = io;
            RootDirectory = root;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        /// 
        /// <summary>
        /// 入出力用オブジェクトを取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public Operator IO { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// RootDirectory
        /// 
        /// <summary>
        /// 各種ユーザ設定を保持するディレクトリのルートディレクトリと
        /// なるパスを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string RootDirectory { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectory
        /// 
        /// <summary>
        /// キャッシュ用ディレクトリのパスを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string CacheDirectory => IO.Combine(RootDirectory, "Cache");

        /* ----------------------------------------------------------------- */
        ///
        /// FeedPath
        /// 
        /// <summary>
        /// 購読フィード一覧を保持するためのファイルのパスを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string FeedPath => IO.Combine(RootDirectory, "Feeds.json");

        #endregion
    }
}
