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
using System.Runtime.Serialization;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssItem
    ///
    /// <summary>
    /// RSS の項目情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class RssItem : ObservableProperty
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        /// 
        /// <summary>
        /// 記事タイトルを取得または設定します。
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
        /// Summary
        /// 
        /// <summary>
        /// 記事の概要を取得または設定します。
        /// </summary>
        /// 
        /// <remarks>
        /// RSS では description タグ、Atom では summary タグに相当します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        /// 
        /// <summary>
        /// 記事の詳細内容を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// 記事の属するカテゴリ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public IList<string> Categories
        {
            get => _categories = _categories ?? new List<string>();
            set => SetProperty(ref _categories, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        /// 
        /// <summary>
        /// 記事の URL 一覧を取得または設定します。
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
        /// PublishTime
        /// 
        /// <summary>
        /// 記事の発行日時を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public DateTime PublishTime
        {
            get => _publishTime;
            set => SetProperty(ref _publishTime, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        /// 
        /// <summary>
        /// 既読かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool Read
        {
            get => _read;
            set => SetProperty(ref _read, value);
        }

        #endregion

        #region Fields
        private string _title = string.Empty;
        private string _summary = string.Empty;
        private string _content = string.Empty;
        private IList<string> _categories = null;
        private Uri _link = null;
        private DateTime _publishTime = DateTime.MinValue;
        private bool _read = false;
        #endregion
    }
}
