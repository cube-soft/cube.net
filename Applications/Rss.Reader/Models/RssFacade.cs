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
                var src = SettingsType.Json.Load<List<RssCategory.Json>>(filename);
                var cvt = src.Select(e => e.Convert());
                Categories = new BindableCollection<RssCategory>(cvt);
                Flatten(Categories);
            }

            _monitor.Uris = _entries.Keys;
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

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 対象とする URL に対応する新着フィード一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssEntry> Entry { get; } = new Bindable<RssEntry>();

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
            Entry.Value = _entries.Values.FirstOrDefault(e => e.Uri == src.Uri);
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
        /// Flatten
        /// 
        /// <summary>
        /// Tree 構造を Flat 化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Flatten(IEnumerable<RssCategory> categories)
        {
            if (categories == null) return;
            foreach (var category in categories)
            {
                foreach (var entry in category.Entries) _entries.Add(entry.Uri, entry);
                Flatten(category.Categories);
            }
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
            if (!_entries.ContainsKey(uri)) return;
            _entries[uri].Feed = feed;
        }

        #region Fields
        private Dictionary<Uri, RssEntry> _entries = new Dictionary<Uri, RssEntry>();
        private RssMonitor _monitor = new RssMonitor();
        #endregion

        #endregion
    }
}
