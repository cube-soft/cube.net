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
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Net.Http;
using Cube.Settings;
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
        public RssMonitor() : this(new RssContentConverter()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor(IContentConverter<RssFeed> converter)
            : this(new ContentHandler<RssFeed>(converter) { UseEntityTag = false }) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor(ContentHandler<RssFeed> handler) : base()
        {
            _http   = HttpClientFactory.Create(handler);
            Handler = handler;
            Timeout = TimeSpan.FromSeconds(2);
            Timer.Subscribe(WhenTick);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Feeds
        ///
        /// <summary>
        /// RSS フィードの管理用コレクションを取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public IDictionary<Uri, RssFeed> Feeds { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectory
        ///
        /// <summary>
        /// キャッシュの存在するディレクトリのパスを取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string CacheDirectory { get; set; }

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
        /// Handler
        ///
        /// <summary>
        /// HTTP ハンドラを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected ContentHandler<RssFeed> Handler { get; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 監視対象となる URL を追加します。
        /// </summary>
        /// 
        /// <param name="uri">URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Add(Uri uri)
        {
            if (Feeds == null) Feeds = new Dictionary<Uri, RssFeed>();
            if (!Feeds.ContainsKey(uri)) Feeds.Add(uri, new RssFeed());
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// キャッシュファイルを読み込みます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Load()
        {
            if (Feeds == null || string.IsNullOrEmpty(CacheDirectory)) return;
            foreach (var uri in Feeds.Keys.ToArray())
            {
                Log(() =>
                {
                    var feed = Load(uri);
                    if (feed != null) Feeds[uri] = feed;
                });
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// 現在の内容をファイルに保存します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void Save()
        {
            if (Feeds == null || string.IsNullOrEmpty(CacheDirectory)) return;
            foreach (var kv in Feeds) Log(() => Save(kv.Key, kv.Value));
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
        public override void Stop()
        {
            base.Stop();
            _http.CancelPendingRequests();
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

        #region Protected

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// RSS フィードをファイルに保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Save(Uri uri, RssFeed feed)
        {
            if (string.IsNullOrEmpty(CacheDirectory)) return;
            if (!IO.Exists(CacheDirectory)) IO.CreateDirectory(CacheDirectory);

            using (var s = IO.OpenWrite(GetPath(uri))) SettingsType.Json.Save(s, feed);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// RSS フィードをファイルから読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssFeed Load(Uri uri)
        {
            if (string.IsNullOrEmpty(CacheDirectory)) return default(RssFeed);

            var path = GetPath(uri);
            if (!IO.Exists(path)) return default(RssFeed);

            using (var s = IO.OpenRead(path)) return SettingsType.Json.Load<RssFeed>(s);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Update(Uri uri, RssFeed feed)
        {
            if (Feeds.ContainsKey(uri))
            {
                Feeds[uri].Title       = feed.Title;
                Feeds[uri].Items       = feed.Items;
                Feeds[uri].LastChecked = DateTime.Now;
            }
            Save(uri, feed);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsyncCore
        ///
        /// <summary>
        /// 指定された URL から RSS フィードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task GetAsyncCore(Uri uri)
        {
            using (var _ = _lock.LockAsync())
            using (var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseContentRead))
            {
                var status = response.StatusCode;
                var code = (int)status;
                var digit = code / 100;

                if (response.Content is HttpValueContent<RssFeed> content) Update(uri, content.Value); // OK
                else if (digit == 3) this.LogDebug($"HTTP:{code} {status}");
                else if (digit == 4) LogMessage(uri, $"HTTP:{code} {status}");
                else if (digit == 5) throw new HttpRequestException($"HTTP:{code} {status}");
                else LogMessage(uri, $"Content is not {nameof(RssFeed)} ({code})");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// 指定された URL から RSS フィードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task GetAsync(Uri uri)
        {
            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    await GetAsyncCore(uri);
                    break;
                }
                catch (Exception err)
                {
                    this.LogWarn(err.ToString(), err);
                    await Task.Delay(RetryInterval);
                    this.LogDebug($"Retry\tCount:{i + 1}\tUrl:{uri}");
                }
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
        private async void WhenTick()
        {
            if (Feeds == null) return;
            SetTimeout();
            foreach (var uri in Feeds.Keys.ToArray())
            {
                await GetAsync(uri);
                await Task.Delay(1000);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetTimeout
        ///
        /// <summary>
        /// タイムアウト時間を設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void SetTimeout()
        {
            try { if (_http.Timeout != Timeout) _http.Timeout = Timeout; }
            catch (Exception /* err */) { this.LogWarn("Timeout cannot be applied"); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetPath
        ///
        /// <summary>
        /// 保存用パスを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private string GetPath(Uri uri)
        {
            var md5  = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(uri.ToString());
            var hash = md5.ComputeHash(data);
            var name = BitConverter.ToString(hash).ToLower().Replace("-", "");
            return IO.Combine(CacheDirectory, name);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LogMessage
        ///
        /// <summary>
        /// エラー内容をログに出力します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void LogMessage(Uri uri, string message)
        {
            this.LogWarn(uri.ToString());
            this.LogWarn(message);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Log
        ///
        /// <summary>
        /// エラー内容をログに出力します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Log(Action action)
        {
            try { action(); }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
        }

        #region Fields
        private HttpClient _http;
        private readonly AsyncLock _lock = new AsyncLock();
        #endregion

        #endregion
    }
}
