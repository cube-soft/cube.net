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
using Cube.Tasks;
using Cube.Xui;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cube.Net.Rss.App.Reader
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
        public MainViewModel() : this(new SettingsFolder(Assembly.GetExecutingAssembly())) { }

        /* ----------------------------------------------------------------- */
        ///
        /// MainViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainViewModel(SettingsFolder settings) : base(new Messenger())
        {
            Model      = new MainFacade(settings, SynchronizationContext.Current);
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
        public ICommand Setup => _setup ?? (
            _setup = new RelayCommand(
                () => Task.Run(() => Send(() => Model.Setup())).Forget()
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Property
        ///
        /// <summary>
        /// RSS フィードのプロパティ画面表示時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Property => _property ?? (
            _property = new BindableCommand(
                () => Send(new PropertyViewModel(
                    Data.Current.Value as RssEntry,
                    e => Send(() => Model.Reschedule(e))
                )),
                () => !Data.Lock.Value.IsReadOnly && Data.Current.Value is RssEntry,
                Data.Current,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        ///
        /// <summary>
        /// 設定画面表示時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Settings => _settings ?? (
            _settings = new RelayCommand(
                () => Send(new SettingsViewModel(Model.Settings))
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// RSS フィードをインポートするコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Import => _import ?? (
            _import = new BindableCommand(
                () => Send(MessageFactory.Import(e =>
                {
                    if (e.Result) Send(() => Model.Import(e.FileName));
                })),
                () => !Data.Lock.Value.IsReadOnly,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// 現在の RSS フィードをエクスポートするコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Export => _export ?? (
            _export = new RelayCommand(
                () => Send(MessageFactory.Export(e =>
                {
                    if (e.Result) Send(() => Model.Export(e.FileName));
                }))
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// NewEntry
        ///
        /// <summary>
        /// 新規 URL を登録するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand NewEntry => _newEntry ?? (
            _newEntry = new BindableCommand(
                () => Send(new RegisterViewModel(e => Model.NewEntry(e))),
                () => !Data.Lock.Value.IsReadOnly,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// NewCategory
        ///
        /// <summary>
        /// 新しいカテゴリを追加するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand NewCategory => _newCategory ?? (
            _newCategory = new BindableCommand(
                () => Send(() => Model.NewCategory()),
                () => !Data.Lock.Value.IsReadOnly,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// フィード削除時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Remove => _remove ?? (
            _remove = new BindableCommand(
                () => Send(
                    MessageFactory.RemoveWarning(Data.Current.Value.Title, e =>
                    {
                        if (e.Result == MessageBoxResult.OK) Send(() => Model.Remove());
                    })
                ),
                () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null,
                Data.Current,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Rename
        ///
        /// <summary>
        /// 名前の変更時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Rename => _rename ?? (
            _rename = new BindableCommand(
                () => Send(() => Model.Rename()),
                () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null,
                Data.Current,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        ///
        /// <summary>
        /// 選択中の RSS エントリの全記事を既読に設定するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Read => _read ?? (
            _read = new BindableCommand(
                () => Send(() => Model.Read()),
                () => Data.Current.Value != null,
                Data.Current
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS フィード更新時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Update => _update ?? (
            _update = new BindableCommand(
                () => Send(() => Model.Update()),
                () => Data.Current.Value != null,
                Data.Current
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// RSS フィードの内容をクリアし、再取得するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Reset => _reset ?? (
            _reset = new BindableCommand(
                () => Send(() => Model.Reset()),
                () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null,
                Data.Current,
                Data.Lock
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Select
        ///
        /// <summary>
        /// IRssEntry を選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Select => _select ?? (
            _select = new RelayCommand<object>(
                e => Send(() =>
                {
                    Model.Select(e as IRssEntry);
                    if (e is RssEntry) Send<ScrollToTopMessage>();
                }),
                e => e is IRssEntry
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// SelectArticle
        ///
        /// <summary>
        /// RssItem を選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectArticle => _selectArticle ?? (
            _selectArticle = new RelayCommand<object>(
                e => Send(() => Model.Select(e as RssItem)),
                e => e is RssItem
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Hover
        ///
        /// <summary>
        /// マウスオーバ時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Hover => _hover ?? (
            _hover = new RelayCommand<object>(
                e => Data.Message.Value = e.ToString(),
                e => e != null
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Navigate
        ///
        /// <summary>
        /// URL 先の Web ページを表示するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Navigate => _navigate ?? (
            _navigate = new RelayCommand<Uri>(e => Data.Content.Value = e)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// RSS フィードの定期取得を停止するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Stop => _stop ?? (
            _stop = new RelayCommand(
                () => Send(() => Model.Stop())
            )
        );

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

        #region Fields
        private ICommand _setup;
        private ICommand _property;
        private ICommand _settings;
        private ICommand _import;
        private ICommand _export;
        private ICommand _newEntry;
        private ICommand _newCategory;
        private ICommand _update;
        private ICommand _read;
        private ICommand _reset;
        private ICommand _remove;
        private ICommand _rename;
        private ICommand _select;
        private ICommand _selectArticle;
        private ICommand _hover;
        private ICommand _navigate;
        private ICommand _stop;
        #endregion
    }
}
