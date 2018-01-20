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
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Cube.Net.Rss;

namespace Cube.Net.App.Rss.Reader
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
    public class MainViewModel : CommonViewModel
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
        public MainViewModel() : this(new SettingsFolder()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// MainViewModel
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainViewModel(SettingsFolder settings) : base()
        {
            Model = new RssFacade(settings);
            DropTarget = new RssEntryDropTarget((s, d, i) => Model.Move(s, d, i));
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
        public RssBindableData Data => Model.Data;

        /* ----------------------------------------------------------------- */
        ///
        /// DropTarget
        /// 
        /// <summary>
        /// ドラッグ&amp;ドロップ時の処理用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntryDropTarget DropTarget { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Model
        /// 
        /// <summary>
        /// Model オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected RssFacade Model { get; }

        #endregion

        #region Commands

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
                () => Messenger.Send(new SettingsViewModel())
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
            _property = new RelayCommand(
                () => Messenger.Send(new PropertyViewModel())
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
            _newEntry = new RelayCommand(
                () => Messenger.Send(new RegisterViewModel(e => Model.NewEntry(e)))
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
            _newCategory = new RelayCommand(() => Model.NewCategory())
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
            _remove = new RelayCommand(() => Model.Remove())
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
            _rename = new RelayCommand(() => Model.Rename())
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
        public RelayCommand Update => _update ?? (
            _update = new RelayCommand(() => Model.Update(Data.Entry.Value))
        );

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateOne
        /// 
        /// <summary>
        /// RSS フィード更新時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand UpdateOne => _updateOne ?? (
            _updateOne = new RelayCommand(() => Model.Update(Data.Feed.Value))
        );

        /* ----------------------------------------------------------------- */
        ///
        /// ReadAll
        /// 
        /// <summary>
        /// 全ての記事を既読に設定するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand ReadAll => _readAll ?? (
            _readAll = new RelayCommand(() => Model.ReadAll())
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
            _reset = new RelayCommand(() => Model.Reset())
        );

        /* ----------------------------------------------------------------- */
        ///
        /// SelectEntry
        /// 
        /// <summary>
        /// RssEntry を選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectEntry => _selectEntry ?? (
            _selectEntry = new RelayCommand<object>(
                e => Model.Select(e as RssEntryBase),
                e => e is RssEntryBase
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
                e => Model.Select(e as RssItem),
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
            _navigate = new RelayCommand<Uri>(e => Data.Uri.Value = e)
        );

        #endregion

        #region Fields
        private RelayCommand _settings;
        private RelayCommand _property;
        private RelayCommand _newEntry;
        private RelayCommand _newCategory;
        private RelayCommand _update;
        private RelayCommand _updateOne;
        private RelayCommand _readAll;
        private RelayCommand _reset;
        private RelayCommand _remove;
        private RelayCommand _rename;
        private RelayCommand<object> _selectEntry;
        private RelayCommand<object> _selectArticle;
        private RelayCommand<object> _hover;
        private RelayCommand<Uri> _navigate;
        #endregion
    }
}
