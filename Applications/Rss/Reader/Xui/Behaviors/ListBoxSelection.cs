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
using System.Collections;
using System.Linq;
using System.Windows.Controls;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// ListBoxSelection
    ///
    /// <summary>
    /// ListBox の SelectionChanged イベントと Command を関連付ける
    /// ための Behavior です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class ListBoxSelection : CommandBehavior<ListBox>
    {
        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnAttached
        /// 
        /// <summary>
        /// 要素へ接続された時に実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += WhenChanged;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnDetaching
        /// 
        /// <summary>
        /// 要素から解除された時に実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnDetaching()
        {
            if (AssociatedObject != null) AssociatedObject.SelectionChanged -= WhenChanged;
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChanged
        /// 
        /// <summary>
        /// SelectionChanged イベント発生時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Command == null) return;
            var args = new SelectionList(e.RemovedItems, e.AddedItems);
            if (Command.CanExecute(args)) Command.Execute(args);
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// SelectionList
    ///
    /// <summary>
    /// 選択項目の変化内容を保持するためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SelectionList
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SelectionList
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="removd">選択解除された項目一覧</param>
        /// <param name="added">選択された項目一覧</param>
        ///
        /* ----------------------------------------------------------------- */
        public SelectionList(IList removd, IList added)
        {
            UnselectedItems = removd;
            SelectedItems   = added;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// SelectedItem
        /// 
        /// <summary>
        /// 選択された項目を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// SelectedItem は SelectedItems の先頭項目を表します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public object SelectedItem => SelectedItems.Cast<object>().FirstOrDefault();

        /* ----------------------------------------------------------------- */
        ///
        /// SelectedItems
        /// 
        /// <summary>
        /// 選択された項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList SelectedItems { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// RemovedItems
        /// 
        /// <summary>
        /// 選択解除された項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList UnselectedItems { get; }

        #endregion
    }
}
