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
    public class RssMonitor : HttpMonitorBase<RssFeed>
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
        public RssMonitor(IDictionary<Uri, RssFeed> buffer) : this(buffer,
            new ContentHandler<RssFeed>()
            {
                UseEntityTag = false,
            })
        {
            Timer.Subscribe(WhenTick);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="buffer">結果を保持するためのバッファ</param>
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor(IDictionary<Uri, RssFeed> buffer, ContentHandler<RssFeed> handler)
            : base(handler)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            Feeds = buffer;
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
        protected IDictionary<Uri, RssFeed> Feeds { get; }

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

            var feed = new RssFeed { Title = uri.ToString() };
            feed.Link = uri;
            Feeds.Add(uri, feed);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィードの内容を更新します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override Task Publish(Uri uri, RssFeed value)
        {
            if (Feeds.ContainsKey(uri))
            {
                Feeds[uri].Title = value.Title;
                Feeds[uri].Items = value.Items;
                Feeds[uri].LastChecked = DateTime.Now;
                return base.Publish(uri, value);
            }
            else return Task.FromResult(0);
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
            if (State != TimerState.Run) return;

            foreach (var uri in Feeds.Keys.ToArray())
            {
                try { await PublishAsync(uri); }
                catch (Exception err) { this.LogWarn(err.ToString(), err); }
                await Task.Delay(1000); // TODO
            }
        }

        #endregion
    }
}
