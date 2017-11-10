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
using Cube.Settings;
using Cube.Xui;

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
    public sealed class RssSubscribeCollection : IEnumerable<RssCategory>, INotifyCollectionChanged
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
            _items.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uris
        /// 
        /// <summary>
        /// 購読しているフィードの URL 一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<Uri> Uris => _lookup.Keys;

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
        /// Clear
        /// 
        /// <summary>
        /// コレクションの内容をクリアします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Clear()
        {
            _items.Clear();
            _lookup.Clear();
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
            var src = SettingsType.Json.Load<List<RssCategory.Json>>(json);
            foreach (var item in src.Select(e => e.Convert()))
            {
                MakeLookup(item);
                _items.Add(item);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Lookup
        /// 
        /// <summary>
        /// URL に対応するオブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// 
        /// <returns>RssEntry オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry Lookup(Uri uri)
            => _lookup.ContainsKey(uri) ? _lookup[uri] : null;

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
        /* ----------------------------------------------------------------- */
        public IEnumerator<RssCategory> GetEnumerator()
            => _items.GetEnumerator();

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

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// MakeLookup
        /// 
        /// <summary>
        /// 検索用ディクショナリを作成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MakeLookup(RssCategory src)
        {
            foreach (var entry in src.Entries)
            {
                if (_lookup.ContainsKey(entry.Uri)) continue;
                _lookup.Add(entry.Uri, entry);
            }

            if (src.Categories == null) return;
            foreach (var category in src.Categories) MakeLookup(category);
        }

        #region Fields
        private BindableCollection<RssCategory> _items = new BindableCollection<RssCategory>();
        private Dictionary<Uri, RssEntry> _lookup = new Dictionary<Uri, RssEntry>();
        #endregion

        #endregion
    }
}
