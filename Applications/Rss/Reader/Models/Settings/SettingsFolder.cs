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
using Cube.FileSystem;
using Cube.Log;
using Cube.Settings;
using System;
using System.Diagnostics;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingsFolder
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SettingsFolder : SettingsFolder<LocalSettings>
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
            AutoSave       = true;
            Path           = io.Combine(root, LocalSettings.FileName);
            IO             = io;
            RootDirectory  = root;
            DataDirectory  = root;
            Version.Digit  = 3;
            Version.Suffix = "β";
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
        public LockSettings Lock { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Shared
        ///
        /// <summary>
        /// ユーザ設定オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SharedSettings Shared { get; private set; }

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
        /// LocalSettings.DataDirectory プロパティは GUI によるユーザ操作で
        /// 値が変更される事があります。SettingsFolder の初期化後、または
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
        public string UserAgent => _userAgent ?? (_userAgent = GetUserAgent());

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
        protected override void OnLoaded(ValueChangedEventArgs<LocalSettings> e)
        {
            Debug.Assert(e.NewValue != null);
            var dest = e.NewValue;
            if (string.IsNullOrEmpty(dest.DataDirectory)) dest.DataDirectory = RootDirectory;
            DataDirectory = dest.DataDirectory;

            Lock = LockSettings.Load(DataDirectory, IO);
            Debug.Assert(Lock != null);
            Lock.PropertyChanged += (s, ev) => OnPropertyChanged(ev);

            Shared = SharedSettings.Load(DataDirectory, IO);
            Debug.Assert(Shared != null);
            Shared.LastCheckUpdate = GetLastCheckUpdate();
            Shared.PropertyChanged += (s, ev) => OnPropertyChanged(ev);

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
        protected override void OnSaved(KeyValueEventArgs<SettingsType, string> e)
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
            var name = $@"SOFTWARE\{Company}\{Product}";
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false))
            {
                if (key == null) return default(DateTime?);
                var s = key.GetValue("LastCheckUpdate") as string;
                if (string.IsNullOrEmpty(s)) return default(DateTime?);
                return DateTime.Parse(s).ToLocalTime();
            }
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
            var app  = $"{Product}/{Version.Number}";
            var win  = Environment.OSVersion.VersionString;
            var net  = $".NET {Environment.Version}";
            var view = BrowserSettings.Version;
            return $"{app} ({win}; {net}; {view})";
        }

        #endregion

        #region Fields
        private string _userAgent;
        #endregion
    }
}
