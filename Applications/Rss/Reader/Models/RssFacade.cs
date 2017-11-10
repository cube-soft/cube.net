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
        /* ----------------------------------------------------------------- */
        public RssFacade()
        {
            var json = "Feeds.json";
            if (System.IO.File.Exists(json)) Items.Load(json);

            _monitor.Uris = Items.Uris;
            _monitor.Interval = TimeSpan.FromHours(1);
            _monitor.Subscribe(WhenReceived);
            _monitor.Start();
        }

        #endregion

        #region Properties

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
        /// Add
        /// 
        /// <summary>
        /// RSS フィードを追加します。
        /// </summary>
        /// 
        /// <param name="src">追加する RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(RssFeed src)
        {

        }

        /* ----------------------------------------------------------------- */
        ///
        /// Format
        /// 
        /// <summary>
        /// RSS フィード中の記事内容を整形します。
        /// </summary>
        /// 
        /// <param name="src">対象とする記事</param>
        /// 
        /// <returns>表示する文字列</returns>
        ///
        /* ----------------------------------------------------------------- */
        public string Format(RssArticle src) => string.Format(
            Properties.Resources.Skeleton,
            Properties.Resources.SkeletonStyle,
            src.Link,
            src.Link,
            src.Title,
            src.PublishTime,
            !string.IsNullOrEmpty(src.Content) ? src.Content : src.Summary
        );

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenReceived
        /// 
        /// <summary>
        /// RSS フィードを RssMonitor オブジェクトに登録します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenReceived(Uri uri, RssFeed feed)
        {
            var entry = Items.Lookup(uri);
            if (entry != null) entry.Feed = feed;
        }

        #region Fields
        private RssMonitor _monitor = new RssMonitor();
        #endregion

        #endregion
    }
}
