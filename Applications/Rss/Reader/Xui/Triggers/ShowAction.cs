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

namespace Cube.Xui.Triggers
{
    /* --------------------------------------------------------------------- */
    ///
    /// ShowAction(TView)
    ///
    /// <summary>
    /// Window を表示する TriggerAction です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class ShowAction<TView> : TriggerAction<DependencyObject>
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
        /// <param name="parameter">DataContext オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
            => new TView { DataContext = parameter }.Show();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ShowDialogAction(TView)
    ///
    /// <summary>
    /// Window を表示する TriggerAction です。
    /// </summary>
    /// 
    /// <remarks>
    /// Window.Show() の代わりに Window.ShowDialog() を実行します。
    /// </remarks>
    /// 
    /* --------------------------------------------------------------------- */
    public class ShowDialogAction<TView> : TriggerAction<DependencyObject>
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
        /// <param name="parameter">DataContext オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
            => new TView { DataContext = parameter }.ShowDialog();
    }
}
