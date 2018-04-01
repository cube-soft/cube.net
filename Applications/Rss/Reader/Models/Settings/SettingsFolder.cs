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
using Cube.FileSystem.Files;
using Cube.Log;
using Cube.Settings;
using System;


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
            AutoSave            = true;
            Path                = io.Combine(root, "LocalSettings.json");
            IO                  = io;
            Root                = root;
            Version.Digit       = 3;
            Version.Suffix      = "β";
            Value.DataDirectory = root;
        }

        #endregion

        #region Properties

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
        /// Root
        ///
        /// <summary>
        /// 各種ユーザ設定を保持するディレクトリのルートディレクトリと
        /// なるパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Root { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Cache
        ///
        /// <summary>
        /// キャッシュ用ディレクトリのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Cache => IO.Combine(Value.DataDirectory, "Cache");

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        ///
        /// <summary>
        /// 購読フィード一覧を保持するためのファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Feed => IO.Combine(Value.DataDirectory, "Feeds.json");

        /* ----------------------------------------------------------------- */
        ///
        /// SharedPath
        ///
        /// <summary>
        /// ユーザ設定を保持するためのファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string SharedPath => IO.Combine(Value.DataDirectory, "Settings.json");

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

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// 読み取り専用モードかどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly { get; set; } = false;

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
            var dest = e.NewValue;
            if (string.IsNullOrEmpty(dest.DataDirectory)) dest.DataDirectory = Root;
            LoadSharedSettings();
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
            IO.Save(SharedPath, ss => Type.Save(ss, Shared));
            base.OnSaved(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadSharedSettings
        ///
        /// <summary>
        /// ユーザ設定を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadSharedSettings()
        {
            var dest = IO.Load(SharedPath, e => Type.Load<SharedSettings>(e), new SharedSettings());
            System.Diagnostics.Debug.Assert(dest != null);
            LoadLastCheckUpdate(dest);
            dest.PropertyChanged += (s, e) => OnPropertyChanged(e);
            Shared = dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadLastCheckUpdate
        ///
        /// <summary>
        /// LastCheckUpdate の項目を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadLastCheckUpdate(SharedSettings dest) => this.LogWarn(() =>
        {
            var name = $@"SOFTWARE\{Company}\{Product}";
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false))
            {
                if (key == null) return;
                var s = key.GetValue("LastCheckUpdate") as string;
                if (string.IsNullOrEmpty(s)) return;
                dest.LastCheckUpdate = DateTime.Parse(s).ToLocalTime();
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
        private string _userAgent = null;
        #endregion
    }
}
