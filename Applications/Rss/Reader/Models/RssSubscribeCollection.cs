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
using Cube.FileSystem;
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
    public sealed class RssSubscribeCollection : IEnumerable, INotifyCollectionChanged, IDisposable
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
            _monitor = new RssMonitor(_feeds)
            {
                Interval = TimeSpan.FromHours(1),
            };
            _items.CollectionChanged += WhenCollectionChanged;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        /// 
        /// <summary>
        /// 入出力用のオブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Operator IO
        {
            get => _feeds.IO;
            set => _feeds.IO = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectory
        /// 
        /// <summary>
        /// キャッシュ用ディレクトリのパスを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string CacheDirectory
        {
            get => _feeds.Directory;
            set => _feeds.Directory = value;
        }

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
        /// <remarks>
        /// TODO: Reset イベントではなく Add イベントを発生させるように
        /// 修正する。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(RssFeed feed) => Reset(() =>
        {
            Default.Entries.Add(new RssEntry
            {
                Title = feed.Title,
                Uri   = feed.Link,
            });

            _feeds.Add(feed.Link, feed);
            _monitor.Start();
        });

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
            _monitor.Stop();

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
        public void Load(string json) => Reset(() =>
        {
            Clear();

            using (var s = IO.OpenRead(json))
            {
                var src = SettingsType.Json.Load<List<RssCategory.Json>>(s);
                foreach (var item in src.Select(e => e.Convert()))
                {
                    MakeFeed(item);
                    _items.Add(item);
                }
            }

            _monitor.Start();
        });

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// JSON ファイルに保存します。
        /// </summary>
        /// 
        /// <param name="json">JSON ファイルのパス</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Save(string json)
        {
            using (var s = IO.OpenWrite(json))
            {
                var data = _items.Select(e => new RssCategory.Json(e));
                SettingsType.Json.Save(s, data);
            }
        }

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
        public RssFeed Lookup(Uri uri) => _feeds.ContainsKey(uri) ? _feeds[uri] : null;

        #region IEnumerable

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

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~RssSubscribeCollection
        /// 
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~RssSubscribeCollection() { Dispose(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        /// 
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        /// 
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            if (disposing) _monitor.Dispose();
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// コレクション内容をリセットします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Reset(Action action)
        {
            try
            {
                _items.CollectionChanged -= WhenCollectionChanged;
                action();
            }
            finally
            {
                _items.CollectionChanged += WhenCollectionChanged;
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
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
                    Links = new List<Uri> { entry.Uri },
                });
            }

            if (src.Categories == null) return;
            foreach (var category in src.Categories) MakeFeed(category);
        }

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
        private bool _disposed = false;
        private SynchronizationContext _context = SynchronizationContext.Current;
        private ObservableCollection<RssCategory> _items = new ObservableCollection<RssCategory>();
        private RssCacheCollection _feeds = new RssCacheCollection();
        private RssMonitor _monitor;
        #endregion

        #endregion
    }
}
