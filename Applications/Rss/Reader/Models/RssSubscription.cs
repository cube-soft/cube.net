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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Net.Rss;
using Cube.Settings;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssSubscription
    ///
    /// <summary>
    /// 購読フィード一覧を管理するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public sealed class RssSubscription
        : IEnumerable<RssEntryBase>, INotifyCollectionChanged, IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssSubscription
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscription()
        {
            _monitor = new RssMonitor(_feeds) { Interval = TimeSpan.FromHours(1) };
            _monitor.Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _items.Synchronously = true;
            _items.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
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
        /// Categories
        /// 
        /// <summary>
        /// カテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssCategory> Categories => this.OfType<RssCategory>();

        /* ----------------------------------------------------------------- */
        ///
        /// Entries
        /// 
        /// <summary>
        /// どのカテゴリにも属さないエントリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssEntry> Entries => this.OfType<RssEntry>();

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

        /* ----------------------------------------------------------------- */
        ///
        /// SubCollectionChanged
        /// 
        /// <summary>
        /// サブコレクション変更時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler SubCollectionChanged;

        /* ----------------------------------------------------------------- */
        ///
        /// Received
        /// 
        /// <summary>
        /// 新着記事を受信した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueEventHandler<RssFeed> Received;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        /// 
        /// <summary>
        /// 新規 URL を非同期で登録します。
        /// </summary>
        /// 
        /// <param name="uri">URL オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public async Task Register(Uri uri)
        {
            using (var http = new RssClient())
            {
                var rss = await http.GetAsync(uri).ConfigureAwait(false);
                if (rss == null) throw Error(Properties.Resources.ErrorFeedNotFound);
                if (_feeds.ContainsKey(rss.Uri)) throw Error(Properties.Resources.ErrorFeedNotFound);

                _items.Add(new RssEntry
                {
                    Title = rss.Title,
                    Uri = rss.Uri,
                    Count = rss.UnreadItems.Count(),
                });

                _feeds.Add(rss.Uri, rss);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateCategory
        ///
        /// <summary>
        /// 新しい RssCategory オブジェクトを生成して挿入します。
        /// </summary>
        /// 
        /// <param name="src">挿入位置</param>
        /// 
        /// <returns>生成オブジェクト</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public RssCategory CreateCategory(RssEntryBase src)
        {
            var parent = src is RssCategory c ? c : src?.Parent;
            var dest = new RssCategory
            {
                Title  = "新しいフォルダー",
                Parent = parent,
                Editing = true,
            };

            var items = parent != null ? parent.Items : _items;
            var count = parent != null ? parent.Entries.Count() : Entries.Count();
            items.Insert(items.Count - count, dest);
            Expand(parent);

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        /// 
        /// <param name="uri">対象とするフィード URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Update(Uri uri) => _monitor.Update(uri);

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        /// 
        /// <param name="uris">対象とするフィード URL 一覧</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Update(IEnumerable<Uri> uris) => _monitor.Update(uris);

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        /// 
        /// <summary>
        /// RSS フィードを削除します。
        /// </summary>
        /// 
        /// <param name="src">削除する RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Remove(RssEntryBase src)
        {
            var parent = src.Parent;
            if (parent != null)
            {
                var prev = parent.Items.Count;
                parent.Items.Remove(src);
            }
            else _items.Remove(src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        /// 
        /// <summary>
        /// 項目を移動します。
        /// </summary>
        /// 
        /// <param name="src">移動元の項目</param>
        /// <param name="dest">移動先のカテゴリ</param>
        /// <param name="index">カテゴリ中の挿入場所</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Move(RssEntryBase src, RssEntryBase dest, int index)
        {
            if (src.Parent is RssCategory c) c.Items.Remove(src);
            else _items.Remove(src);

            src.Parent = dest as RssCategory;
            var items = src.Parent?.Items ?? _items;
            if (index < 0 || index >= items.Count) items.Add(src);
            else items.Insert(index, src);
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
            _monitor.Stop();
            if (_items.Count > 0) _items.Clear();
            _feeds.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Reset(Uri uri)
        {
            _feeds.DeleteCache(uri);
            var feed = FindFeed(uri);
            feed.Items.Clear();
            feed.LastChecked = DateTime.MinValue;
            Update(uri);
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
        public void Load(string json)
        {
            Clear();

            using (var s = IO.OpenRead(json))
            {
                var src = SettingsType.Json.Load<List<RssCategory.Json>>(s);
                foreach (var item in src.Select(e => e.Convert(null)))
                {
                    MakeFeed(item);
                    if (!string.IsNullOrEmpty(item.Title)) _items.Add(item);
                    else foreach (var entry in item.Items)
                    {
                        System.Diagnostics.Debug.Assert(entry is RssEntry);
                        entry.Parent = null;
                        _items.Add(entry);
                    }
                }
            }

            _monitor.Start();
        }

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
            using (var s = IO.Create(json))
            {
                var root = new RssCategory
                {
                    Title = string.Empty,
                    Items = new BindableCollection<RssEntryBase>(_items.OfType<RssEntry>()),
                };

                var data = _items
                    .OfType<RssCategory>()
                    .Concat(new[] { root })
                    .Select(e => new RssCategory.Json(e));

                SettingsType.Json.Save(s, data);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FindFeed
        /// 
        /// <summary>
        /// URL に対応する RssEntry オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// 
        /// <returns>RssEntry オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry FindEntry(Uri uri)
        {
            var cvt = _items.Flatten(e => (e is RssCategory c) ? c.Items : null)
                            .OfType<RssEntry>();

            return uri != null ?
                   cvt.FirstOrDefault(e => e.Uri == uri) :
                   cvt.FirstOrDefault();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FindFeed
        /// 
        /// <summary>
        /// URL に対応する RssFeed オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed FindFeed(Uri uri) => _feeds.ContainsKey(uri) ? _feeds[uri] : null;

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
        public IEnumerator<RssEntryBase> GetEnumerator() => _items.GetEnumerator();

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
        /* ----------------------------------------------------------------- */
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
        ~RssSubscription() { Dispose(false); }

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

            if (disposing)
            {
                _monitor.Dispose();
                _feeds.Dispose();
            }
        }

        #endregion

        #endregion

        #region Implementations

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
            src.PropertyChanged -= WhenPropertyChanged;
            src.PropertyChanged += WhenPropertyChanged;
            src.Items.CollectionChanged += (s, e) =>
            {
                src.Count = src.Items.Aggregate(0, (x, i) => x + i.Count);
                SubCollectionChanged?.Invoke(this, e);
            };

            foreach (var entry in src.Entries)
            {
                if (_feeds.ContainsKey(entry.Uri)) continue;

                var items = new BindableCollection<RssItem>();
                var feed  = new RssFeed
                {
                    Title = entry.Title,
                    Uri   = entry.Uri,
                    Items = items,
                };
                items.CollectionChanged += (s, e) => entry.Count = feed.UnreadItems.Count();

                _feeds.Add(entry.Uri, feed);

                entry.PropertyChanged -= WhenPropertyChanged;
                entry.PropertyChanged += WhenPropertyChanged;
                entry.Count = _feeds[entry.Uri].UnreadItems.Count();
            }

            if (src.Categories == null) return;
            foreach (var category in src.Categories) MakeFeed(category);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Expand
        /// 
        /// <summary>
        /// RSS カテゴリの子要素が表示された状態に設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Expand(RssCategory src)
        {
            while (src != null)
            {
                src.Expanded = true;
                src = src.Parent;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateCount
        /// 
        /// <summary>
        /// 未読記事数を更新します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void UpdateCount(RssEntryBase src)
        {
            if (src == null || src.Parent == null) return;
            var parent = src.Parent;
            parent.Count = parent.Items.Aggregate(0, (x, e) => x + e.Count);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        /// 
        /// <summary>
        /// 例外オブジェクトを生成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private ArgumentException Error(string message)
            => new ArgumentException(message);

        /* ----------------------------------------------------------------- */
        ///
        /// WhenPropertyChanged
        /// 
        /// <summary>
        /// RssEntryBase のプロパティ変更時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void WhenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RssEntryBase.Count)) return;
            UpdateCount(sender as RssEntryBase);
        }

        #endregion

        #region Fields
        private bool _disposed = false;
        private BindableCollection<RssEntryBase> _items = new BindableCollection<RssEntryBase>();
        private RssCacheCollection _feeds = new RssCacheCollection();
        private RssMonitor _monitor;
        #endregion
    }
}
