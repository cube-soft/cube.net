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
using Cube.Mixin.Logging;
using Cube.Mixin.Uri;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainFacade
    ///
    /// <summary>
    /// MainViewModel とモデルの窓口となるクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class MainFacade : DisposableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MainFacade
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="src">設定用オブジェクト</param>
        /// <param name="dispatcher">同期用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public MainFacade(SettingFolder src, Dispatcher dispatcher)
        {
            _ = src.TryLoad();
            this.LogInfo($"Owner:{src.Lock.UserName}@{src.Lock.MachineName} ({src.Lock.Sid})");
            this.LogInfo($"User-Agent:{src.UserAgent}");

            Setting = src;
            Setting.PropertyChanged += WhenSettingChanged;
            Setting.AutoSave = true;

            var feeds = Setting.IO.Combine(Setting.DataDirectory, LocalSetting.FeedFileName);
            var cache = Setting.IO.Combine(Setting.DataDirectory, LocalSetting.CacheDirectoryName);

            _core = new RssSubscriber(dispatcher)
            {
                IO             = Setting.IO,
                FileName       = feeds,
                CacheDirectory = cache,
                Capacity       = Setting.Shared.Capacity,
                IsReadOnly     = Setting.Lock.IsReadOnly,
                UserAgent      = Setting.UserAgent
            };
            _core.Set(RssCheckFrequency.High, Setting.Shared.HighInterval);
            _core.Set(RssCheckFrequency.Low, Setting.Shared.LowInterval);
            _core.Received += WhenReceived;

            _checker = new UpdateChecker(Setting);
            Data = new MainBindableData(_core, Setting, dispatcher);
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
        public MainBindableData Data { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Setting
        ///
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingFolder Setting { get; }

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
            Data.Message.Value = Properties.Resources.MessageLoading;

            this.LogDebug(() => _core.Load());

            var entry = _core.Find(Setting.Shared.StartUri) ??
                        _core.Flatten<RssEntry>().FirstOrDefault();
            if (entry != null)
            {
                Select(entry);
                entry.Parent.Expand();
            }

            Debug.Assert(Setting.Shared.InitialDelay.HasValue);
            _core.Start(Setting.Shared.InitialDelay.Value);

            Data.Message.Value = string.Empty;
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
            _core.Suspend();
            try { await _core.RegisterAsync(src.ToUri()); }
            finally { _core.Start(TimeSpan.Zero); }
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
        public void NewCategory() => Select(_core.Create(Data.Current.Value));

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
            _core.Move(src, dest, index);

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// 現在選択中の RSS エントリを削除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Remove() => _core.Remove(Data.Current.Value);

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
        public void Read() => Data.Current.Value.Read();

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
        /* ----------------------------------------------------------------- */
        public void Update()
        {
            Data.Message.Value = Properties.Resources.MessageUpdating;
            _core.Update(Data.Current.Value);
        }

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
            Data.Message.Value = Properties.Resources.MessageUpdating;
            _core.Reset(Data.Current.Value);
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
                _core.Select(Data.LastEntry.Value, current);
                current.Selected = true;
                Data.LastEntry.Value = current;
                Select(current.Items.FirstOrDefault());
                Setting.Shared.StartUri = current.Uri;
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

            Debug.Assert(Data.LastEntry.Value != null);
            var entry = Data.LastEntry.Value;
            Data.Content.Value = entry.SkipContent ? src.Link as object : src;
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
            try
            {
                _core.Stop();
                this.LogDebug(() => _core.Import(path));
            }
            finally { _core.Start(TimeSpan.Zero); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// OPML 形式でエクスポートします。
        /// </summary>
        ///
        /// <param name="path">保存先のパス</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Export(string path) => _core.Export(path);

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
        public void Reschedule(RssEntry src) => _core.Reschedule(src);

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// RSS フィードの監視を停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop() => _core.Stop();

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _core?.Dispose();
                Setting.Dispose();
            }
        }

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
        private void WhenReceived(object s, ValueEventArgs<RssFeed> e)
        {
            var src  = e.Value;
            var dest = _core.Find(src.Uri);

            Debug.Assert(src != null);
            if (dest == null) return;
            if (dest != Data.LastEntry.Value) dest.Shrink();

            var count = dest.Update(src);

            Data.Message.Value = Setting.Shared.EnableMonitorMessage ?
                                 src.ToMessage(count) :
                                 string.Empty;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenSettingChanged
        ///
        /// <summary>
        /// 設定内容変更時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenSettingChanged(object s, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Setting.Shared.HighInterval):
                    _core.Set(RssCheckFrequency.High, Setting.Shared.HighInterval);
                    break;
                case nameof(Setting.Shared.LowInterval):
                    _core.Set(RssCheckFrequency.Low, Setting.Shared.LowInterval);
                    break;
                case nameof(Setting.Shared.CheckUpdate):
                    if (Setting.Shared.CheckUpdate) _checker.Start();
                    else _checker.Stop();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Fields
        private readonly RssSubscriber _core;
        private readonly UpdateChecker _checker;
        #endregion
    }
}
