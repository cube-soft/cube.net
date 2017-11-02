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
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// SelectedItemChanged
    ///
    /// <summary>
    /// TreeView の SelectedItemChanged イベントと Command を関連付ける
    /// ための添付ビヘイビアクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SelectedItemChanged : Behavior<TreeView>
    {
        #region DependencyProperty

        /* ----------------------------------------------------------------- */
        ///
        /// CommandProperty
        /// 
        /// <summary>
        /// Command を保持するための DependencyProperty を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(SelectedItemChanged),
                new PropertyMetadata(null, WhenCommandChanged)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// GetCommand
        /// 
        /// <summary>
        /// ICommand オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="obj">イベントの送信者</param>
        /// 
        /// <returns>ICommand オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static ICommand GetCommand(DependencyObject obj)
            => obj.GetValue(CommandProperty) as ICommand;

        /* ----------------------------------------------------------------- */
        ///
        /// SetCommand
        /// 
        /// <summary>
        /// Command オブジェクトを設定します。
        /// </summary>
        ///
        /// <param name="obj">イベントの送信者</param>
        /// <param name="value">コマンド</param>
        /// 
        /* ----------------------------------------------------------------- */
        public static void SetCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(CommandProperty, value);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenCommandChanged
        /// 
        /// <summary>
        /// CommandProperty の内容が変化した時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void WhenCommandChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is TreeView tv)
            {
                if (e.NewValue is ICommand) tv.SelectedItemChanged += WhenSelectedItemChanged;
                else tv.SelectedItemChanged += WhenSelectedItemChanged;
            }
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
        private static void WhenSelectedItemChanged(object s, RoutedPropertyChangedEventArgs<object> e)
        {
            if (s is TreeView tv)
            {
                var cmd = GetCommand(tv);
                if (cmd.CanExecute(e.NewValue)) cmd.Execute(e.NewValue);
            }
        }

        #endregion
    }
}
