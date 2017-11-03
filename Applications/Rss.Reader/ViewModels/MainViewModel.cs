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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Cube.Net.Rss;
using Cube.Xui;
using Cube.Xui.Behaviors;

namespace Cube.Net.Applications.Rss.Reader
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
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Categories
        /// 
        /// <summary>
        /// RSS フィード購読サイトおよびカテゴリ一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection<RssCategory> Categories => _model.Categories;

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 対象となる Web サイトの RSS フィードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssFeed> Feed => _model.Feed;

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        /// 
        /// <summary>
        /// 対象とする記事の内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Content => _model.Content;

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// SelectEntry
        /// 
        /// <summary>
        /// RssEntry 選択時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RelayCommand<object> SelectEntry
            => _selectEntry = _selectEntry ?? new RelayCommand<object>(
                e => _model.Select(e as RssEntry),
                e => e is RssEntry
            );

        /* ----------------------------------------------------------------- */
        ///
        /// SelectArticle
        /// 
        /// <summary>
        /// RssArticle 選択時に実行されるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RelayCommand<SelectionList> SelectArticle
            => _selectArticle = _selectArticle ?? new RelayCommand<SelectionList>(
                e => _model.Select(e.SelectedItem as RssArticle),
                e => e.SelectedItem is RssArticle
            );

        #endregion

        #region Fields
        private RssFacade _model = new RssFacade();
        private RelayCommand<object> _selectEntry;
        private RelayCommand<SelectionList> _selectArticle;
        #endregion
    }
}
