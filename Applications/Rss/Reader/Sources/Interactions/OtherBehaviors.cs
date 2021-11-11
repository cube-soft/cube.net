﻿/* ------------------------------------------------------------------------- */
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
using System.Windows.Controls;
using System.Windows.Media;
using Cube.Xui.Behaviors;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// ShowRegisterWindowBehavior
    ///
    /// <summary>
    /// RegisterWindow を表示するための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ShowRegisterWindowBehavior : ShowDialogBehavior<RegisterWindow, RegisterViewModel> { }

    /* --------------------------------------------------------------------- */
    ///
    /// ShowPropertyWindowBehavior
    ///
    /// <summary>
    /// PropertyWindow を表示するための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ShowPropertyWindowBehavior : ShowDialogBehavior<PropertyWindow, PropertyViewModel> { }

    /* --------------------------------------------------------------------- */
    ///
    /// ShowSettingWindowAction
    ///
    /// <summary>
    /// SettingWindow を表示するための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ShowSettingWindowBehavior : ShowDialogBehavior<SettingWindow, SettingViewModel> { }

    /* --------------------------------------------------------------------- */
    ///
    /// ScrollToTopMessage
    ///
    /// <summary>
    /// 最上部までスクロールさせるためのメッセージです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ScrollToTopMessage { }

    /* --------------------------------------------------------------------- */
    ///
    /// ScrollToTopBehavior
    ///
    /// <summary>
    /// 最上部までスクロールさせるための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ScrollToTopBehavior : MessageBehavior<ScrollToTopMessage>
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
        protected override void Invoke(ScrollToTopMessage e)
        {
            var outer = VisualTreeHelper.GetChild(AssociatedObject, 0) as Decorator;
            if (outer.Child is ScrollViewer inner) inner.ScrollToTop();
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// SelectDirectoryBehavior
    ///
    /// <summary>
    /// ディレクトリを選択するための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SelectDirectoryBehavior : OpenDirectoryBehavior
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
        protected override void Invoke(OpenDirectoryMessage e)
        {
            base.Invoke(e);
            if (!e.Cancel && AssociatedObject is TextBox tb) tb.Text = e.Value;
        }
    }
}
