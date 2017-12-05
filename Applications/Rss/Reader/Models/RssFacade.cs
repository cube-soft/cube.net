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
using Cube.Net.Rss;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFacade
    ///
    /// <summary>
    /// RSS フィードに関連する処理の窓口となるクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssFacade
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssFacade
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="settings">設定用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssFacade(SettingsFolder settings)
        {
            Settings = settings;

            Items.IO = Settings.IO;
            Items.CacheDirectory = Settings.Cache;
            if (IO.Exists(settings.Feed)) Items.Load(settings.Feed);
            Items.CollectionChanged += (s, e) => Items.Save(Settings.Feed);
            Items.SubCollectionChanged += (s, e) => Items.Save(Settings.Feed);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        /// 
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder Settings { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        /// 
        /// <summary>
        /// 入出力用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Operator IO => Settings.IO;

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscribeCollection Items { get; } = new RssSubscribeCollection();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// RSS フィードを選択します。
        /// </summary>
        /// 
        /// <param name="src">選択項目</param>
        /// <param name="prev">直前の RSS フィード情報</param>
        /// 
        /// <returns>選択項目に対応する RSS フィード</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public RssFeed Select(RssEntry src, RssFeed prev)
        {
            if (prev != null) Items.Cache(prev.Uri);
            return Items.Lookup(src.Uri);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        /// 
        /// <summary>
        /// RSS フィード中の記事内容を取得します。
        /// </summary>
        /// 
        /// <param name="src">対象とする記事</param>
        /// 
        /// <returns>表示する文字列</returns>
        /// 
        /// <remarks>
        /// Read メソッドが実行されたタイミングで RssArticle.Read が
        /// true に設定されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public string Read(RssItem src)
        {
            src.Read = true;
            return string.Format(
                Properties.Resources.Skeleton,
                Properties.Resources.SkeletonStyle,
                src.Link,
                src.Link,
                src.Title,
                src.PublishTime,
                !string.IsNullOrEmpty(src.Content) ? src.Content : src.Summary
            );
        }

        #endregion
    }
}
