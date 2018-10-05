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
using Cube.Xui;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssCategory
    ///
    /// <summary>
    /// RSS エントリが属するカテゴリ情報を保持するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssCategory : ObservableProperty, IRssEntry
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssCategory
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="context">同期用コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssCategory(SynchronizationContext context)
        {
            _dispose = new OnceAction<bool>(Dispose);
            Context  = context;
            Children = new BindableCollection<IRssEntry> { Context = context };
            Children.CollectionChanged += WhenChildrenChanged;
        }

        #endregion

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
            get
            {
                if (!_count.HasValue)
                {
                    _count = Children.Aggregate(0, (x, i) => x + i.Count);
                }
                return _count.Value;
            }
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
        public BindableCollection<IRssEntry> Children { get; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ~RssCategory
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~RssCategory() { _dispose.Invoke(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            _dispose.Invoke(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージオブジェクトを開放するかどうか
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Children.CollectionChanged -= WhenChildrenChanged;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnPropertyChanged
        ///
        /// <summary>
        /// プロパティ変更時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Count))
            {
                _count = null;
                if (Parent is RssCategory rc) rc.RaisePropertyChanged(nameof(Count));
            }
            base.OnPropertyChanged(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChildrenChanged
        ///
        /// <summary>
        /// 子要素の変更時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenChildrenChanged(object s, NotifyCollectionChangedEventArgs e) =>
            RaisePropertyChanged(nameof(Count));

        #region Json

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

            public  RssCategory Convert(SynchronizationContext context) => Convert(null, context);
            public  RssCategory Convert(RssCategory src) => Convert(src, src.Context);
            private RssCategory Convert(RssCategory src, SynchronizationContext context)
            {
                var dest = new RssCategory(context)
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
                System.Diagnostics.Debug.Assert(dest != null);
                foreach (var item in items.GetOrDefault()) dest.Children.Add(item);
            }
        }

        #endregion

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private string _title;
        private int? _count;
        private IRssEntry _parent;
        private bool _expanded = false;
        private bool _editing = false;
        #endregion
    }
}
