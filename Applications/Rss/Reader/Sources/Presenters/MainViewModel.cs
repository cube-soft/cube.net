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
using System.Threading;
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
    public sealed class MainViewModel : GenericViewModel<MainFacade>
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
        public MainViewModel() : this(
            new SettingFolder(Assembly.GetExecutingAssembly()),
            SynchronizationContext.Current
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
        public MainViewModel(SettingFolder settings, SynchronizationContext context) : base(
            new MainFacade(settings, new ContextInvoker(context, false)),
            new Aggregator(),
            context
        ) {
            DropTarget = new RssDropTarget((s, d, i) => Facade.Move(s, d, i))
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
        public MainBindableData Data => Facade.Data;

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
        public ICommand Setup => Get(() => new DelegateCommand(() => Track(Facade.Setup)));

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
                e => Sync(() => Facade.Reschedule(e)),
                Data.Current.Value as RssEntry,
                Context
            )),
            () => !Data.Lock.Value.IsReadOnly && Data.Current.Value is RssEntry
        ).Associate(Data.Current).Associate(Data.Lock));

        /* ----------------------------------------------------------------- */
        ///
        /// Setting
        ///
        /// <summary>
        /// 設定画面表示時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Setting => Get(() => new DelegateCommand(() =>
            Send(new SettingViewModel(Facade.Setting, Context))
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
            () => Send(MessageFactory.Import(), e => Facade.Import(e.First())),
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
            () => Send(MessageFactory.Export(), e => Facade.Export(e))
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
            () => Send(new RegisterViewModel(e => Facade.NewEntry(e), Context)),
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
            () => Sync(Facade.NewCategory),
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
            () => Send(MessageFactory.RemoveWarning(Data.Current.Value.Title),
                e => Facade.Remove(),
                e => e == DialogStatus.Yes),
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
            () => Sync(Facade.Rename),
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
            () => Sync(Facade.Read),
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
            () => Sync(Facade.Update),
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
            () => Sync(Facade.Reset),
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
            e => Sync(() =>
            {
                Facade.Select(e as IRssEntry);
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
            e => Sync(() => Facade.Select(e as RssItem)),
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
        public ICommand Stop => Get(() => new DelegateCommand(() => Sync(Facade.Stop)));

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
            if (disposing) Facade.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
