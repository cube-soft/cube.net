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
using Cube.Collections;
using Cube.FileSystem;
using Cube.FileSystem.Files;
using Cube.Log;
using Cube.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssCacheDictionary
    ///
    /// <summary>
    /// RSS フィードを保持するためのコレクションクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssCacheDictionary : IDictionary<Uri, RssFeed>, IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssCacheDictionary
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCacheDictionary() : this(new Dictionary<Uri, RssFeed>()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssCacheDictionary
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="inner">内部バッファ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssCacheDictionary(IDictionary<Uri, RssFeed> inner)
        {
            _dispose = new OnceAction<bool>(Dispose);
            _inner   = inner;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        ///
        /// <summary>
        /// 入出力用オブジェクトを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Operator IO { get; set; } = new Operator();

        /* ----------------------------------------------------------------- */
        ///
        /// Directory
        ///
        /// <summary>
        /// キャッシュを保存するためのディレクトリのパスを取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Directory { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Capacity
        ///
        /// <summary>
        /// メモリ上に保持しておく要素数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Capacity
        {
            get => _capacity;
            set => _capacity = Math.Max(value, 1);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnlyCache
        ///
        /// <summary>
        /// キャッシュファイルを読み込み専用で利用するかどうかを示す値を
        /// 取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnlyCache { get; set; } = false;

        #region IDictionary<Uri, RssFeed>

        /* ----------------------------------------------------------------- */
        ///
        /// Item
        ///
        /// <summary>
        /// 指定したキーを持つ要素を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed this[Uri key]
        {
            get => Get(key, false);
            set => _inner[key] = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Keys
        ///
        /// <summary>
        /// キー一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<Uri> Keys => _inner.Keys;

        /* ----------------------------------------------------------------- */
        ///
        /// Values
        ///
        /// <summary>
        /// 値一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<RssFeed> Values => _inner.Values;

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        ///
        /// <summary>
        /// 要素の数を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count => _inner.Count;

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// 読み取り専用かどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly => _inner.IsReadOnly;

        #endregion

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Get
        ///
        /// <summary>
        /// URL に対応するオブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="key">URL</param>
        /// <param name="locked">メモリ上にロックするかどうか</param>
        ///
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed Get(Uri key, bool locked) => Pop(_inner[key], locked);

        /* ----------------------------------------------------------------- */
        ///
        /// Unlock
        ///
        /// <summary>
        /// ロック状態を解除します。
        /// </summary>
        ///
        /// <param name="key">URL</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Unlock(Uri key)
        {
            if (_memory.ContainsKey(key)) _memory[key] = false;
            Stash();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// メモリ上にある要素を全てキャッシュファイルに保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save()
        {
            foreach (var key in _memory.Keys.ToList()) this.LogWarn(() => SaveCache(key));
            _memory.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SaveCache
        ///
        /// <summary>
        /// RSS フィードをキャッシュファイルに保存します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィードの URL</param>
        ///
        /* ----------------------------------------------------------------- */
        public void SaveCache(Uri uri)
        {
            if (string.IsNullOrEmpty(Directory) || !_inner.ContainsKey(uri)) return;

            var feed = _inner[uri];
            if (feed == null || !feed.LastChecked.HasValue) return;

            if (!IO.Exists(Directory)) IO.CreateDirectory(Directory);
            IO.Save(CacheName(uri), e => SettingsType.Json.Save(e, new RssFeed.Json(feed)));

            feed.Items.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DeleteCache
        ///
        /// <summary>
        /// キャッシュファイルを削除します。
        /// </summary>
        ///
        /// <param name="uri">削除する RSS フィードの URL</param>
        ///
        /* ----------------------------------------------------------------- */
        public void DeleteCache(Uri uri) => IO.Delete(CacheName(uri));

        #region IDictionary<Uri, RssFeed>

        /* ----------------------------------------------------------------- */
        ///
        /// Contains
        ///
        /// <summary>
        /// 指定された要素が含まれているかどうかを判別します。
        /// </summary>
        ///
        /// <param name="item">要素</param>
        ///
        /// <returns>含まれているかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Contains(KeyValuePair<Uri, RssFeed> item) => _inner.Contains(item);

        /* ----------------------------------------------------------------- */
        ///
        /// ContainsKey
        ///
        /// <summary>
        /// 指定されたキーを持つ要素が含まれているかどうかを判別します。
        /// </summary>
        ///
        /// <param name="key">要素</param>
        ///
        /// <returns>含まれているかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool ContainsKey(Uri key) => _inner.ContainsKey(key);

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 要素を追加します。
        /// </summary>
        ///
        /// <param name="item">要素</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(RssFeed item) => Add(item.Uri, item);

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 要素を追加します。
        /// </summary>
        ///
        /// <param name="item">要素</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(KeyValuePair<Uri, RssFeed> item)
        {
            _inner.Add(item);
            Pop(item.Value, false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 指定したキーおよび値を持つ要素を追加します。
        /// </summary>
        ///
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(Uri key, RssFeed value) =>
            Add(new KeyValuePair<Uri, RssFeed>(key, value));

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 指定したキーを持つ要素を削除します。
        /// </summary>
        ///
        /// <param name="key">キー</param>
        ///
        /// <returns>削除が成功したかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(Uri key) => Remove(key, false);

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 指定したキーを持つ要素を削除します。
        /// </summary>
        ///
        /// <param name="key">キー</param>
        /// <param name="deleteCache">
        /// キャッシュファイルを削除するかどうか
        /// </param>
        ///
        /// <returns>削除が成功したかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(Uri key, bool deleteCache)
        {
            var result = _inner.Remove(key);
            if (result) _memory.Remove(key);
            if (deleteCache) DeleteCache(key);
            return result;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 指定した要素を削除します。
        /// </summary>
        ///
        /// <param name="item">要素</param>
        ///
        /// <returns>削除が成功したかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(KeyValuePair<Uri, RssFeed> item) => Remove(item.Key);

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
            _memory.Clear();
            _inner.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TryGetValue
        ///
        /// <summary>
        /// 指定したキーに対応する値を取得します。
        /// </summary>
        ///
        /// <param name="key">キー</param>
        /// <param name="value">値を格納するためのオブジェクト</param>
        ///
        /// <returns>取得が成功したかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool TryGetValue(Uri key, out RssFeed value)
        {
            var result = _inner.TryGetValue(key, out value);
            if (result) Pop(value, false);
            return result;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CopyTo
        ///
        /// <summary>
        /// 全ての要素を配列にコピーします。
        /// </summary>
        ///
        /// <remarks>
        /// このメソッドはサポートされません。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void CopyTo(KeyValuePair<Uri, RssFeed>[] array, int index) =>
            throw new NotSupportedException();

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        ///
        /// <summary>
        /// コレクションを反復処理する列挙子を取得します。
        /// </summary>
        ///
        /// <returns>列挙子</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerator<KeyValuePair<Uri, RssFeed>> GetEnumerator()
        {
            foreach (var kv in _inner)
            {
                Pop(kv.Value, false);
                yield return kv;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        ///
        /// <summary>
        /// コレクションを反復処理する列挙子を取得します。
        /// </summary>
        ///
        /// <returns>列挙子</returns>
        ///
        /* ----------------------------------------------------------------- */
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~RssCacheCollection
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~RssCacheDictionary() { _dispose.Invoke(false); }

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
            if (!IsReadOnlyCache) Save();
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Stash
        ///
        /// <summary>
        /// 最も古い要素をファイルに保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Stash()
        {
            if (IsReadOnlyCache || _memory.Count <= Capacity) return;
            var key = _memory.FirstOrDefault(e => !e.Value).Key;
            if (key == null) return;
            SaveCache(key);
            _memory.Remove(key);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Pop
        ///
        /// <summary>
        /// RSS フィードをキャッシュファイルから復旧します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFeed Pop(RssFeed dest, bool locked)
        {
            if (dest != null)
            {
                var f = locked;
                if (_memory.ContainsKey(dest.Uri))
                {
                    f |= _memory[dest.Uri];
                    _memory.Remove(dest.Uri);
                }
                else Load(dest);

                _memory.Add(dest.Uri, f);
                Stash();
            }
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Pop
        ///
        /// <summary>
        /// RSS フィードをキャッシュファイルから読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Load(RssFeed dest)
        {
            var feed = Load(dest.Uri);
            if (feed == null) return;

            dest.Title         = feed.Title;
            dest.Description   = feed.Description;
            dest.Link          = feed.Link;
            dest.LastChecked   = feed.LastChecked;
            dest.LastPublished = feed.LastPublished;

            if (dest.Items.Count > 0) dest.Items.Clear();
            foreach (var a in feed.UnreadItems) dest.Items.Add(a);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// RSS フィードをキャッシュファイルから読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFeed Load(Uri uri) =>
            !string.IsNullOrEmpty(Directory) ?
            IO.Load(CacheName(uri), e =>
            {
                try { return SettingsType.Json.Load<RssFeed.Json>(e).Convert(); }
                catch { return default(RssFeed); }
            }) :
            default(RssFeed);

        /* ----------------------------------------------------------------- */
        ///
        /// CacheName
        ///
        /// <summary>
        /// キャッシュが保存されるファイルのパスを取得します。
        /// </summary>
        ///
        /// <param name="uri">URL</param>
        ///
        /// <returns>キャッシュファイルのパス</returns>
        ///
        /* ----------------------------------------------------------------- */
        private string CacheName(Uri uri)
        {
            var md5  = new MD5CryptoServiceProvider();
            var data = System.Text.Encoding.UTF8.GetBytes(uri.ToString());
            var hash = md5.ComputeHash(data);
            var name = BitConverter.ToString(hash).ToLowerInvariant().Replace("-", "");
            return IO.Combine(Directory, name);
        }

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private readonly IDictionary<Uri, RssFeed> _inner;
        private readonly OrderedDictionary<Uri, bool> _memory = new OrderedDictionary<Uri, bool>();
        private int _capacity = 100;
        #endregion
    }
}
