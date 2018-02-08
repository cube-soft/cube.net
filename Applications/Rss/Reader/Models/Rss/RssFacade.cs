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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Cube.Conversions;
using Cube.Net.Rss;

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
            Settings.LoadOrDelete();
            Settings.PropertyChanged += WhenSettingsChanged;
            Settings.AutoSave = true;

            Core.IO = Settings.IO;
            Core.FileName = Settings.Feed;
            Core.CacheDirectory = Settings.Cache;
            Core.Set(RssCheckFrequency.High, Settings.Value.HighInterval);
            Core.Set(RssCheckFrequency.Low, Settings.Value.LowInterval);
            Core.Received += WhenReceived;
            Core.Load();

            Data = new RssBindableData(Core, Settings.Value);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Data
        ///
        /// <summary>
        /// バインド可能なデータ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssBindableData Data { get; }

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
        /// Property
        ///
        /// <summary>
        /// 選択中の Web ページのプロパティを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssEntry Property { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Core
        ///
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssSubscriber Core { get; } = new RssSubscriber();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Setup
        ///
        /// <summary>
        /// 初期処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Setup()
        {
            var entry = Core.Find<RssEntry>(Settings.Value.Start);
            if (entry != null)
            {
                Select(entry);
                entry.Parent.Expand();
            }
            Core.Start();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NewEntry
        ///
        /// <summary>
        /// 新規 URL を登録します。
        /// </summary>
        ///
        /// <param name="src">URL</param>
        ///
        /* ----------------------------------------------------------------- */
        public async Task NewEntry(string src)
        {
            Core.Suspend();
            try { await Core.RegisterAsync(src.ToUri()); }
            finally { Core.Start(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NewCategory
        ///
        /// <summary>
        /// 新しいカテゴリを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void NewCategory() =>
            Select(Core.Create(Data.Entry.Value));

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateEntry
        ///
        /// <summary>
        /// 選択中の RSS エントリの内容を更新します。
        /// </summary>
        ///
        /// <param name="src">選択中の RSS エントリ</param>
        ///
        /* ----------------------------------------------------------------- */
        public void UpdateEntry(IRssEntry src)
        {
            if (src is RssEntry entry) Core.Update(entry.Uri);
            else if (src is RssCategory category)
            {
                Core.Update(category.Entries.Select(e => e.Uri));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateFeed
        ///
        /// <summary>
        /// 選択中の RSS フィードの内容を更新します。
        /// </summary>
        ///
        /// <param name="src">選択中の RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public void UpdateFeed(RssFeed src) => Core.Update(src?.Uri);

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// 項目を移動します。
        /// </summary>
        ///
        /// <param name="src">移動元の項目</param>
        /// <param name="dest">移動先のカテゴリ</param>
        /// <param name="index">カテゴリ中の挿入場所</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Move(IRssEntry src, IRssEntry dest, int index) =>
            Core.Move(src, dest, index);

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 現在選択中の RSS エントリを削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Remove()
        {
            if (Data.Entry.Value != null) Core.Remove(Data.Entry.Value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Rename
        ///
        /// <summary>
        /// 現在選択中の RSS エントリの名前を変更可能な状態に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Rename() => Data.Entry.Value.Editing = true;

        /* ----------------------------------------------------------------- */
        ///
        /// ReadAll
        ///
        /// <summary>
        /// 現在選択中の RSS エントリ下の全ての記事を既読に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void ReadAll() => Data.Entry.Value?.Read();

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Reset()
        {
            if (Data.Entry.Value is RssEntry entry) Core.Reset(entry.Uri);
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
        public void Select(IRssEntry src)
        {
            if (src == null) return;

            Data.Entry.Value = src;

            if (src is RssEntry current && current != Property)
            {
                current.Selected = true;
                Property = current;
                Data.Feed.Value = current;

                var items = Data.Feed.Value?.Items;
                var first = items?.Count > 0 ? items[0] : null;
                Select(first);
                Settings.Value.Start = current.Uri;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// RSS フィード中の記事を選択します。
        /// </summary>
        ///
        /// <param name="src">選択項目</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Select(RssItem src)
        {
            if (src == null) return;
            if (Property.SkipContent) Data.Uri.Value = src.Link;
            else Data.Article.Value = src;
            if (Data.Feed.Value is RssEntry entry) entry.Read(src);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// OPML 形式ファイルをインポートします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Import(string path)
        {
            var dest = RssOpml.Load(path);
            if (dest.Count() <= 0) return;

            try
            {
                Core.Stop();
                Core.Clear();
                Core.Add(dest);
            }
            finally { Core.Start(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// OPML 形式でエクスポートします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Export(string path) => RssOpml.Save(Core, path, Settings.IO);

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        ///
        /// <summary>
        /// RSS フィードのチェック方法を再設定します。
        /// </summary>
        ///
        /// <param name="src">選択項目</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Reschedule(RssEntry src) => Core.Reschedule(src);

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

            if (disposing)
            {
                var value = Data.Article.Value;
                if (value != null) value.Status = RssItemStatus.Read;
                Core.Dispose();
                Settings.Dispose();
            }
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
        private void WhenReceived(object sender, ValueEventArgs<RssFeed> e)
        {
            var src  = e.Value;
            var dest = Core.Find<RssFeed>(src.Uri);
            if (src == null || dest == null) return;

            src.Items = src.Items.Shrink(dest.LastChecked).ToList();
            foreach (var item in src.Items) dest.Items.Insert(0, item);

            dest.Description   = src.Description;
            dest.Link          = src.Link;
            dest.LastChecked   = src.LastChecked;
            dest.LastPublished = src.LastPublished;

            if (dest is RssEntry re) re.Count = re.UnreadItems.Count();
            if (!Settings.Value.EnableMonitorMessage) return;

            Data.Message.Value =
                src.Items.Count > 0 ?
                string.Format(Properties.Resources.MessageReceived, src.Items.Count, src.Title) :
                string.Format(Properties.Resources.MessageNoReceived, src.Title);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenSettingsChanged
        ///
        /// <summary>
        /// 設定内容変更時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Value.HighInterval):
                    Core.Set(RssCheckFrequency.High, Settings.Value.HighInterval);
                    break;
                case nameof(Settings.Value.LowInterval):
                    Core.Set(RssCheckFrequency.Low, Settings.Value.LowInterval);
                    break;
            }
        }

        #endregion

        #region Fields
        private bool _disposed = false;
        #endregion
    }
}
