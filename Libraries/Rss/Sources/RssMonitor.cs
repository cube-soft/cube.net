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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cube.Collections;
using Cube.Mixin.Logging;
using Cube.Mixin.Tasks;
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssMonitor
    ///
    /// <summary>
    /// Provides functionality to periodically get RSS feeds from the
    /// provided HTTP servers.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssMonitor : HttpMonitorBase<RssFeed>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the RssMonitor class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor() : base(new() { UseEntityTag = false })
        {
            _http      = new RssClient(Handler);
            Timeout    = _http.Timeout;
            RetryCount = 2;
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Contains
        ///
        /// <summary>
        /// 指定された URL が監視対象かどうかを判別します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィード URL</param>
        ///
        /// <returns>監視対象かどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Contains(Uri uri) => _feeds.ContainsKey(uri);

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        ///
        /// <summary>
        /// 監視対象となる RSS フィード URL を登録します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィード URL</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Register(Uri uri) => Register(uri, null);

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        ///
        /// <summary>
        /// 監視対象となる RSS フィード URL を登録します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィード URL</param>
        /// <param name="last">最終チェック日時</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Register(Uri uri, DateTime? last)
        {
            if (Contains(uri)) return;
            _feeds.Add(uri, last);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        ///
        /// <summary>
        /// 監視対象となる RSS フィード URL 一覧を登録します。
        /// </summary>
        ///
        /// <param name="items">RSS フィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Register(IEnumerable<Uri> items) => Register(
            items.Select(e => KeyValuePair.Create(e, default(DateTime?)))
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        ///
        /// <summary>
        /// 監視対象となる RSS フィード URL 一覧を登録します。
        /// </summary>
        ///
        /// <param name="items">RSS フィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Register(IEnumerable<KeyValuePair<Uri, DateTime?>> items)
        {
            foreach (var kv in items) Register(kv.Key, kv.Value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 監視対象リストから削除します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィード URL</param>
        ///
        /// <returns>削除に成功したかどうか</returns>
        ///
        /* ----------------------------------------------------------------- */
        public bool Remove(Uri uri) => _feeds.Remove(uri);

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        ///
        /// <summary>
        /// 監視対象リストをクリアします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Clear() => _feeds.Clear();

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。更新が終了すると Publish
        /// メソッドを通じて結果が通知されます。
        /// </summary>
        ///
        /// <param name="src">対象とするフィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(params Uri[] src) => Update((IEnumerable<Uri>)src);

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。非同期処理は非同期で実行され、
        /// 終了すると Subscribe で登録されたコールバック関数を通じて
        /// 結果が通知されます。
        /// </summary>
        ///
        /// <param name="src">対象とするフィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(IEnumerable<Uri> src) => Task.Run(async () =>
        {
            Suspend();
            foreach (var uri in src)
            {
                try { await Publish(uri).ConfigureAwait(false); }
                catch (Exception err)
                {
                    await PublishError(new Dictionary<Uri, Exception> {
                        { uri, err }
                    }).ConfigureAwait(false);
                }
            }
            if (State == TimerState.Suspend) Start();
        }).Forget();

        /* ----------------------------------------------------------------- */
        ///
        /// LastChecked
        ///
        /// <summary>
        /// 指定された URL の最後チェック日時を取得します。
        /// </summary>
        ///
        /// <param name="uri">RSS フィード URL</param>
        ///
        /// <returns>最後チェック日時</returns>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastChecked(Uri uri) => _feeds[uri];

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequests
        ///
        /// <summary>
        /// Gets the sequence of HTTP request URLs.
        /// </summary>
        ///
        /// <returns>Sequence of HTTP Request URLs</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected override IEnumerable<Uri> GetRequests() =>
            _feeds.OrderBy(e => e.Value).Select(e => e.Key).ToList();

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// Gets the HTTP response from the specified URL, parse it into
        /// an RSS feed, and publishes the result.
        /// </summary>
        ///
        /// <param name="uri">HTTP request URL.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task Publish(Uri uri)
        {
            if (!Contains(uri)) return;

            this.LogWarn(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });
            var sw   = Stopwatch.StartNew();
            var dest = await _http.GetAsync(uri).ConfigureAwait(false);
            this.LogDebug($"{uri} ({sw.Elapsed})");

            _feeds[uri] = dest.LastChecked;
            await Publish(uri, dest).ConfigureAwait(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// Publishes the specified RSS feed.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task Publish(Uri uri, RssFeed feed)
        {
            foreach (var cb in Subscription)
            {
                try { await cb(uri, feed); }
                catch (Exception err) { this.LogWarn(err); }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PublishError
        ///
        /// <summary>
        /// Publishes error results.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task PublishError(IDictionary<Uri, Exception> errors)
        {
            foreach (var kv in errors)
            {
                await Publish(kv.Key, new()
                {
                    Uri         = kv.Key,
                    LastChecked = DateTime.Now,
                    Error       = kv.Value,
                }).ConfigureAwait(false);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnError
        ///
        /// <summary>
        /// Occurs when some errors are detected.
        /// </summary>
        ///
        /// <param name="errors">Error list.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnError(IDictionary<Uri, Exception> errors) =>
            PublishError(errors).Forget();

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
            try { if (disposing) _http.Dispose(); }
            finally { base.Dispose(disposing); }
        }

        #endregion

        #region Fields
        private readonly RssClient _http;
        private readonly Dictionary<Uri, DateTime?> _feeds = new();
        #endregion
    }
}
