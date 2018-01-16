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
using System.Threading.Tasks;
using Cube.Net.Http;
using Cube.Tasks;
using Cube.Log;

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
        public RssMonitor() : this(new Dictionary<Uri, RssFeed>()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="buffer">結果を保持するためのバッファ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor(IDictionary<Uri, RssFeed> buffer) : base()
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            Feeds   = buffer;
            Handler = new HeaderHandler { UseEntityTag = false };
            _http   = new RssClient(Handler);
            Timeout = _http.Timeout;

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
        protected IDictionary<Uri, RssFeed> Feeds { get; }

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
        protected IList<Func<Uri, RssFeed, Task>> Subscriptions { get; }
            = new List<Func<Uri, RssFeed, Task>>();

        #endregion

        #region Methods

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
        public void Register(Uri uri)
        {
            if (Feeds.ContainsKey(uri)) return;

            var feed = new RssFeed
            {
                Title = uri.ToString(),
                Uri   = uri,
            };

            Feeds.Add(uri, feed);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
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
        public IDisposable Subscribe(Func<Uri, RssFeed, Task> action)
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
        public IDisposable Subscribe(Action<Uri, RssFeed> action) =>
            Subscribe(async (u, v) =>
        {
            action(u, v);
            await Task.FromResult(0);
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
        /// <param name="uri">対象とするフィード URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Update(Uri uri) => Update(new[] { uri });

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
        /// <remarks>
        /// Feeds に登録されていない URL が指定された場合、無視されます。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        public void Update(IEnumerable<Uri> uris) => Task.Run(async () =>
        {
            Suspend();
            foreach (var uri in uris)
            {
                try { await UpdateAsync(uri); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
            }
            if (State == TimerState.Suspend) Start();
        }).Forget();

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// 新しい結果を発行します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// <param name="feed">RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual async Task Publish(Uri uri, RssFeed feed)
        {
            foreach (var action in Subscriptions)
            {
                try { await action(uri, feed); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
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
        }

        #endregion

        #region Implementations

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
            if (!Feeds.ContainsKey(uri)) return;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var dest = Feeds[uri];
            var src = await GetAsync(uri).ConfigureAwait(false);
            Shrink(src, dest.LastChecked);
            sw.Stop();
            this.LogDebug($"Url:{uri}\tTime:{sw.Elapsed}");

            foreach (var item in src.Items) dest.Items.Insert(0, item);
            dest.Title       = src.Title;
            dest.Description = src.Description;
            dest.Link        = src.Link;
            dest.LastChecked = src.LastChecked;

            if (src.Items.Count > 0) await Publish(uri, src);
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
            try { if (_http.Timeout != Timeout) _http.Timeout = Timeout; }
            catch (Exception /* err */) { this.LogWarn("Timeout cannot be applied"); }
            return await _http.GetAsync(uri);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Shrink
        ///
        /// <summary>
        /// 更新日時を基準として不要な項目を削除します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Shrink(RssFeed src, DateTime threshold) =>
            src.Items = src
                .Items
                .Where(e => e.PublishTime > threshold)
                .OrderBy(e => e.PublishTime)
                .ToList();

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
            if (State != TimerState.Run) return;

            foreach (var uri in Feeds.Keys.ToArray())
            {
                try { await UpdateAsync(uri); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
            }
        }

        #region Fields
        private RssClient _http;
        #endregion

        #endregion
    }
}
