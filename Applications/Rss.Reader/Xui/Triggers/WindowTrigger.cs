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
using System.Windows.Interactivity;
using GalaSoft.MvvmLight.Messaging;

namespace Cube.Xui.Triggers
{
    /* --------------------------------------------------------------------- */
    ///
    /// WindowTrigger(T)
    ///
    /// <summary>
    /// Window を表示する Trigger の基底クラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// 引数で指定された型で Messenger.Default に登録されます。
    /// </remarks>
    /// 
    /* --------------------------------------------------------------------- */
    public abstract class WindowTrigger<TDataContext> : TriggerBase<DependencyObject>
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
            Messenger.Default.Register<TDataContext>(AssociatedObject, e => InvokeActions(e));
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
            Messenger.Default.Unregister<TDataContext>(AssociatedObject);
            base.OnDetaching();
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WindowAction(TView)
    ///
    /// <summary>
    /// Window を表示する Trigger の基底クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class WindowAction<TView> : TriggerAction<DependencyObject>
        where TView : Window, new()
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        /// 
        /// <summary>
        /// 処理を実行します。
        /// </summary>
        /// 
        /// <param name="parameter">DataContext</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
            => new TView { DataContext = parameter }.Show();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WindowAction(TView)
    ///
    /// <summary>
    /// Window を表示する Trigger の基底クラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// Window.Show() の代わりに Window.ShowDialog() を実行します。
    /// </remarks>
    /// 
    /* --------------------------------------------------------------------- */
    public class DialogAction<TView> : TriggerAction<DependencyObject>
        where TView : Window, new()
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        /// 
        /// <summary>
        /// 処理を実行します。
        /// </summary>
        /// 
        /// <param name="parameter">DataContext</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
            => new TView { DataContext = parameter }.ShowDialog();
    }
}
