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
using System.Collections.ObjectModel;
using Cube.Net.Rss;
using Cube.Settings;

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

            Monitor.Interval = TimeSpan.FromHours(1);
            Monitor.Subscribe(WhenReceived);
            Monitor.Start();
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
        public ObservableCollection<RssCategory> Categories { get; }
            = new ObservableCollection<RssCategory>();

        /* ----------------------------------------------------------------- */
        ///
        /// Feeds
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IDictionary<Uri, RssFeed> Feeds { get; }
            = new Dictionary<Uri, RssFeed>();

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// RSS フィード監視用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor Monitor { get; } = new RssMonitor();

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
            foreach (var f in category.Feeds) Monitor.Uris.Add(f.Feed);
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
            if (Feeds.ContainsKey(uri)) Feeds[uri] = feed;
            else Feeds.Add(uri, feed);
        }

        #endregion
    }
}
