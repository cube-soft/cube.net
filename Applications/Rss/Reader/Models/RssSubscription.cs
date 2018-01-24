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
            _primary = new RssMonitor() { Interval = TimeSpan.FromHours(1) };
            _primary.Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _secondary = new RssMonitor() { Interval = TimeSpan.FromHours(24) };
            _secondary.Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _tree.Synchronously = true;
            _tree.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
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
        /// Clear
        /// 
        /// <summary>
        /// コレクションの内容をクリアします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Clear()
        {
            _primary.Stop();
            _secondary.Stop();

            _tree.Clear();
            _entries.Clear();
            _feeds.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
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
        public RssCategory Create(RssEntryBase src)
        {
            var dest = new RssCategory
            {
                Title   = "新しいフォルダー",
                Parent  = src is RssCategory c ? c : src?.Parent,
                Editing = true,
            };

            var items = src.Parent != null ? src.Parent.Items : _tree;
            var count = src.Parent != null ? src.Parent.Entries.Count() : Entries.Count();

            items.Insert(items.Count - count, dest);
            src.Parent.Expand();

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetEntry
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
        public RssEntry GetEntry(Uri uri) => _entries.ContainsKey(uri) ? _entries[uri] : null;

        /* ----------------------------------------------------------------- */
        ///
        /// GetFeed
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
        public RssFeed GetFeed(Uri uri) => _feeds.ContainsKey(uri) ? _feeds[uri] : null;

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
                    if (!string.IsNullOrEmpty(item.Title)) _tree.Add(item);
                    else foreach (var entry in item.Items)
                    {
                        entry.Parent = null;
                        _tree.Add(entry);
                    }
                }
            }

            _primary.Start();
            _secondary.Start();
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
            else _tree.Remove(src);

            src.Parent = dest as RssCategory;
            var items = src.Parent?.Items ?? _tree;
            if (index < 0 || index >= items.Count) items.Add(src);
            else items.Insert(index, src);
        }

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
                if (_feeds.ContainsKey(rss.Uri)) throw Error(Properties.Resources.ErrorFeedAlreadyExists);

                _tree.Add(new RssEntry
                {
                    Title = rss.Title,
                    Uri   = rss.Uri,
                    Count = rss.UnreadItems.Count(),
                });

                _feeds.Add(rss.Uri, rss);
            }
        }

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
            if (src.Parent != null) src.Parent.Items.Remove(src);
            else _tree.Remove(src);

            if (src is RssEntry entry)
            {
                _entries.Remove(entry.Uri);
                _feeds.Remove(entry.Uri);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        /// 
        /// <summary>
        /// RssMonitor を再設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Reschedule()
        {
            var now = DateTime.Now;
            var thresh = TimeSpan.FromDays(30);

            ResetPrimary(now, thresh);
            ResetSecondary(now, thresh);
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
            var feed = GetFeed(uri);
            feed.Items.Clear();
            feed.LastChecked = new DateTime(1970, 1, 1);
            Update(uri);
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
            var root = new RssCategory
            {
                Title = string.Empty,
                Items = new BindableCollection<RssEntryBase>(_tree.OfType<RssEntry>()),
            };

            var data = _tree.OfType<RssCategory>()
                            .Concat(new[] { root })
                            .Select(e => new RssCategory.Json(e));

            using (var s = IO.Create(json)) SettingsType.Json.Save(s, data);
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
        public void Update(Uri uri)
        {
            if (_primary.Contains(uri)) _primary.Update(uri);
            else if (_secondary.Contains(uri)) _secondary.Update(uri);
        }

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
        public void Update(IEnumerable<Uri> uris)
        {
            var pi = uris.Where(e => _primary.Contains(e));
            var si = uris.Where(e => _secondary.Contains(e));

            _primary.Update(pi);
            _secondary.Update(si);
        }

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
        public IEnumerator<RssEntryBase> GetEnumerator() => _tree.GetEnumerator();

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
                _primary.Dispose();
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
        /// ResetPrimary
        /// 
        /// <summary>
        /// RssMonitor を再設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void ResetPrimary(DateTime now, TimeSpan thresh)
        {
            _primary.Suspend();
            _primary.Clear();

            foreach (var uri in _feeds
                .Where(e => now - e.Value.LastPublished <= thresh)
                .Select(e => e.Key)
            ) _primary.Register(uri);

            _primary.Start();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ResetSecondary
        /// 
        /// <summary>
        /// RssMonitor を再設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void ResetSecondary(DateTime now, TimeSpan thresh)
        {
            _secondary.Suspend();
            _secondary.Clear();

            foreach (var uri in _feeds
                .Where(e => now - e.Value.LastPublished > thresh)
                .Select(e => e.Key)
            ) _secondary.Register(uri);

            _secondary.Start();
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
        private BindableCollection<RssEntryBase> _tree = new BindableCollection<RssEntryBase>();
        private IDictionary<Uri, RssEntry> _entries = new Dictionary<Uri, RssEntry>();
        private RssCacheDictionary _feeds = new RssCacheDictionary();
        private RssMonitor _primary;
        private RssMonitor _secondary;
        #endregion
    }
}
