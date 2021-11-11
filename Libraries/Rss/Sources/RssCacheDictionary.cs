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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Cube.Collections;
using Cube.DataContract;
using Cube.FileSystem;
using Cube.Mixin.String;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssCacheDictionary
    ///
    /// <summary>
    /// Provides functionality to preserve the RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssCacheDictionary : EnumerableBase<KeyValuePair<Uri, RssFeed>>, IDictionary<Uri, RssFeed>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssCacheDictionary
        ///
        /// <summary>
        /// Initializes a new instance of the RssCacheDictionary class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCacheDictionary() : this(new Dictionary<Uri, RssFeed>()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssCacheDictionary
        ///
        /// <summary>
        /// Initializes a new instance of the RssCacheDictionary class with
        /// the specified collection.
        /// </summary>
        ///
        /// <param name="inner">Collection for inner buffer.</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssCacheDictionary(IDictionary<Uri, RssFeed> inner)
        {
            _inner = inner;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Directory
        ///
        /// <summary>
        /// Gets or sets the path of the directory for storing the cache.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Directory { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Capacity
        ///
        /// <summary>
        /// Gets or sets the number of elements to be kept in memory.
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
        /// Gets or sets a value indicating whether the cache file should
        /// be used as read-only.
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
        /// Gets or sets the element with the specified key.
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
        /// Gets the collection of feed URLs that represent dictionary keys.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<Uri> Keys => _inner.Keys;

        /* ----------------------------------------------------------------- */
        ///
        /// Values
        ///
        /// <summary>
        /// Gets the collection of RSS feeds.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<RssFeed> Values => _inner.Values;

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        ///
        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count => _inner.Count;

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
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
        /// Gets the feed corresponding to the specified URL.
        /// </summary>
        ///
        /// <param name="key">URL.</param>
        /// <param name="locked">Whether to lock it in memory</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed Get(Uri key, bool locked) => MarkMemory(_inner[key], locked);

        /* ----------------------------------------------------------------- */
        ///
        /// Unlock
        ///
        /// <summary>
        /// Unlocks the value corresponding to the specified URL.
        /// </summary>
        ///
        /// <param name="key">URL.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Unlock(Uri key)
        {
            if (_memory.ContainsKey(key)) _memory[key] = false;
            Stash();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Delete
        ///
        /// <summary>
        /// Delete the cache file.
        /// </summary>
        ///
        /// <param name="uri">URL of the RSS feed to delete.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Delete(Uri uri)
        {
            var dest = CacheName(uri);
            if (dest.HasValue()) Io.Delete(dest);
        }

        #region IDictionary<Uri, RssFeed>

        /* ----------------------------------------------------------------- */
        ///
        /// Contains
        ///
        /// <summary>
        /// Determines whether the specified item is included.
        /// </summary>
        ///
        /// <param name="item">Item to check.</param>
        ///
        /// <returns>true if contained.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Contains(KeyValuePair<Uri, RssFeed> item) => _inner.Contains(item);

        /* ----------------------------------------------------------------- */
        ///
        /// ContainsKey
        ///
        /// <summary>
        /// Determines whether an element with the specified key is included.
        /// </summary>
        ///
        /// <param name="key">Key to check.</param>
        ///
        /// <returns>true if contained.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool ContainsKey(Uri key) => _inner.ContainsKey(key);

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// Adds the specified RSS feed.
        /// </summary>
        ///
        /// <param name="item">RSS feed to add.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(RssFeed item) => Add(item.Uri, item);

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        ///
        /// <param name="item">Item to add.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(KeyValuePair<Uri, RssFeed> item)
        {
            _inner.Add(item);
            var dest = CacheName(item.Key);
            if (dest.HasValue() && !Io.Exists(dest)) _ = MarkMemory(item.Value, false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// Adds an item with the specified key and value.
        /// </summary>
        ///
        /// <param name="key">URL.</param>
        /// <param name="value">RSS feed.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Add(Uri key, RssFeed value) =>
            Add(new KeyValuePair<Uri, RssFeed>(key, value));

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        ///
        /// <param name="key">URL to remove.</param>
        ///
        /// <returns>true for success.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(Uri key) => Remove(key, false);

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        ///
        /// <param name="key">URL to remove.</param>
        /// <param name="deleteCache">
        /// Whether to delete the cache file.
        /// </param>
        ///
        /// <returns>true for success.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(Uri key, bool deleteCache)
        {
            var result = _inner.Remove(key);
            if (result) _ = _memory.Remove(key);
            if (deleteCache) Delete(key);
            return result;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// Removes the specified item.
        /// </summary>
        ///
        /// <param name="item">Item to remove.</param>
        ///
        /// <returns>true for success.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(KeyValuePair<Uri, RssFeed> item) => Remove(item.Key);

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        ///
        /// <summary>
        /// Clears the all of items.
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
        /// Tries to get the value corresponding to the specified key.
        /// </summary>
        ///
        /// <param name="key">URL.</param>
        /// <param name="value">Object for setting the value.</param>
        ///
        /// <returns>true for success.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool TryGetValue(Uri key, out RssFeed value)
        {
            var result = _inner.TryGetValue(key, out value);
            if (result) _ = MarkMemory(value, false);
            return result;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CopyTo
        ///
        /// <summary>
        /// Copies all items to the specified array.
        /// </summary>
        ///
        /// <remarks>
        /// The method does not be supported.
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
        /// Gets the enumerator to iterate over the collection.
        /// </summary>
        ///
        /// <returns>Enumerator to iterate.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public override IEnumerator<KeyValuePair<Uri, RssFeed>> GetEnumerator()
        {
            foreach (var kv in _inner)
            {
                _ = MarkMemory(kv.Value, false);
                yield return kv;
            }
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// Releases the unmanaged resources used by the WakeableTimer
        /// and optionally releases the managed resources.
        /// </summary>
        ///
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (!IsReadOnlyCache)
            {
                foreach (var key in _memory.Keys.ToList()) GetType().LogWarn(() => Save(key, false));
                _memory.Clear();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MarkMemory
        ///
        /// <summary>
        /// Marks the specified RSS feed as existing in memory.
        /// Also, if there is a cache file for the specified RSS feed,
        /// it will be loaded.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFeed MarkMemory(RssFeed dest, bool locked)
        {
            try
            {
                if (dest != null)
                {
                    var exist = _memory.ContainsKey(dest.Uri);
                    var value = exist ?
                                locked | _memory[dest.Uri] :
                                locked;

                    if (exist) _ = _memory.Remove(dest.Uri);
                    else Load(dest);

                    _memory.Add(dest.Uri, value);
                }
                return dest;
            }
            finally { Stash(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stash
        ///
        /// <summary>
        /// Saves the oldest element to a file.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Stash()
        {
            if (IsReadOnlyCache || _memory.Count <= Capacity) return;
            var key = _memory.FirstOrDefault(e => !e.Value).Key;
            if (key == null) return;
            Save(key, true);
            _ = _memory.Remove(key);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// Reads RSS feeds from a cache file.
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
        /// Reads RSS feeds from a cache file.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFeed Load(Uri uri)
        {
            var src = CacheName(uri);
            return Io.Exists(src) ?
                   Format.Json.Deserialize<RssFeed.Json>(src).Convert() :
                   null;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// Saves the RSS feed to a cache file.
        /// </summary>
        ///
        /// <param name="uri">URL of the RSS feed to save.</param>
        /// <param name="shrink">
        /// Whether to lighten the RSS feed in memory after saving.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        private void Save(Uri uri, bool shrink)
        {
            var dest = CacheName(uri);
            if (!dest.HasValue() || !_inner.ContainsKey(uri)) return;

            var src = _inner[uri];
            if (src == null || !src.LastChecked.HasValue) return;

            Io.CreateDirectory(Directory);
            Format.Json.Serialize(dest, new RssFeed.Json(src));
            if (shrink) src.Items.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CacheName
        ///
        /// <summary>
        /// Get the path to the file where the cache is saved.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string CacheName(Uri uri)
        {
            if (string.IsNullOrEmpty(Directory) || uri == null) return string.Empty;

            var md5  = new MD5CryptoServiceProvider();
            var data = System.Text.Encoding.UTF8.GetBytes(uri.ToString());
            var hash = md5.ComputeHash(data);
            var name = BitConverter.ToString(hash).ToLowerInvariant().Replace("-", "");
            return Io.Combine(Directory, name);
        }

        #endregion

        #region Fields
        private readonly IDictionary<Uri, RssFeed> _inner;
        private readonly OrderedDictionary<Uri, bool> _memory = new();
        private int _capacity = 100;
        #endregion
    }
}
