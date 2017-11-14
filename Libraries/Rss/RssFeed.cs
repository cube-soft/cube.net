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
    public class RssFeed
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
        public string Id { get; set; }

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
        public string Title { get; set; }

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
        public string Description { get; set; }

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
        public IEnumerable<Uri> Links { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        /// 
        /// <summary>
        /// Web サイトの URL 一覧の先頭要素を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Link => Links?.FirstOrDefault();

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
        public Uri Icon { get; set; }

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
        public DateTime LastChecked { get; set; }

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
        public IEnumerable<RssArticle> Items { get; set; }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssArticle
    ///
    /// <summary>
    /// RSS の項目情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class RssArticle
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Id
        /// 
        /// <summary>
        /// 記事 ID を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Id { get; set; }

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
        public string Title { get; set; }

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
        public string Summary { get; set; }

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
        public string Content { get; set; }

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
        public IEnumerable<Uri> Links { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        /// 
        /// <summary>
        /// 記事で最初に出現した URL を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Link => Links?.FirstOrDefault();

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
        public DateTime PublishTime { get; set; }

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
        public IEnumerable<string> Categories { get; set; }

        #endregion
    }
}
