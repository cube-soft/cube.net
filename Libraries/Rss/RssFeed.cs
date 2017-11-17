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
        /// Id
        /// 
        /// <summary>
        /// Web サイトの ID を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

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
        /// Links
        /// 
        /// <summary>
        /// Web サイトの URL 一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public IList<Uri> Links
        {
            get => _links = _links ?? new List<Uri>();
            set => SetProperty(ref _links, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        /// 
        /// <summary>
        /// Web サイトの URL 一覧の先頭要素を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Link => Links.FirstOrDefault();

        /* ----------------------------------------------------------------- */
        ///
        /// Icon
        /// 
        /// <summary>
        /// Web サイトのアイコンを示す URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public Uri Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
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
        public DateTime LastChecked
        {
            get => _lastChecked;
            set => SetProperty(ref _lastChecked, value);
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
        public IList<RssArticle> Items
        {
            get => _items = _items ?? new List<RssArticle>();
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
        public IEnumerable<RssArticle> UnreadItems => Items.Where(e => !e.Read);

        #endregion

        #region Fields
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private IList<Uri> _links = null;
        private Uri _icon = null;
        private DateTime _lastChecked = DateTime.MinValue;
        private IList<RssArticle> _items = null;
        #endregion
    }
}
