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
using System.Diagnostics;
using System.Reflection;
using Cube.FileSystem;
using Cube.FileSystem.DataContract;
using Cube.Forms.Controls;
using Cube.Mixin.Assembly;
using Cube.Mixin.Environment;
using Cube.Mixin.Logging;
using Cube.Mixin.String;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingFolder
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SettingFolder : SettingFolder<LocalSetting>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SettingFolder
        ///
        /// <summary>
        /// Initializes static resources of the SettingFolder class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        static SettingFolder() { Network.Setup(); }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingFolder
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingFolder(Assembly assembly) : this(
            System.IO.Path.Combine(
                Environment.SpecialFolder.LocalApplicationData.GetName(),
                assembly.GetCompany(),
                assembly.GetProduct()
            ),
            new IO()
        ) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingFolder
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="root">設定用フォルダのルートパス</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingFolder(string root, IO io) :
            base(Format.Json, io.Combine(root, LocalSetting.FileName), io)
        {
            AutoSave       = true;
            RootDirectory  = root;
            DataDirectory  = root;
            Version.Suffix = "β";
            Reset(root);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Lock
        ///
        /// <summary>
        /// ロック設定オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LockSetting Lock { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Shared
        ///
        /// <summary>
        /// ユーザ設定オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SharedSetting Shared { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// RootDirectory
        ///
        /// <summary>
        /// 各種ローカル設定を保持するディレクトリのルートとなるパスを
        /// 取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string RootDirectory { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// DataDirectory
        ///
        /// <summary>
        /// 各種ユーザ設定を保持するディレクトリのパスを取得します。
        /// </summary>
        ///
        /// <remarks>
        /// LocalSetting.DataDirectory プロパティは GUI によるユーザ操作で
        /// 値が変更される事があります。SettingFolder の初期化後、または
        /// ローカル設定ファイルの読み込み後はこのプロパティから 取得して
        /// 下さい。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public string DataDirectory { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// ユーザエージェントを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent => _userAgent ??= GetUserAgent();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            Lock.Release(DataDirectory, IO);
            base.Dispose(disposing);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnLoaded
        ///
        /// <summary>
        /// 設定の読み込み時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnLoaded(ValueChangedEventArgs<LocalSetting> e)
        {
            Debug.Assert(e.NewValue != null);
            var min  = 100;
            var dest = e.NewValue;
            if (!dest.DataDirectory.HasValue()) dest.DataDirectory = RootDirectory;
            dest.EntryColumn = Math.Min(dest.EntryColumn, dest.Width - min * 2);
            dest.ArticleColumn = Math.Min(dest.ArticleColumn, dest.Width - dest.EntryColumn - min);
            DataDirectory = dest.DataDirectory;
            Reset(DataDirectory);

            base.OnLoaded(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnSaved
        ///
        /// <summary>
        /// 設定の保存時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnSaved(KeyValueEventArgs<Format, string> e)
        {
            if (!Lock.IsReadOnly) Shared.Save(DataDirectory, IO);
            base.OnSaved(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetLastCheckUpdate
        ///
        /// <summary>
        /// LastCheckUpdate の項目を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private DateTime? GetLastCheckUpdate() => this.LogWarn(() =>
        {
            var name = $@"Software\CubeSoft\CubeRssReader";
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false);
            if (key == null) return default;

            var s = key.GetValue("LastCheckUpdate") as string;
            return s.HasValue() ? DateTime.Parse(s).ToLocalTime() : default;
        });

        /* ----------------------------------------------------------------- */
        ///
        /// GetUserAgent
        ///
        /// <summary>
        /// User-Agent を表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetUserAgent()
        {
            var app  = $"CubeRssReader/{Version.Number}";
            var win  = Environment.OSVersion.VersionString;
            var net  = $".NET {Environment.Version}";
            var view = WebControl.EmulateVersion;
            return $"{app} ({win}; {net}; {view})";
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// Reloads the additional Setting.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Reset(string directory)
        {
            Lock = LockSetting.Load(directory, IO);
            Debug.Assert(Lock != null);
            Lock.PropertyChanged += (s, ev) => OnPropertyChanged(ev);

            Shared = SharedSetting.Load(directory, IO);
            Debug.Assert(Shared != null);
            Shared.LastCheckUpdate = GetLastCheckUpdate();
            Shared.PropertyChanged += (s, ev) => OnPropertyChanged(ev);
        }

        #endregion

        #region Fields
        private string _userAgent;
        #endregion
    }
}
