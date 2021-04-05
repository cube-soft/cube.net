﻿/* ------------------------------------------------------------------------- */
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
    /// 定期的に登録した URL からフィードを取得するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssMonitor : NetworkMonitorBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor()
        {
            Handler    = new HeaderHandler { UseEntityTag = false };
            _http      = new RssClient(Handler);
            Timeout    = _http.Timeout;
            RetryCount = 2;

            Timer.Subscribe(WhenTick);
        }

        #endregion

        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// ユーザエージェントを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public string UserAgent
        {
            get => Handler.UserAgent;
            set => Handler.UserAgent = value;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Feeds
        ///
        /// <summary>
        /// RSS フィードの管理用コレクションを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected IDictionary<Uri, DateTime?> Feeds { get; } =
            new Dictionary<Uri, DateTime?>();

        /* ----------------------------------------------------------------- */
        ///
        /// Handler
        ///
        /// <summary>
        /// HTTP 通信用ハンドラを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected HeaderHandler Handler { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscriptions
        ///
        /// <summary>
        /// 購読者一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected IList<Func<RssFeed, Task>> Subscriptions { get; } =
            new List<Func<RssFeed, Task>>();

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
        public bool Contains(Uri uri) => Feeds.ContainsKey(uri);

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
        public DateTime? LastChecked(Uri uri) => Feeds[uri];

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
            Feeds.Add(uri, last);
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
        public bool Remove(Uri uri) => Feeds.Remove(uri);

        /* ----------------------------------------------------------------- */
        ///
        /// Clear
        ///
        /// <summary>
        /// 監視対象リストをクリアします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Clear() => Feeds.Clear();

        /* ----------------------------------------------------------------- */
        ///
        /// SubscribeAsync
        ///
        /// <summary>
        /// データ受信時に非同期実行する処理を登録します。
        /// </summary>
        ///
        /// <param name="action">非同期実行する処理</param>
        ///
        /// <returns>登録解除用オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable SubscribeAsync(Func<RssFeed, Task> action)
        {
            Subscriptions.Add(action);
            return Disposable.Create(() => Subscriptions.Remove(action));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
        ///
        /// <summary>
        /// データ受信時に実行する処理を登録します。
        /// </summary>
        ///
        /// <param name="action">実行する処理</param>
        ///
        /// <returns>登録解除用オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable Subscribe(Action<RssFeed> action) => SubscribeAsync(e =>
        {
            action(e);
            return Task.FromResult(0);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。更新が終了すると Publish
        /// メソッドを通じて結果が通知されます。
        /// </summary>
        ///
        /// <param name="uris">対象とするフィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(params Uri[] uris) => Update((IEnumerable<Uri>)uris);

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
        /// <param name="uris">対象とするフィード URL 一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(IEnumerable<Uri> uris) => Task.Run(async () =>
        {
            Suspend();
            foreach (var uri in uris)
            {
                try { await UpdateAsync(uri).ConfigureAwait(false); }
                catch (Exception err) { await PublishErrorAsync(uri, err).ConfigureAwait(false); }
            }
            if (State == TimerState.Suspend) Start();
        }).Forget();

        /* ----------------------------------------------------------------- */
        ///
        /// PublishAsync
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        ///
        /// <param name="feed">RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual async Task PublishAsync(RssFeed feed)
        {
            foreach (var action in Subscriptions)
            {
                try { await action(feed); }
                catch (Exception err) { this.LogWarn(err); }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージリソースを解放するかどうかを示す値
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _http.Dispose();
                Handler.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// PublishErrorAsync
        ///
        /// <summary>
        /// エラー内容を非同期で通知します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Task PublishErrorAsync(Uri uri, Exception error) =>
            PublishErrorAsync(new Dictionary<Uri, Exception> { { uri, error } });

        /* ----------------------------------------------------------------- */
        ///
        /// PublishErrorAsync
        ///
        /// <summary>
        /// エラー内容を非同期で通知します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task PublishErrorAsync(IDictionary<Uri, Exception> errors)
        {
            foreach (var kv in errors)
            {
                await PublishAsync(new RssFeed
                {
                    Uri         = kv.Key,
                    LastChecked = DateTime.Now,
                    Error       = kv.Value,
                }).ConfigureAwait(false);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateAsync
        ///
        /// <summary>
        /// 指定された URL に対応する RSS フィードを非同期で更新します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task UpdateAsync(Uri uri)
        {
            if (!Contains(uri)) return;

            var sw   = Stopwatch.StartNew();
            var dest = await GetAsync(uri).ConfigureAwait(false);
            this.LogDebug($"{uri} ({sw.Elapsed})");

            Feeds[uri] = dest.LastChecked;
            await PublishAsync(dest).ConfigureAwait(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        ///  GetAsync
        ///
        /// <summary>
        /// 指定された URL から RSS フィードを非同期で取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<RssFeed> GetAsync(Uri uri)
        {
            this.LogWarn(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });
            return await _http.GetAsync(uri);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsync
        ///
        /// <summary>
        /// RSS の取得処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task RunAsync(IList<Uri> src, IDictionary<Uri, Exception> errors)
        {
            foreach (var uri in src.ToArray())
            {
                this.LogDebug($"TRY: {uri}");
                try
                {
                    if (State != TimerState.Run) return;
                    await UpdateAsync(uri).ConfigureAwait(false);
                }
                catch (Exception err) { errors.Add(uri, err); }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenTick
        ///
        /// <summary>
        /// 一定間隔で実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task WhenTick()
        {
            if (Subscriptions.Count <= 0) return;
            if (!Network.Available)
            {
                this.LogDebug("Network not available");
                return;
            }

            var src    = Feeds.OrderBy(e => e.Value).Select(e => e.Key).ToList();
            var errors = new Dictionary<Uri, Exception>();
            await RunAsync(src, errors).ConfigureAwait(false);

            for (var i = 0; i < RetryCount && errors.Count > 0; ++i)
            {
                await Task.Delay(RetryInterval).ConfigureAwait(false);
                var retry = errors.Keys.ToList();
                errors.Clear();
                await RunAsync(retry, errors).ConfigureAwait(false);
            }

            await PublishErrorAsync(errors).ConfigureAwait(false);
        }

        #endregion

        #region Fields
        private readonly RssClient _http;
        #endregion
    }
}
