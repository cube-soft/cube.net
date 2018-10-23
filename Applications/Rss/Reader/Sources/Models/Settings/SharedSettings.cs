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
using Cube.DataContract;
using Cube.FileSystem;
using Cube.FileSystem.Mixin;
using System;
using System.Runtime.Serialization;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SharedSettings
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class SharedSettings : ObservableProperty
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
        public SharedSettings() { Reset(); }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// 設定を保持するファイルの名前を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string FileName => "Settings.json";

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

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// 設定情報を読み込みます。
        /// </summary>
        ///
        /// <param name="directory">ディレクトリ</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /// <returns>SharedSettings オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static SharedSettings Load(string directory, IO io) => io.LoadOrDefault(
            io.Combine(directory, FileName),
            e => Format.Json.Deserialize<SharedSettings>(e),
            new SharedSettings()
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// 設定情報を保存します。
        /// </summary>
        ///
        /// <param name="directory">ディレクトリ</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Save(string directory, IO io) => io.Save(
            io.Combine(directory, FileName),
            e => Format.Json.Serialize(e, this)
        );

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
            _capacity             = 100;
            _lightMode            = false;
            _enableNewWindow      = false;
            _enableMonitorMessage = true;
            _checkUpdate          = true;
            _lastCheckUpdate      = null;
            _highInterval         = TimeSpan.FromHours(1);
            _lowInterval          = TimeSpan.FromHours(24);
            _initialDelay         = TimeSpan.FromSeconds(3);
        }

        #endregion

        #region Fields
        private Uri _startUri;
        private int _capacity;
        private bool _lightMode;
        private bool _enableNewWindow;
        private bool _enableMonitorMessage;
        private bool _checkUpdate;
        private DateTime? _lastCheckUpdate;
        private TimeSpan? _highInterval;
        private TimeSpan? _lowInterval;
        private TimeSpan? _initialDelay;
        #endregion
    }
}
