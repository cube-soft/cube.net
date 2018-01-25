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
    [DataContract]
    public class RssFeed : ObservableProperty
    {
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
        public IList<RssItem> Items
        {
            get => _items = _items ?? new List<RssItem>();
            set => SetProperty(ref _items, value);
        }

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

        #endregion

        #region Fields
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private Uri _link = null;
        private Uri _uri = null;
        private DateTime? _lastChecked = null;
        private DateTime? _lastPublished = null;
        private IList<RssItem> _items = null;
        #endregion
    }
}
