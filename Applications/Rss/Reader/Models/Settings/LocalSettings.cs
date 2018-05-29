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
using System.Runtime.Serialization;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// LocalSettings
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class LocalSettings : ObservableProperty
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
        public LocalSettings() { Reset(); }

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
        public static string FileName => "LocalSettings.json";

        /* ----------------------------------------------------------------- */
        ///
        /// FeedFileName
        ///
        /// <summary>
        /// 購読フィード一覧を保持するためのファイルの名前を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string FeedFileName => "Feeds.json";

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectoryName
        ///
        /// <summary>
        /// キャッシュファイルを格納するディレクトリの名前を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string CacheDirectoryName => "Cache";

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
        /// EntryColumn
        ///
        /// <summary>
        /// RSS エントリ一覧部分の幅を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int EntryColumn
        {
            get => _entryColumn;
            set => SetProperty(ref _entryColumn, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ArticleColumn
        ///
        /// <summary>
        /// 新着記事一覧部分の幅を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int ArticleColumn
        {
            get => _articleColumn;
            set => SetProperty(ref _articleColumn, value);
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
            _width         = 1100;
            _height        = 650;
            _entryColumn   = 230;
            _articleColumn = 270;
            _dataDirectory = null;
        }

        #endregion

        #region Fields
        private int _width;
        private int _height;
        private int _entryColumn;
        private int _articleColumn;
        private string _dataDirectory;
        #endregion
    }
}
