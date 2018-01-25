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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
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
        /* ----------------------------------------------------------------- */
        public RssEntry()
        {
            _dispose = new OnceAction<bool>(Dispose);
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
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry(RssFeed cp) : this()
        {
            Title         = cp.Title;
            Uri           = cp.Uri;
            Link          = cp.Link;
            Description   = cp.Description;
            LastChecked   = cp.LastChecked;
            LastPublished = cp.LastPublished;
            Items         = cp.Items;
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
        public int Count => UnreadItems.Count();

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
            set => throw new NotSupportedException();
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Items is INotifyCollectionChanged cc)
                {
                    cc.CollectionChanged -= WhenItemsChanged;
                }
            }
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
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Items):
                    if (Items is INotifyCollectionChanged cc)
                    {
                        cc.CollectionChanged -= WhenItemsChanged;
                        cc.CollectionChanged += WhenItemsChanged;
                        RaisePropertyChanged(nameof(Count));
                    }
                    break;
                case nameof(Count):
                    if (Parent is RssCategory rc)
                    {
                        rc.RaisePropertyChanged(nameof(Count));
                    }
                    break;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenItemsChanged
        /// 
        /// <summary>
        /// Items.CollectionChanged イベント発生時に実行されるハンドラです。
        /// Items が INotifyCollectionChanged を実装しない場合、この処理は
        /// スキップされます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenItemsChanged(object sender, NotifyCollectionChangedEventArgs e) =>
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
            [DataMember] public string Title { get; set; }
            [DataMember] public Uri Uri { get; set; }
            [DataMember] public RssCheckFrequency Frequency { get; set; }
            [DataMember] public bool SkipContent { get; set; }

            public Json(RssEntry src)
            {
                Title       = src.Title;
                Uri         = src.Uri;
                Frequency   = src.Frequency;
                SkipContent = src.SkipContent;
            }

            public RssEntry Convert(RssCategory src) => new RssEntry
            {
                Title       = Title,
                Uri         = Uri,
                Frequency   = Frequency,
                SkipContent = SkipContent,
                Parent      = src,
            };
        }

        #endregion

        #endregion

        #region Fields
        private OnceAction<bool> _dispose;
        private IRssEntry _parent;
        private RssCheckFrequency _frequency = RssCheckFrequency.Auto;
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
        Auto,
        /// <summary>高頻度</summary>
        High,
        /// <summary>低頻度</summary>
        Low,
        /// <summary>チェックしない</summary>
        None,
    }
}
