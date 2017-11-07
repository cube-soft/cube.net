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
using System.Collections.Generic;
using System.Linq;
using Cube.Net.Rss;
using Cube.Settings;
using Cube.Xui;

namespace Cube.Net.Applications.Rss.Reader
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
            var filename = "Feeds.json";
            if (System.IO.File.Exists(filename))
            {
                var src = SettingsType.Json.Load<List<RssCategory>>(filename);
                foreach (var item in src)
                {
                    Categories.Add(item);
                    Add(item);
                }
            }

            _monitor.Interval = TimeSpan.FromHours(1);
            _monitor.Subscribe(WhenReceived);
            _monitor.Start();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection<RssCategory> Categories { get; }
            = new BindableCollection<RssCategory>();

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 対象とする URL に対応する新着フィード一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssFeed> Feed { get; } = new Bindable<RssFeed>();

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        /// 
        /// <summary>
        /// 対象とする記事の内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Content { get; } = new Bindable<string>();

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
        /// Select
        /// 
        /// <summary>
        /// RSS フィードを選択します。
        /// </summary>
        /// 
        /// <param name="src">対象とする Web サイト</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Select(RssEntry src)
        {
            Feed.Value = _feeds.FirstOrDefault(e => e.Key == src.Uri).Value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        /// 
        /// <summary>
        /// RSS フィード中の記事を選択します。
        /// </summary>
        /// 
        /// <param name="src">対象とする記事</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Select(RssArticle src)
        {
            Content.Value = string.Format(
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

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// RSS フィードを RssMonitor オブジェクトに登録します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Add(RssCategory category)
        {
            foreach (var f in category.Entries) _monitor.Uris.Add(f.Uri);
            if (category.Categories == null) return;
            foreach (var c in category.Categories) Add(c);
        }

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
            if (_feeds.ContainsKey(uri)) _feeds[uri] = feed;
            else _feeds.Add(uri, feed);
        }

        #region Fields
        private Dictionary<Uri, RssFeed> _feeds = new Dictionary<Uri, RssFeed>();
        private RssMonitor _monitor = new RssMonitor();
        #endregion

        #endregion
    }
}
