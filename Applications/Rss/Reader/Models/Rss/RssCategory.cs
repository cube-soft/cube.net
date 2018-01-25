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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssCategory
    ///
    /// <summary>
    /// 登録サイトが属するカテゴリ情報を保持するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssCategory : ObservableProperty, IRssEntry
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Parent
        /// 
        /// <summary>
        /// 親要素を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IRssEntry Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        /// 
        /// <summary>
        /// タイトルを取得または設定します。
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
        /// Count
        /// 
        /// <summary>
        /// 未読記事数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Selected
        /// 
        /// <summary>
        /// 選択状態かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Expanded
        /// 
        /// <summary>
        /// 子要素が表示状態かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Expanded
        {
            get => _expanded;
            set => SetProperty(ref _expanded, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Editing
        /// 
        /// <summary>
        /// ユーザによる編集中かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Editing
        {
            get => _editing;
            set => SetProperty(ref _editing, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// サブカテゴリ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssCategory> Categories => Children.OfType<RssCategory>();

        /* ----------------------------------------------------------------- */
        ///
        /// Entries
        /// 
        /// <summary>
        /// エントリ一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssEntry> Entries => Children.OfType<RssEntry>();

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        /// 
        /// <summary>
        /// 子要素一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection<IRssEntry> Children { get; set; }
            = new BindableCollection<IRssEntry>();

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
            [DataMember] string Title { get; set; }
            [DataMember] IEnumerable<Json> Categories { get; set; }
            [DataMember] IEnumerable<RssEntry.Json> Entries { get; set; }

            public Json(RssCategory src)
            {
                Title      = src.Title;
                Entries    = src.Entries?.Select(e => new RssEntry.Json(e));
                Categories = src.Categories?.Select(e => new Json(e));
            }

            public RssCategory Convert(RssCategory src)
            {
                var dest = new RssCategory
                {
                    Title  = Title,
                    Parent = src,
                };
                Add(dest, Categories?.Select(e => e.Convert(dest)));
                Add(dest, Entries.Select(e => e.Convert(dest)));
                return dest;
            }

            private void Add(RssCategory dest, IEnumerable<IRssEntry> items)
            {
                if (dest == null || items == null) return;
                foreach (var item in items) dest.Children.Add(item);
            }
        }

        #endregion

        #region Fields
        private string _title;
        private int _count;
        private IRssEntry _parent;
        private bool _selected = false;
        private bool _expanded = false;
        private bool _editing = false;
        #endregion
    }
}
