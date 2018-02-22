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
using Cube.Collections;
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
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
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
        public DateTime? PublishTime
        {
            get => _publishTime;
            set => SetProperty(ref _publishTime, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Status
        ///
        /// <summary>
        /// オブジェクトの状態を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssItemStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        ///
        /// <summary>
        /// 記事の属するカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<string> Categories { get; } = new List<string>();

        #endregion

        #region Json

        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public string Summary { get; set; }
            [DataMember] public string Content { get; set; }
            [DataMember] public IList<string> Categories { get; set; }
            [DataMember] public Uri Link { get; set; }
            [DataMember] public DateTime? PublishTime { get; set; }
            [DataMember] public RssItemStatus Status { get; set; }

            public Json(RssItem src)
            {
                Title       = src.Title;
                Summary     = src.Summary;
                Content     = src.Content;
                Link        = src.Link;
                PublishTime = src.PublishTime;
                Status      = src.Status;
                Categories  = new List<string>();
                foreach (var c in src.Categories) Categories.Add(c);
            }

            public RssItem Convert()
            {
                var dest = new RssItem()
                {
                    Title       = Title,
                    Summary     = Summary,
                    Content     = Content,
                    Link        = Link,
                    PublishTime = PublishTime,
                    Status      = Status,
                };

                foreach(var c in Categories.GetOrDefault()) Categories.Add(c);
                return dest;
            }
        }

        #endregion

        #region Fields
        private string _title = string.Empty;
        private string _summary = string.Empty;
        private string _content = string.Empty;
        private Uri _link = null;
        private DateTime? _publishTime = null;
        private RssItemStatus _status = RssItemStatus.Uninitialized;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssItemStatus
    ///
    /// <summary>
    /// RssItem オブジェクトの状態を表す列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum RssItemStatus
    {
        /// <summary>未初期化</summary>
        Uninitialized,
        /// <summary>未読</summary>
        Unread,
        /// <summary>既読</summary>
        Read,
    }
}
