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
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// CommandBehavior(T)
    ///
    /// <summary>
    /// イベントと Command を関連付ける Behavior の基底クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public abstract class CommandBehavior<T> : Behavior<T> where T : DependencyObject
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Command
        /// 
        /// <summary>
        /// SelectedItemChanged イベント発生時に実行されるコマンドを
        /// 取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Command
        {
            get => GetValue(CommandProperty) as ICommand;
            set => SetValue(CommandProperty, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CommandProperty
        /// 
        /// <summary>
        /// Command を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.RegisterAttached(
                nameof(Command),
                typeof(ICommand),
                typeof(CommandBehavior<T>),
                new UIPropertyMetadata(null)
            );

        #endregion
    }
}
