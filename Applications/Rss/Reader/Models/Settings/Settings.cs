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
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Settings() { Reset(); }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// StartUri
        ///
        /// <summary>
        /// 起動時に表示する RssEntry オブジェクトの URL を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember(Name = "Start")]
        public Uri StartUri
        {
            get => _startUri;
            set => SetProperty(ref _startUri, value);
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
        /// Capacity
        ///
        /// <summary>
        /// メモリ上に保持する RSS フィードの最大項目数を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
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
        /// DataDirectory
        ///
        /// <summary>
        /// データ格納用ディレクトリのパスを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string DataDirectory
        {
            get => _dataDirectory;
            set => SetProperty(ref _dataDirectory, value);
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
            get => _initialDelay;
            set => SetProperty(ref _initialDelay, value);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnDeserializing
        ///
        /// <summary>
        /// デシリアライズ直前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => Reset();

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// 値をリセットします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Reset()
        {
            _startUri             = null;
            _width                = 1100;
            _height               = 650;
            _capacity             = 1000;
            _lightMode            = false;
            _enableNewWindow      = false;
            _enableMonitorMessage = true;
            _checkUpdate          = true;
            _dataDirectory        = null;
            _lastCheckUpdate      = null;
            _highInterval         = TimeSpan.FromHours(1);
            _lowInterval          = TimeSpan.FromHours(24);
            _initialDelay         = TimeSpan.FromSeconds(3);
        }

        #endregion

        #region Fields
        private Uri _startUri;
        private int _width;
        private int _height;
        private int _capacity;
        private bool _lightMode;
        private bool _enableNewWindow;
        private bool _enableMonitorMessage;
        private bool _checkUpdate;
        private string _dataDirectory;
        private DateTime? _lastCheckUpdate;
        private TimeSpan? _highInterval;
        private TimeSpan? _lowInterval;
        private TimeSpan? _initialDelay;
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
            AutoSave            = true;
            Path                = io.Combine(root, "Settings.json");
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
        /// UserAgent
        ///
        /// <summary>
        /// ユーザエージェントを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent => _userAgent ?? (_userAgent = GetUserAgent());

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
                var dest = e.NewValue;
                if (string.IsNullOrEmpty(dest.DataDirectory)) dest.DataDirectory = Root;

                var name = $@"SOFTWARE\{Company}\{Product}";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false))
                {
                    if (key == null) return;
                    var s = key.GetValue("LastCheckUpdate") as string;
                    if (string.IsNullOrEmpty(s)) return;
                    dest.LastCheckUpdate = DateTime.Parse(s).ToLocalTime();
                }
            });

            base.OnLoaded(e);
        }

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
