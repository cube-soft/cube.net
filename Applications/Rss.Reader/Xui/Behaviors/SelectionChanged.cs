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
    /// SelectionChangedChanged
    ///
    /// <summary>
    /// ListBox の SelectionChanged イベントと Command を関連付ける
    /// ための Behavior です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SelectionChanged : CommandBehavior<ListBox>
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
            AssociatedObject.SelectionChanged += WhenSelectionChanged;
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
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged -= WhenSelectionChanged;
            }
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenSelectionChanged
        /// 
        /// <summary>
        /// SelectionChanged イベント発生時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Command == null) return;
            var args = new SelectionList(e.RemovedItems, e.AddedItems);
            if (Command.CanExecute(args)) Command.Execute(args);
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// SelectionChangedChanged
    ///
    /// <summary>
    /// ListBox の SelectionChanged イベントと Command を関連付ける
    /// ための Behavior です。
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
            RemovedItems = removd;
            AddedItems   = added;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// RemovedItems
        /// 
        /// <summary>
        /// 選択解除された項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList RemovedItems { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// AddedItems
        /// 
        /// <summary>
        /// 選択された項目一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList AddedItems { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// SelectedItem
        /// 
        /// <summary>
        /// 選択された項目を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// SelectedItem は AddedItems の先頭項目を表します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public object SelectedItem => AddedItems.Cast<object>().FirstOrDefault();

        #endregion
    }
}
