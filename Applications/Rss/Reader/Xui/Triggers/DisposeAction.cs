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
using System.Windows;
using System.Windows.Interactivity;

namespace Cube.Xui.Triggers
{
    /* --------------------------------------------------------------------- */
    ///
    /// DisposeAction
    ///
    /// <summary>
    /// DataContext の開放処理を実行する TriggerAction です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class DisposeAction : TriggerAction<DependencyObject>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        /// 
        /// <summary>
        /// 処理を実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object notused)
        {
            if (AssociatedObject is FrameworkElement e)
            {
                if (e.DataContext is IDisposable dc) dc.Dispose();
                e.DataContext = null;
            }
        }
    }
}
