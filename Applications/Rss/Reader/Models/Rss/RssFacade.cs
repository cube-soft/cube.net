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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cube.Conversions;
using Cube.Log;
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
            this.LogWarn(() => settings.Load());

            Settings = settings;
            Settings.PropertyChanged += WhenSettingsChanged;
            Settings.AutoSave = true;

            Core.IO = Settings.IO;
            Core.FileName = Settings.Feed;
            Core.CacheDirectory = Settings.Cache;
            Core.Set(RssCheckFrequency.High, Settings.Value.HighInterval);
            Core.Set(RssCheckFrequency.Low, Settings.Value.LowInterval);
            Core.Received += WhenReceived;

            this.LogDebug("Load", () => Core.Load());
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
            var entry = Core.Find(Settings.Value.Start) ??
                        Core.Flatten<RssEntry>().FirstOrDefault();
            if (entry != null)
            {
                Select(entry);
                entry.Parent.Expand();
            }

            Debug.Assert(Settings.Value.InitialDelay.HasValue);
            Core.Start(Settings.Value.InitialDelay.Value);
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
            finally { Core.Start(TimeSpan.Zero); }
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
        public void NewCategory() => Select(Core.Create(Data.Current.Value));

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// 選択中の RSS エントリの内容を更新します。
        /// カテゴリが指定された場合、カテゴリ中の全 RSS エントリの内容を
        /// 更新します。
        /// </summary>
        ///
        /// <param name="src">選択中の RSS エントリ</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Update(IRssEntry src)
        {
            if (src is RssEntry re) Core.Update(re.Uri);
            else if (src is RssCategory rc) Core.Update(
                rc.Children.Flatten<RssEntry>().Select(e => e.Uri).ToArray()
            );
        }

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
        public void Remove() => Core.Remove(Data.Current.Value);

        /* ----------------------------------------------------------------- */
        ///
        /// Rename
        ///
        /// <summary>
        /// 現在選択中の RSS エントリの名前を変更可能な状態に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Rename() => Data.Current.Value.Editing = true;

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        ///
        /// <summary>
        /// 現在選択中の RSS エントリの全記事を既読に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Read() => Data.Current.Value?.Read();

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
            if (Data.Current.Value is RssEntry entry) Core.Reset(entry.Uri);
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
        /* ----------------------------------------------------------------- */
        public void Select(IRssEntry src)
        {
            Data.Current.Value = src;

            if (src is RssEntry current && current != Data.LastEntry.Value)
            {
                current.Selected = true;
                Data.LastEntry.Value = current;
                Select(current.Items.FirstOrDefault());
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

            Debug.Assert(Data.LastEntry.HasValue);
            var entry = Data.LastEntry.Value;

            if (entry.SkipContent) Data.Uri.Value = src.Link;
            else Data.Article.Value = src;
            entry.Read(src);
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
            var dest = this.LogDebug("Import", () => RssOpml.Load(path, Settings.IO));
            if (dest.Count() <= 0) return;

            try
            {
                Core.Stop();
                Core.Clear();
                Core.Add(dest);
            }
            finally { Core.Start(TimeSpan.Zero); }
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

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// RSS フィードの監視を停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop() => Core.Stop();

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
            var dest = Core.Find(src.Uri);

            if (src == null || dest == null) return;
            if (dest != Data.LastEntry.Value) dest.Shrink();

            dest.Update(src);

            if (Settings.Value.EnableMonitorMessage) Data.Message.Value = src.ToMessage();
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
