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
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Cube.Net.Rss;
using Cube.Xui.Behaviors;

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
            Model.Message.Value = "Ready";
            DropTarget = new RssEntryDropTarget((s, d, i) => Model.Items.Move(s, d, i));
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Model
        /// 
        /// <summary>
        /// Model オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssFacade Model { get; }

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
        public ICommand Settings =>
            _settings = _settings ?? new RelayCommand(
                () => Messenger.Send(new SettingsViewModel())
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
        public ICommand Property =>
            _property = _property ?? new RelayCommand(
                () => Messenger.Send(new PropertyViewModel())
            );

        /* ----------------------------------------------------------------- */
        ///
        /// Register
        /// 
        /// <summary>
        /// 新規 URL 登録時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Register =>
            _register = _register ?? new RelayCommand(
                () => Messenger.Send(new RegisterViewModel(e => Model.Register(e)))
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
        public ICommand Remove =>
            _remove = _remove ?? new RelayCommand<object>(
                e => Model.Items.Remove(e)
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
        public ICommand Rename =>
            _rename = _rename ?? new RelayCommand<object>(
                e => { }
            );

        /* ----------------------------------------------------------------- */
        ///
        /// Refresh
        /// 
        /// <summary>
        /// RSS フィード更新時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Refresh =>
            _refresh = _refresh ?? new RelayCommand(
                () => { },
                () => false
            );

        /* ----------------------------------------------------------------- */
        ///
        /// SelectEntry
        /// 
        /// <summary>
        /// RssEntry 選択時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectEntry =>
            _selectEntry = _selectEntry ?? new RelayCommand<object>(
                e => Model.Select(e as RssEntry),
                e => e is RssEntry
            );

        /* ----------------------------------------------------------------- */
        ///
        /// SelectItem
        /// 
        /// <summary>
        /// RssItem 選択時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectItem =>
            _selectItem = _selectItem ?? new RelayCommand<SelectionList>(
                e => Model.Read(e.SelectedItem as RssItem),
                e => e.SelectedItem is RssItem
            );

        #endregion

        #region Fields
        private RelayCommand _settings;
        private RelayCommand _property;
        private RelayCommand _register;
        private RelayCommand _refresh;
        private RelayCommand<object> _remove;
        private RelayCommand<object> _rename;
        private RelayCommand<object> _selectEntry;
        private RelayCommand<SelectionList> _selectItem;
        #endregion
    }
}
