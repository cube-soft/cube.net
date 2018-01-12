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

            Items.IO = Settings.IO;
            Items.CacheDirectory = Settings.Cache;
            if (IO.Exists(settings.Feed)) Items.Load(settings.Feed);
            Items.CollectionChanged += (s, e) => Items.Save(Settings.Feed);
            Items.SubCollectionChanged += (s, e) => Items.Save(Settings.Feed);
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
        /// Items
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssSubscription Items { get; } = new RssSubscription();

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 対象となる Web サイトの RSS フィードを取得します。
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
        public Task Register(string src) => Items.Register(
            src.Contains("://") ?
            new Uri(src) :
            new Uri("http://" + src)
        );

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
        public void Select(RssEntry src)
        {
            var prev = Feed.Value;
            if (prev?.UnreadItems.Count() <= 0) prev.Items.Clear();
            Feed.Value = Items.Lookup(src.Uri);
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
            Items.Dispose();
        }

        #endregion

        #endregion

        #region Fields
        private bool _disposed = false;
        #endregion
    }
}
