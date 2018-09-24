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
using System.Threading;

namespace Cube.Net.Rss.App.Reader
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
    public class RssEntry : RssFeed, IRssEntry
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="context">同期用コンテキスト</param>
        ///
        /// <remarks>
        /// 将来的に Items に対して ObservableCollection(T) を指定する
        /// 必要がある。その場合は base を BindableCollection(T) で
        /// 初期化する。現在は SafeItems を利用しているため必要として
        /// いない模様（SafeItems の項目も参照）。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry(SynchronizationContext context)
        {
            _dispose = new OnceAction<bool>(Dispose);
            Context  = context;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="cp">コピー元オブジェクト</param>
        /// <param name="context">同期用コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry(RssFeed cp, SynchronizationContext context) : this(context)
        {
            Title         = cp.Title;
            Uri           = cp.Uri;
            Link          = cp.Link;
            Count         = cp.UnreadItems.Count();
            Description   = cp.Description;
            LastChecked   = cp.LastChecked;
            LastPublished = cp.LastPublished;

            foreach (var i in cp.Items.GetOrDefault()) Items.Add(i);
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
        /// Frequency
        ///
        /// <summary>
        /// RSS フィードの確認頻度を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCheckFrequency Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
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
            set
            {
                if (SetProperty(ref _count, value) && Parent is RssCategory rc)
                {
                    rc.RaisePropertyChanged(nameof(Count));
                }
            }
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
        /// 子要素が表示状態かどうかを示す値を取得します。このプロパティは
        /// 常に false を返します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Expanded
        {
            get => false;
            set { }
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
        /// SkipContent
        ///
        /// <summary>
        /// Content の表示をスキップし、直接 Web ページを表示するかどうか
        /// を示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool SkipContent
        {
            get => _skipContent;
            set => SetProperty(ref _skipContent, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SafeItems
        ///
        /// <summary>
        /// 新着記事一覧を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// Items を BindableCollection(T) で初期化し、直接 Binding すると
        /// ListView の ItemsControl に対して InvalidOperationException が
        /// 発生する場合がある。該当の現象が解消でき次第、このプロパティは
        /// 削除する。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssItem> SafeItems => Items.Where(e => true);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ~RssEntry
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~RssEntry() { _dispose.Invoke(false); }

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
        protected virtual void Dispose(bool disposing) { }

        #endregion

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
            [DataMember] public string Title { get; set; }
            [DataMember] public Uri Uri { get; set; }
            [DataMember] public Uri Link { get; set; }
            [DataMember] public int Count { get; set; }
            [DataMember] public RssCheckFrequency Frequency { get; set; }
            [DataMember] public bool SkipContent { get; set; }

            public Json(RssEntry src)
            {
                Title       = src.Title;
                Uri         = src.Uri;
                Link        = src.Link;
                Count       = src.Count;
                Frequency   = src.Frequency;
                SkipContent = src.SkipContent;
            }

            public RssEntry Convert(RssCategory src) => new RssEntry(src.Context)
            {
                Title       = Title,
                Uri         = Uri,
                Link        = Link,
                Count       = Count,
                Frequency   = Frequency,
                SkipContent = SkipContent,
                Parent      = src,
            };
        }

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private IRssEntry _parent;
        private RssCheckFrequency _frequency = RssCheckFrequency.Auto;
        private int _count = 0;
        private bool _selected = false;
        private bool _editing = false;
        private bool _skipContent = false;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssCheckFrequency
    ///
    /// <summary>
    /// RSS フィードのチェック頻度を表した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum RssCheckFrequency
    {
        /// <summary>自動</summary>
        Auto = 0,
        /// <summary>高頻度</summary>
        High = 1,
        /// <summary>低頻度</summary>
        Low = 2,
        /// <summary>チェックしない</summary>
        None = -1,
    }
}
