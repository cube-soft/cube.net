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
using System.Linq;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Net.Rss;
using Cube.Settings;
using Cube.Xui;
using Cube.Log;

namespace Cube.Net.App.Rss.Reader
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
        /// RssSubscription
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscriber()
        {
            _dispose = new OnceAction<bool>(Dispose);

            _tree.IsSynchronous = true;
            _tree.CollectionChanged += (s, e) => OnCollectionChanged(e);

            _monitors[0] = new RssMonitor() { Interval = TimeSpan.FromHours(1) };
            _monitors[0].Subscribe(OnReceived);

            _monitors[1] = new RssMonitor() { Interval = TimeSpan.FromHours(24) };
            _monitors[1].Subscribe(OnReceived);
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
        /// どのカテゴリにも属さない RSS エントリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssEntry> Entries => this.OfType<RssEntry>();

        #endregion

        #region Events

        #region CollectionChanged

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
        /// OnCollectionChanged
        /// 
        /// <summary>
        /// CollectionChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Save();
            CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Received

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

        /* ----------------------------------------------------------------- */
        ///
        /// OnReceived
        /// 
        /// <summary>
        /// Received イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OnReceived(RssFeed src)
        {
            var dest = Find<RssFeed>(src.Uri);
            if (dest == null) return;

            src.Items = src.Items.Shrink(dest.LastChecked).ToList();
            foreach (var item in src.Items) dest.Items.Insert(0, item);

            dest.Description   = src.Description;
            dest.Link          = src.Link;
            dest.LastChecked   = src.LastChecked;
            dest.LastPublished = src.LastPublished;

            Received?.Invoke(this, ValueEventArgs.Create(src));
        }

        #endregion

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
        public T Find<T>(Uri uri) where T : class =>
            _feeds.ContainsKey(uri) ? _feeds[uri] as T : null;
        
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
            var dest   = new RssCategory
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
        /// Register
        /// 
        /// <summary>
        /// 新しい RSS フィード URL を非同期で登録します。
        /// </summary>
        /// 
        /// <param name="uri">URL オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public async Task Register(Uri uri)
        {
            Suspend();
            var rss = await _client.GetAsync(uri).ConfigureAwait(false);
            if (rss == null) throw Properties.Resources.ErrorFeedNotFound.ToException();
            if (_feeds.ContainsKey(rss.Uri)) throw Properties.Resources.ErrorFeedAlreadyExists.ToException();

            var dest = new RssEntry(rss);
            RegisterCore(dest);
            Start();
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
            if (src.Parent is RssCategory rc) rc.Children.Remove(src);
            else _tree.Remove(src);

            var parent = dest as RssCategory;
            var items  = parent?.Children ?? _tree;
            src.Parent = parent;
            if (index < 0 || index >= items.Count) items.Add(src);
            else items.Insert(index, src);
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
        /// Clear
        /// 
        /// <summary>
        /// 全ての項目を削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Clear()
        {
            Suspend();
            foreach (var e in _tree.ToArray()) Remove(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// 設定ファイルを読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Load()
        {
            Clear();
            foreach (var rc in LoadCore())
            {
                if (!string.IsNullOrEmpty(rc.Title)) RegisterCore(rc);
                else foreach (var re in rc.Entries)
                {
                    re.Parent = null;
                    RegisterCore(re);
                }
            }
            Start();
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
        public void Save() => SaveCore(
            Categories.Concat(new[] { new RssCategory
            {
                Title    = string.Empty,
                Children = new BindableCollection<IRssEntry>(Entries),
            }})
        );

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
        /* ----------------------------------------------------------------- */
        public void Start()
        {
            _monitors[0].Start();
            _monitors[1].Start(TimeSpan.FromMinutes(1));
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
        /// <param name="uri">対象とするフィード URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Update(Uri uri)
        {
            var mon = _monitors[0].Contains(uri) ? _monitors[0] :
                      _monitors[1].Contains(uri) ? _monitors[1] : null;
            mon?.Update(uri);
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
            var m0 = uris.Where(e => _monitors[0].Contains(e));
            if (m0.Count() > 0) _monitors[0].Update(m0);

            var m1 = uris.Where(e => _monitors[1].Contains(e));
            if (m1.Count() > 0) _monitors[1].Update(m1);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得します。
        /// </summary>
        /// 
        /// <param name="uri">対象とするフィード URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Reset(Uri uri)
        {
            _feeds.DeleteCache(uri);
            var dest = Find<RssFeed>(uri);
            if (dest != null)
            {
                dest.Items.Clear();
                dest.LastChecked = DateTime.MinValue;
                Update(uri);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        /// 
        /// <summary>
        /// RSS フィードのチェック方法を再設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Reschedule()
        {
            var now = DateTime.Now;
            _monitors[0].Reschedule(_feeds.Values, e => e.IsHighFrequency(now));
            _monitors[1].Reschedule(_feeds.Values, e => e.IsLowFrequency(now));
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
                       src.IsLowFrequency(now)  ? _monitors[1] : null;

            foreach (var mon in _monitors)
            {
                if (!mon.Contains(src.Uri)) continue;
                if (mon == dest) return;
                mon.Remove(src.Uri);
                break;
            }

            dest?.Register(src.Uri, src.LastChecked);
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
        /// <param name="sec">チェック間隔</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Set(RssCheckFrequency kind, int sec) =>
            Set(kind, TimeSpan.FromSeconds(sec));

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
        public void Set(RssCheckFrequency kind, TimeSpan time)
        {
            var mon = kind == RssCheckFrequency.High ? _monitors[0] :
                      kind == RssCheckFrequency.Low  ? _monitors[1] : null;

            if (mon != null && !mon.Interval.Equals(time))
            {
                mon.Stop();
                mon.Interval = time;
                mon.Start(time);
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
                foreach (var mon in _monitors) mon.Dispose();
                _feeds.Dispose();
            }
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// LoadCore
        /// 
        /// <summary>
        /// 設定ファイルからカテゴリおよび RSS エントリ情報を読み込みます。
        /// </summary>
        /// 
        /// <remarks>
        /// TODO: エラー時に元ファイルの削除およびバックアップ領域からの
        /// 復旧処理を追加。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        private IEnumerable<RssCategory> LoadCore()
        {
            if (IO.Exists(FileName))
            {
                try
                {
                    using (var ss = IO.OpenRead(FileName))
                    {
                        return SettingsType.Json
                                           .Load<List<RssCategory.Json>>(ss)
                                           .Select(e => e.Convert(null));
                    }
                }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
            }
            return new RssCategory[0];
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SaveCore
        /// 
        /// <summary>
        /// カテゴリおよび RSS エントリ情報を設定ファイルに保存します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void SaveCore(IEnumerable<RssCategory> src)
        {
            try
            {
                var json = src.Select(e => new RssCategory.Json(e));
                using (var ss = IO.Create(FileName)) SettingsType.Json.Save(ss, json);
            }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterCore
        /// 
        /// <summary>
        /// カテゴリおよびカテゴリ中の RSS エントリを全て登録します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void RegisterCore(RssCategory src)
        {
            foreach (var re in src.Entries) RegisterCore(re);
            foreach (var rc in src.Categories) RegisterCore(rc);

            src.Children.CollectionChanged -= WhenChildrenChanged;
            src.Children.CollectionChanged += WhenChildrenChanged;

            if (src.Parent == null) _tree.Add(src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterCore
        /// 
        /// <summary>
        /// RSS エントリを登録します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void RegisterCore(RssEntry src)
        {
            if (_feeds.ContainsKey(src.Uri)) return;
            var items = new BindableCollection<RssItem>(src.Items);
            src.Items = src.Items.ToBindable();

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

            foreach (var item in src.Children)
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

            _feeds.Remove(src.Uri);

            if (src.Parent is RssCategory rc) rc.Children.Remove(src);
            else _tree.Remove(src);
            src.Dispose();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChildrenChanged
        /// 
        /// <summary>
        /// RssCategory.Children が変更時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void WhenChildrenChanged(object s, NotifyCollectionChangedEventArgs e) => Save();

        #endregion

        #region Fields
        private OnceAction<bool> _dispose;
        private BindableCollection<IRssEntry> _tree = new BindableCollection<IRssEntry>();
        private RssCacheDictionary _feeds = new RssCacheDictionary();
        private RssMonitor[] _monitors = new RssMonitor[2];
        private RssClient _client = new RssClient();
        #endregion
    }
}
