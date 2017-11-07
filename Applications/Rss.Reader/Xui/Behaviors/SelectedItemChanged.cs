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
using System.Windows;
using System.Windows.Controls;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// SelectedItemChanged
    ///
    /// <summary>
    /// TreeView の SelectedItemChanged イベントと Command を関連付ける
    /// ための Behavior クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SelectedItemChanged : CommandBehavior<TreeView>
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
            AssociatedObject.SelectedItemChanged += WhenSelectedItemChanged;
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
                AssociatedObject.SelectedItemChanged -= WhenSelectedItemChanged;
            }
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenSelectedItemChanged
        /// 
        /// <summary>
        /// SelectedItemChanged イベント発生時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenSelectedItemChanged(object s, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Command == null) return;
            if (Command.CanExecute(e.NewValue)) Command.Execute(e.NewValue);
        }

        #endregion
    }
}
