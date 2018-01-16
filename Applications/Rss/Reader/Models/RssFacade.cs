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
using System.Linq;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Net.Rss;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFacade
    ///
    /// <summary>
    /// RSS フィードに関連する処理の窓口となるクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public sealed class RssFacade : IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssFacade
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="settings">設定用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssFacade(SettingsFolder settings)
        {
            Settings = settings;

            Subscription.IO = Settings.IO;
            Subscription.CacheDirectory = Settings.Cache;
            if (IO.Exists(settings.Feed)) Subscription.Load(settings.Feed);
            Subscription.CollectionChanged += (s, e) => Subscription.Save(Settings.Feed);
            Subscription.SubCollectionChanged += (s, e) => Subscription.Save(Settings.Feed);
            Subscription.Received += WhenReceived;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        /// 
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder Settings { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        /// 
        /// <summary>
        /// 入出力用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Operator IO => Settings.IO;

        /* ----------------------------------------------------------------- */
        ///
        /// Subscription
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscription Subscription { get; } = new RssSubscription();

        /* ----------------------------------------------------------------- */
        ///
        /// Entry
        /// 
        /// <summary>
        /// 選択中の Web サイトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssEntryBase> Entry { get; } = new Bindable<RssEntryBase>();

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 選択中の Web サイトの RSS フィードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssFeed> Feed { get; } = new Bindable<RssFeed>();

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        /// 
        /// <summary>
        /// 対象とする記事の内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Content { get; } = new Bindable<string>();

        /* ----------------------------------------------------------------- */
        ///
        /// Message
        /// 
        /// <summary>
        /// メッセージを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Message { get; } = new Bindable<string>();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        ///
        /// <summary>
        /// 新規 URL を登録します。
        /// </summary>
        /// 
        /// <param name="src">URL</param>
        /// 
        /* ----------------------------------------------------------------- */
        public Task Register(string src) => Subscription.Register(
            src.Contains("://") ?
            new Uri(src) :
            new Uri("http://" + src)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Refresh
        ///
        /// <summary>
        /// 選択中の RSS フィードの内容を更新します。
        /// </summary>
        /// 
        /// <param name="src">選択中の RSS エントリ</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Refresh(RssEntryBase src)
        {
            if (src is RssEntry entry) Subscription.Update(entry.Uri);
            else if (src is RssCategory category)
            {
                Subscription.Update(category.Entries.Select(e => e.Uri));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Refresh
        ///
        /// <summary>
        /// 選択中の RSS フィードの内容を更新します。
        /// </summary>
        /// 
        /// <param name="src">選択中の RSS フィード</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Refresh(RssFeed src)
        {
            if (src != null) Subscription.Update(src.Uri);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// RSS フィードを選択します。
        /// </summary>
        /// 
        /// <param name="src">選択項目</param>
        /// 
        /// <remarks>
        /// 未読アイテムがゼロの場合、選択項目から外れたタイミングで
        /// 全記事を削除しています。これは主にメモリ使用量の抑制を目的と
        /// しています。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        public void Select(RssEntryBase src)
        {
            if (src == null) return;

            Entry.Value = src;
            if (src is RssEntry entry)
            {
                var prev = Feed.Value;
                if (prev?.UnreadItems.Count() <= 0) prev.Items.Clear();
                Feed.Value = Subscription.Lookup(entry.Uri);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        /// 
        /// <summary>
        /// RSS フィード中の記事内容を取得します。
        /// </summary>
        /// 
        /// <param name="src">対象とする記事</param>
        /// 
        /// <remarks>
        /// Read メソッドが実行されたタイミングで RssArticle.Read が
        /// true に設定されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Read(RssItem src)
        {
            if (src == null) return;

            src.Read = true;
            Content.Value = string.Format(
                Properties.Resources.Skeleton,
                Properties.Resources.SkeletonStyle,
                src.Link,
                src.Title,
                src.PublishTime,
                !string.IsNullOrEmpty(src.Content) ? src.Content : src.Summary
            );
        }

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~RssFacade
        /// 
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~RssFacade() { Dispose(false); }

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
            Dispose(true);
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
            if (_disposed) return;
            _disposed = true;
            Subscription.Dispose();
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenReceived
        /// 
        /// <summary>
        /// 新着記事を受信した時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenReceived(object sender, KeyValueEventArgs<Uri, RssFeed> e) =>
            Message.Value = string.Format(Properties.Resources.MessageReceived,
                e.Value.Items.Count,
                e.Value.Title
            );

        #endregion

        #region Fields
        private bool _disposed = false;
        #endregion
    }
}
