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
using System.Runtime.Serialization;

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
    public class Settings : ObservableProperty
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 起動時に表示する RssEntry オブジェクトの URL を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public Uri Start
        {
            get => _feedUri;
            set => SetProperty(ref _feedUri, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Width
        ///
        /// <summary>
        /// メイン画面の幅を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Height
        ///
        /// <summary>
        /// メイン画面の高さを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LightMode
        ///
        /// <summary>
        /// 軽量モードを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool LightMode
        {
            get => _lightMode;
            set => SetProperty(ref _lightMode, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableNewWindow
        ///
        /// <summary>
        /// 新しいウィンドウで開くを有効にするかどうかを示す値を取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool EnableNewWindow
        {
            get => _enableNewWindow;
            set => SetProperty(ref _enableNewWindow, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableMonitorMessage
        ///
        /// <summary>
        /// RSS フィードの取得状況をステータスバーに表示するかどうかを
        /// 示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool EnableMonitorMessage
        {
            get => _enableMonitorMessage;
            set => SetProperty(ref _enableMonitorMessage, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CheckUpdate
        ///
        /// <summary>
        /// ソフトウェアのアップデートを確認するかどうかを示す値を取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool CheckUpdate
        {
            get => _checkUpdate;
            set => SetProperty(ref _checkUpdate, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HighInterval
        ///
        /// <summary>
        /// 高頻度モニタのチェック間隔を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public TimeSpan? HighInterval
        {
            get => _highInterval;
            set => SetProperty(ref _highInterval, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LowInterval
        ///
        /// <summary>
        /// 低頻度モニタのチェック間隔を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public TimeSpan? LowInterval
        {
            get => _lowInterval;
            set => SetProperty(ref _lowInterval, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastCheckUpdate
        ///
        /// <summary>
        /// ソフトウェアの最終アップデート確認日時を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastCheckUpdate
        {
            get => _lastCheckUpdate;
            set => SetProperty(ref _lastCheckUpdate, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InitialDelay
        ///
        /// <summary>
        /// モニタの初期遅延時間を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan? InitialDelay
        {
            get => _initialDelay ?? (_initialDelay = TimeSpan.FromSeconds(3));
            set => SetProperty(ref _initialDelay, value);
        }

        #endregion

        #region Fields
        private Uri _feedUri;
        private int _width = 1100;
        private int _height = 650;
        private bool _lightMode = false;
        private bool _enableNewWindow = false;
        private bool _enableMonitorMessage = true;
        private bool _checkUpdate = true;
        private DateTime? _lastCheckUpdate = null;
        private TimeSpan? _highInterval = TimeSpan.FromHours(1);
        private TimeSpan? _lowInterval = TimeSpan.FromHours(24);
        private TimeSpan? _initialDelay = null;
        #endregion
    }

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
            AutoSave       = true;
            Path           = io.Combine(root, "Settings.json");
            IO             = io;
            Root           = root;
            Version.Digit  = 3;
            Version.Suffix = "β";
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
        public string Cache => IO.Combine(Root, "Cache");

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        ///
        /// <summary>
        /// 購読フィード一覧を保持するためのファイルのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Feed => IO.Combine(Root, "Feeds.json");

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// ユーザエージェントを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent
        {
            get
            {
                var app = $"{Product}/{Version.Number}";
                var win = Environment.OSVersion.VersionString;
                var net = $".NET {Environment.Version}";
                return $"{app} ({win}; {net})";
            }
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
        protected override void OnLoaded(ValueChangedEventArgs<Settings> e)
        {
            this.LogWarn(() =>
            {
                var name = $@"SOFTWARE\{Company}\{Product}";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false))
                {
                    if (key == null) return;
                    var dest = key.GetValue("LastCheckUpdate") as string;
                    if (string.IsNullOrEmpty(dest)) return;
                    e.NewValue.LastCheckUpdate = DateTime.Parse(dest).ToLocalTime();
                }
            });

            base.OnLoaded(e);
        }

        #endregion
    }
}
