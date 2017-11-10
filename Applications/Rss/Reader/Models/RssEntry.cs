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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using Cube.Net.Rss;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntry
    ///
    /// <summary>
    /// RSS フィードを購読する Web サイトの情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssEntry
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
        public string Title { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        /// 
        /// <summary>
        /// RSS フィードを取得するための URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Parent
        /// 
        /// <summary>
        /// RSS エントリが属するカテゴリを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCategory Parent { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// RSS フィードを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed Feed { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry.Json
        /// 
        /// <summary>
        /// JSON 解析用クラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public Uri Uri { get; set; }
            public RssEntry Convert(RssCategory src) => new RssEntry
            {
                Title  = Title,
                Uri    = Uri,
                Parent = src,
            };
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssCategory
    ///
    /// <summary>
    /// 登録サイトが属するカテゴリ情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssCategory
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Default
        /// 
        /// <summary>
        /// デフォルトに指定されているカテゴリかどうかを表す値を取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Default { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        /// 
        /// <summary>
        /// カテゴリのタイトルを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Title { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// サブカテゴリ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssCategory> Categories { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Entries
        /// 
        /// <summary>
        /// 登録した Web サイト一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssEntry> Entries { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        /// 
        /// <summary>
        /// 表示項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable Items => new CompositeCollection
        {
            new CollectionContainer { Collection = Categories },
            new CollectionContainer { Collection = Entries },
        };

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry.Json
        /// 
        /// <summary>
        /// JSON 解析用クラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        internal class Json
        {
            [DataMember] bool Default { get; set; }
            [DataMember] string Title { get; set; }
            [DataMember] List<Json> Categories { get; set; }
            [DataMember] List<RssEntry.Json> Entries { get; set; }
            public RssCategory Convert()
            {
                var dest = new RssCategory
                {
                    Default    = Default,
                    Title      = Title,
                    Categories = Categories?.Select(e => e.Convert()),
                };
                dest.Entries = Entries.Select(e => e.Convert(dest));
                return dest;
            }
        }

        #endregion
    }
}
