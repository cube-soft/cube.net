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
using Cube.Mixin.Observing;
using Cube.Xui;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainViewModel
    ///
    /// <summary>
    /// メイン画面とモデルを関連付けるための ViewModel です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class MainViewModel : CommonViewModel
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MainViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainViewModel() : this(new SettingsFolder(
            Assembly.GetExecutingAssembly()) { Dispatcher = new Dispatcher(false) }
        ) { }

        /* ----------------------------------------------------------------- */
        ///
        /// MainViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainViewModel(SettingsFolder settings) : base(new Aggregator())
        {
            Model      = new MainFacade(settings, settings.Dispatcher);
            DropTarget = new RssDropTarget((s, d, i) => Model.Move(s, d, i))
            {
                IsReadOnly = Data.Lock.Value.IsReadOnly
            };
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Data
        ///
        /// <summary>
        /// バインド用データを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainBindableData Data => Model.Data;

        /* ----------------------------------------------------------------- */
        ///
        /// DropTarget
        ///
        /// <summary>
        /// ドラッグ&amp;ドロップ時の処理用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssDropTarget DropTarget { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Model
        ///
        /// <summary>
        /// Model オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private MainFacade Model { get; }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Setup
        ///
        /// <summary>
        /// 初期処理を実行するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Setup => Get(() => new DelegateCommand(() => Track(() => Model.Setup())));

        /* ----------------------------------------------------------------- */
        ///
        /// Property
        ///
        /// <summary>
        /// RSS フィードのプロパティ画面表示時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Property => Get(() => new DelegateCommand(
            () => Send(new PropertyViewModel(
                Data.Current.Value as RssEntry,
                e => TrackSync(() => Model.Reschedule(e))
            )),
            () => !Data.Lock.Value.IsReadOnly && Data.Current.Value is RssEntry
        ).Associate(Data.Current).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        ///
        /// <summary>
        /// 設定画面表示時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Settings => Get(() => new DelegateCommand(() =>
            Send(new SettingsViewModel(Model.Settings))
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// RSS フィードをインポートするコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Import => Get(() => new DelegateCommand(
            () =>
            {
                var e = MessageFactory.Import();
                Send(e);
                if (!e.Cancel) TrackSync(() => Model.Import(e.Value.First()));
            },
            () => !Data.Lock.Value.IsReadOnly
        ).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// 現在の RSS フィードをエクスポートするコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Export =>Get(() => new DelegateCommand(
            () =>
            {
                var e = MessageFactory.Export();
                Send(e);
                if (!e.Cancel) TrackSync(() => Model.Export(e.Value));
            }
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// NewEntry
        ///
        /// <summary>
        /// 新規 URL を登録するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand NewEntry => Get(() => new DelegateCommand(
            () => Send(new RegisterViewModel(e => Model.NewEntry(e))),
            () => !Data.Lock.Value.IsReadOnly
        ).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// NewCategory
        ///
        /// <summary>
        /// 新しいカテゴリを追加するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand NewCategory => Get(() => new DelegateCommand(
            () => TrackSync(() => Model.NewCategory()),
            () => !Data.Lock.Value.IsReadOnly
        ).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// フィード削除時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Remove => Get(() => new DelegateCommand(
            () =>
            {
                var e = MessageFactory.RemoveWarning(Data.Current.Value.Title);
                Send(e);
                if (e.Status == DialogStatus.Ok) TrackSync(() => Model.Remove());
            },
            () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
        ).Associate(Data.Current).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Rename
        ///
        /// <summary>
        /// 名前の変更時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Rename => Get(() => new DelegateCommand(
            () => TrackSync(() => Model.Rename()),
            () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
        ).Associate(Data.Current).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        ///
        /// <summary>
        /// 選択中の RSS エントリの全記事を既読に設定するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Read => Get(() => new DelegateCommand(
            () => TrackSync(() => Model.Read()),
            () => Data.Current.Value != null
        ).Associate(Data.Current));

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィード更新時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Update => Get(() => new DelegateCommand(
            () => TrackSync(() => Model.Update()),
            () => Data.Current.Value != null
        ).Associate(Data.Current));

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Reset => Get(() => new DelegateCommand(
            () => TrackSync(() => Model.Reset()),
            () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
        ).Associate(Data.Current).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// IRssEntry を選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Select => Get(() => new DelegateCommand<object>(
            e => TrackSync(() =>
            {
                Model.Select(e as IRssEntry);
                if (e is RssEntry) Send<ScrollToTopMessage>();
            }),
            e => e is IRssEntry
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// SelectArticle
        ///
        /// <summary>
        /// RssItem を選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectArticle => Get(() => new DelegateCommand<object>(
            e => TrackSync(() => Model.Select(e as RssItem)),
            e => e is RssItem
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// Hover
        ///
        /// <summary>
        /// マウスオーバ時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Hover => Get(() => new DelegateCommand<object>(
            e => Data.Message.Value = e.ToString(),
            e => e != null
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// Navigate
        ///
        /// <summary>
        /// URL 先の Web ページを表示するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Navigate => Get(() => new DelegateCommand<Uri>(e => Data.Content.Value = e));

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// RSS フィードの定期取得を停止するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Stop => Get(() => new DelegateCommand(() => TrackSync(() => Model.Stop())));

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing) Model.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
