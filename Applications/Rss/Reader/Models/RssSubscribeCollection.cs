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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Cube.Net.Rss;
using Cube.Settings;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssSubscribeCollection
    ///
    /// <summary>
    /// 購読フィード一覧を管理するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public sealed class RssSubscribeCollection : IEnumerable, INotifyCollectionChanged
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssSubscribeCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscribeCollection()
        {
            _items.CollectionChanged += WhenCollectionChanged;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Default
        /// 
        /// <summary>
        /// 未整理用のエントリを格納するカテゴリを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCategory Default => _items.First(e => string.IsNullOrEmpty(e.Title));

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// カテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssCategory> Categories => _items;

        /* ----------------------------------------------------------------- */
        ///
        /// Uris
        /// 
        /// <summary>
        /// 購読しているフィードの URL 一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<Uri> Uris => _feeds.Keys;

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// CollectionChanged
        /// 
        /// <summary>
        /// コレクション変更時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        /// 
        /// <summary>
        /// 新しい RSS フィードを追加します。
        /// </summary>
        /// 
        /// <param name="feed">RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(RssFeed feed)
        {
            Default.Entries.Add(new RssEntry
            {
                Title = feed.Title,
                Uri   = feed.Link,
            });
            _feeds.Add(feed.Link, feed);
            RaiseResetAction();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        /// 
        /// <summary>
        /// コレクションの内容をクリアします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Clear()
        {
            if (_items.Count > 0)
            {
                _items.Clear();
                _feeds.Clear();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// JSON ファイルを読み込みます。
        /// </summary>
        /// 
        /// <param name="json">JSON ファイルのパス</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Load(string json) => Suppress(() =>
        {
            Clear();
            var src = SettingsType.Json.Load<List<RssCategory.Json>>(json);
            foreach (var item in src.Select(e => e.Convert()))
            {
                MakeFeed(item);
                _items.Add(item);
            }
            RaiseResetAction();
        });

        /* ----------------------------------------------------------------- */
        ///
        /// Lookup
        /// 
        /// <summary>
        /// URL に対応する RSS フィードを取得します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed Lookup(Uri uri)
            => _feeds.ContainsKey(uri) ? _feeds[uri] : null;

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        /// 
        /// <summary>
        /// 反復用オブジェクトを取得します。
        /// </summary>
        /// 
        /// <returns>反復用オブジェクト</returns>
        /// 
        /// <remarks>
        /// GetEnumerator() メソッドは無名カテゴリの場合、カテゴリ中の
        /// エントリーを直接返します。全てのカテゴリを取得する場合は
        /// Categories プロパティを利用して下さい。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerator GetEnumerator()
        {
            foreach (var category in _items)
            {
                if (!string.IsNullOrEmpty(category.Title)) yield return category;
                else if (category.Entries != null)
                {
                    foreach (var entry in category.Entries) yield return entry;
                }
            }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Suppress
        /// 
        /// <summary>
        /// CollectionChanged イベントを抑制した状態で処理を実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Suppress(Action action)
        {
            try
            {
                _items.CollectionChanged -= WhenCollectionChanged;
                action();
            }
            finally { _items.CollectionChanged += WhenCollectionChanged; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MakeFeed
        /// 
        /// <summary>
        /// RSS フィード用のオブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MakeFeed(RssCategory src)
        {
            foreach (var entry in src.Entries)
            {
                if (_feeds.ContainsKey(entry.Uri)) continue;

                _feeds.Add(entry.Uri, new RssFeed
                {
                    Title = entry.Title,
                    Link  = entry.Uri,
                    Items = new RssArticle[0],
                });
            }

            if (src.Categories == null) return;
            foreach (var category in src.Categories) MakeFeed(category);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseResetAction
        /// 
        /// <summary>
        /// NotifyCollectionChangedAction.Reset を指定されたイベントを
        /// 発生させます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void RaiseResetAction()
            => CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// WhenCollectionChanged
        /// 
        /// <summary>
        /// CollectionChanged イベントを発生時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void WhenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_context != null) _context.Post(_ => CollectionChanged?.Invoke(this, e), null);
            else CollectionChanged?.Invoke(this, e);
        }

        #region Fields
        private ObservableCollection<RssCategory> _items = new ObservableCollection<RssCategory>();
        private Dictionary<Uri, RssFeed> _feeds = new Dictionary<Uri, RssFeed>();
        private SynchronizationContext _context = SynchronizationContext.Current;
        #endregion

        #endregion
    }
}
