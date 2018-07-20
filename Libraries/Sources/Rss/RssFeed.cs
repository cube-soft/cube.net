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
using Cube.Collections.Mixin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFeed
    ///
    /// <summary>
    /// RSS 情報を保持するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssFeed : ObservableProperty
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssFeed
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed() : this(new List<RssItem>()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssFeed
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="items">Items 用バッファ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed(IList<RssItem> items)
        {
            Items = items;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        ///
        /// <summary>
        /// Web サイトのタイトルを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Description
        ///
        /// <summary>
        /// Web サイトの概要を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        ///
        /// <summary>
        /// Web サイトの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Link
        {
            get => _link;
            set => SetProperty(ref _link, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// フィードの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastChecked
        ///
        /// <summary>
        /// RSS フィードを最後にチェックした日時を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastChecked
        {
            get => _lastChecked;
            set => SetProperty(ref _lastChecked, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastPublished
        ///
        /// <summary>
        /// RSS フィードの最新記事の更新日時を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastPublished
        {
            get => _lastPublished;
            set => SetProperty(ref _lastPublished, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        ///
        /// <summary>
        /// 新着記事一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<RssItem> Items { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// UnreadItems
        ///
        /// <summary>
        /// 未読の新着記事一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssItem> UnreadItems =>
            Items.Where(e => e.Status == RssItemStatus.Unread);

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        ///
        /// <summary>
        /// 操作時に発生したエラーを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Exception Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        #endregion

        #region Json

        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public string Description { get; set; }
            [DataMember] public Uri Uri { get; set; }
            [DataMember] public Uri Link { get; set; }
            [DataMember] public DateTime? LastChecked { get; set; }
            [DataMember] public DateTime? LastPublished { get; set; }
            [DataMember] public IList<RssItem.Json> Items { get; set; }

            public Json(RssFeed src)
            {
                Title         = src.Title;
                Description   = src.Description;
                Uri           = src.Uri;
                Link          = src.Link;
                LastChecked   = src.LastChecked;
                LastPublished = src.LastPublished;
                Items         = new List<RssItem.Json>();
                foreach (var i in src.Items) Items.Add(new RssItem.Json(i));
            }

            public RssFeed Convert()
            {
                var dest = new RssFeed
                {
                    Title         = Title,
                    Description   = Description,
                    Uri           = Uri,
                    Link          = Link,
                    LastChecked   = LastChecked,
                    LastPublished = LastPublished,
                };

                foreach (var i in Items.GetOrDefault()) dest.Items.Add(i.Convert());
                return dest;
            }
        }

        #endregion

        #region Fields
        private string _title = string.Empty;
        private string _description = string.Empty;
        private Uri _link;
        private Uri _uri;
        private DateTime? _lastChecked;
        private DateTime? _lastPublished;
        private Exception _error;
        #endregion
    }
}
