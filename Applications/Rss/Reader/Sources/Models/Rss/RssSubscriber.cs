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
using Cube.FileSystem;
using Cube.Tasks;
using Cube.Xui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssSubscriber
    ///
    /// <summary>
    /// 購読フィード一覧を管理するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class RssSubscriber :
        IEnumerable<IRssEntry>, INotifyCollectionChanged, IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssSubscriber
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscriber() : this(SynchronizationContext.Current) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssSubscriber
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="context">同期用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscriber(SynchronizationContext context)
        {
            _dispose = new OnceAction<bool>(Dispose);
            _context = context;

            _tree = new BindableCollection<IRssEntry> { Context = context };
            _tree.CollectionChanged += (s, e) =>
            {
                AutoSaveCore();
                CollectionChanged?.Invoke(this, e);
            };

            _monitors[0] = new RssMonitor { Interval = TimeSpan.FromHours(1) };
            _monitors[0].Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _monitors[1] = new RssMonitor { Interval = TimeSpan.FromHours(24) };
            _monitors[1].Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _monitors[2] = new RssMonitor(); // for RssCheckFrequency.None
            _monitors[2].Subscribe(e => Received?.Invoke(this, ValueEventArgs.Create(e)));

            _autosaver.AutoReset = false;
            _autosaver.Interval = 1000.0;
            _autosaver.Elapsed += WhenAutoSaved;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// RSS エントリ一覧が保存されている JSON ファイルのパスを取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string FileName { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        ///
        /// <summary>
        /// 入出力用のオブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IO IO
        {
            get => _feeds.IO;
            set => _feeds.IO = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Capacity
        ///
        /// <summary>
        /// 未読記事をメモリ上に保持する最大数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Capacity
        {
            get => _feeds.Capacity;
            set => _feeds.Capacity = value;
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
        /// IsReadOnly
        ///
        /// <summary>
        /// キャッシュファイルを読み込み専用で利用するかどうかを示す値を
        /// 取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly
        {
            get => _feeds.IsReadOnlyCache;
            set => _feeds.IsReadOnlyCache = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// User-Agent を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent
        {
            get => _monitors.First().UserAgent;
            set { foreach (var mon in _monitors) mon.UserAgent = value; }
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
        /// どのカテゴリにも属さない RSS エントリ一覧を取得します。
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

        #region Entry

        /* ----------------------------------------------------------------- */
        ///
        /// Find
        ///
        /// <summary>
        /// URL に対応するオブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="uri">URL</param>
        ///
        /// <returns>対応するオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry Find(Uri uri) =>
            uri != null && _feeds.ContainsKey(uri) ? _feeds[uri] as RssEntry : null;

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// RSS エントリの選択状況を変更します。
        /// </summary>
        ///
        /// <param name="from">直前に選択されていたオブジェクト</param>
        /// <param name="to">選択オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Select(RssEntry from, RssEntry to)
        {
            if (to   != null) _feeds.Get(to.Uri, true); // lock
            if (from != null) _feeds.Unlock(from.Uri);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// 新しいカテゴリを生成して挿入します。
        /// </summary>
        ///
        /// <param name="src">挿入位置</param>
        ///
        /// <returns>カテゴリ</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssCategory Create(IRssEntry src)
        {
            var parent = src is RssCategory rc ? rc : src?.Parent as RssCategory;
            var dest   = new RssCategory(_context)
            {
                Title   = Properties.Resources.MessageNewCategory,
                Parent  = parent,
                Editing = true,
            };

            var items = parent != null ? parent.Children : _tree;
            var count = parent != null ? parent.Entries.Count() : Entries.Count();
            items.Insert(items.Count - count, dest);
            parent.Expand();

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterAsync
        ///
        /// <summary>
        /// 新しい RSS フィード URL を非同期で登録します。
        /// </summary>
        ///
        /// <param name="uri">URL オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public async Task RegisterAsync(Uri uri)
        {
            var rss = await _client.GetAsync(uri).ConfigureAwait(false);
            if (rss == null) throw Error(Properties.Resources.ErrorFeedNotFound);
            if (_feeds.ContainsKey(rss.Uri)) throw Error(Properties.Resources.ErrorFeedExists);

            AddCore(new RssEntry(rss, _context));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 新しい RSS エントリ一覧を追加します。
        /// </summary>
        ///
        /// <param name="src">RSS エントリ一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(IEnumerable<IRssEntry> src)
        {
            foreach (var item in src)
            {
                if (item is RssCategory rc) AddCore(rc);
                else if (item is RssEntry re) AddCore(re);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// RSS エントリを削除します。
        /// </summary>
        ///
        /// <param name="src">削除する RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Remove(IRssEntry src)
        {
            if (src is RssCategory rc) RemoveCore(rc);
            else if (src is RssEntry re) RemoveCore(re);
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
        public void Move(IRssEntry src, IRssEntry dest, int index)
        {
            var same = dest != null && src.Parent == dest.Parent;
            var si   = (src.Parent as RssCategory)?.Children ?? _tree;
            if (same && si.IndexOf(src) < index) --index;

            var parent = src is RssEntry && dest is RssCategory ?
                         dest as RssCategory :
                         dest?.Parent as RssCategory;
            var di     = parent?.Children ?? _tree;
            src.Parent = parent;
            si.Remove(src);
            if (index < 0 || index >= di.Count) di.Add(src);
            else di.Insert(index, src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// 設定ファイルを読み込みます。
        /// </summary>
        ///
        /// <remarks>
        /// ロード成功時にバックアップファイルを非同期で生成します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Load() {
            Add(RssExtension.Load(FileName, _context, IO).SelectMany(e =>
                !string.IsNullOrEmpty(e.Title) ?
                new[] { e as IRssEntry } :
                e.Entries.Select(re =>
                {
                    re.Parent = null;
                    return re as IRssEntry;
                })
            ));

            if (!IsReadOnly && _feeds.Count > 0) RssExtension.Backup(FileName, IO);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// 設定ファイルに保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save()
        {
            var empty = new RssCategory(_context) { Title = string.Empty };
            foreach (var entry in Entries) empty.Children.Add(entry);
            Categories.Concat(new[] { empty }).Save(FileName, IO);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// OPML 形式ファイルをインポートします。
        /// </summary>
        ///
        /// <param name="path">読み込むファイルのパス</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Import(string path)
        {
            var dest = new RssOpml(_context, IO).Load(path, _feeds);
            if (dest.Count() > 0) Add(dest);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// OPML 形式でエクスポートします。
        /// </summary>
        ///
        /// <param name="path">保存先のパス</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Export(string path) => new RssOpml(_context, IO).Save(this, path);

        #endregion

        #region Monitor

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 監視を開始します。
        /// </summary>
        ///
        /// <param name="delay">初期遅延時間</param>
        ///
        /// <remarks>
        /// _monitors[2] は RssCheckFrequency.None のエントリが Update
        /// 実行時にのみ使用されるため Start メソッドでは起動しません。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Start(TimeSpan delay)
        {
            _monitors[0].Start(delay);
            _monitors[1].Start(TimeSpan.FromSeconds(delay.TotalSeconds * 20));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// 監視を停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop()
        {
            foreach (var mon in _monitors) mon.Stop();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Suspend
        ///
        /// <summary>
        /// 監視を一時停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Suspend()
        {
            foreach (var mon in _monitors) mon.Suspend();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        ///
        /// <param name="src">対象 RSS エントリまたはカテゴリ</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(IRssEntry src) =>
            Update(src.Flatten<RssEntry>().ToArray());

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        ///
        /// <param name="src">対象 RSS エントリ一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(params RssEntry[] src)
        {
            var uris = src.Select(e => e.Uri);

            var m0 = uris.Where(e => _monitors[0].Contains(e));
            if (m0.Count() > 0) _monitors[0].Update(m0);

            var m1 = uris.Where(e => _monitors[1].Contains(e));
            if (m1.Count() > 0) _monitors[1].Update(m1);

            var m2 = uris.Where(e => _monitors[2].Contains(e));
            if (m2.Count() > 0) _monitors[2].Update(m2);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得します。
        /// </summary>
        ///
        /// <param name="src">対象とする RSS エントリ</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Reset(IRssEntry src)
        {
            var entries = src.Flatten<RssEntry>().ToArray();

            foreach (var entry in entries)
            {
                _feeds.Delete(entry.Uri);
                entry.Items.Clear();
                entry.Count = 0;
                entry.LastChecked = null;
            }

            Update(entries);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        ///
        /// <summary>
        /// RSS フィードのチェック方法を再設定します。
        /// </summary>
        ///
        /// <param name="src">対象とする RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Reschedule(RssEntry src)
        {
            var now  = DateTime.Now;
            var dest = src.IsHighFrequency(now) ? _monitors[0] :
                       src.IsLowFrequency(now)  ? _monitors[1] : _monitors[2];

            foreach (var mon in _monitors)
            {
                if (!mon.Contains(src.Uri)) continue;
                if (mon == dest) return;
                mon.Remove(src.Uri);
                break;
            }

            dest.Register(src.Uri, src.LastChecked);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Set
        ///
        /// <summary>
        /// RSS フィードのチェック間隔を設定します。
        /// </summary>
        ///
        /// <param name="kind">種類</param>
        /// <param name="time">チェック間隔</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Set(RssCheckFrequency kind, TimeSpan? time)
        {
            var mon = kind == RssCheckFrequency.High ? _monitors[0] :
                      kind == RssCheckFrequency.Low  ? _monitors[1] : null;

            if (mon != null && time.HasValue && !mon.Interval.Equals(time))
            {
                mon.Stop();
                mon.Interval = time.Value;
                mon.Start(time.Value);
            }
        }

        #endregion

        #region IEnumarable<IRssEntry>

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
        public IEnumerator<IRssEntry> GetEnumerator() => _tree.GetEnumerator();

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
        ~RssSubscriber() { _dispose.Invoke(false); }

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
            _dispose.Invoke(true);
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
            if (disposing)
            {
                _autosaver.Stop();
                _autosaver.Elapsed -= WhenAutoSaved;

                foreach (var mon in _monitors) mon.Dispose();
                _feeds.Dispose();
            }

            if (!IsReadOnly) Save();
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// AddCore
        ///
        /// <summary>
        /// カテゴリおよびカテゴリ中の RSS エントリを全て追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddCore(RssCategory src)
        {
            foreach (var re in src.Entries) AddCore(re);
            foreach (var rc in src.Categories) AddCore(rc);

            src.Children.CollectionChanged -= WhenChildrenChanged;
            src.Children.CollectionChanged += WhenChildrenChanged;

            if (src.Parent == null) _tree.Add(src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddCore
        ///
        /// <summary>
        /// RSS エントリを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddCore(RssEntry src)
        {
            if (_feeds.ContainsKey(src.Uri)) return;
            _feeds.Add(src);
            if (src.Parent == null) _tree.Add(src);
            Reschedule(src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RemoveCore
        ///
        /// <summary>
        /// カテゴリおよびカテゴリ中の RSS エントリを全て削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RemoveCore(RssCategory src)
        {
            src.Children.CollectionChanged -= WhenChildrenChanged;

            foreach (var item in src.Children.ToList())
            {
                if (item is RssCategory c) RemoveCore(c);
                else if (item is RssEntry e) RemoveCore(e);
            }

            if (src.Parent is RssCategory rc) rc.Children.Remove(src);
            else _tree.Remove(src);
            src.Dispose();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RemoveCore
        ///
        /// <summary>
        /// RSS エントリを削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RemoveCore(RssEntry src)
        {
            foreach (var mon in _monitors) mon.Remove(src.Uri);

            _feeds.Remove(src.Uri, true);

            if (src.Parent is RssCategory rc) rc.Children.Remove(src);
            else _tree.Remove(src);
            src.Dispose();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AutoSaveCore
        ///
        /// <summary>
        /// 自動保存を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AutoSaveCore()
        {
            if (!IsReadOnly)
            {
                _autosaver.Stop();
                _autosaver.Interval = 1000.0;
                _autosaver.Start();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        ///
        /// <summary>
        /// 例外オブジェクトに変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Exception Error(string src) => new ArgumentException(src);

        #endregion

        #region Handlers
        private void WhenChildrenChanged(object s, EventArgs e) => AutoSaveCore();
        private void WhenAutoSaved(object s, EventArgs e) => Task.Run(() => Save()).Forget();
        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private readonly BindableCollection<IRssEntry> _tree;
        private readonly RssCacheDictionary _feeds = new RssCacheDictionary();
        private readonly RssMonitor[] _monitors = new RssMonitor[3];
        private readonly RssClient _client = new RssClient();
        private readonly SynchronizationContext _context;
        private readonly System.Timers.Timer _autosaver = new System.Timers.Timer();
        #endregion
    }
}
