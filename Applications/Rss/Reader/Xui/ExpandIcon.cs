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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// ExpandIcon
    ///
    /// <summary>
    /// TreeView の ExpandIcon の表示方法を指定するためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class ExpandIcon
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// VisibilityProperty
        /// 
        /// <summary>
        /// Visibility を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty VisibilityProperty
            = DependencyProperty.RegisterAttached(
                "Visibility",
                typeof(Visibility),
                typeof(ExpandIcon),
                new PropertyMetadata(Visibility.Visible, WhenVisibilityChanged)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// GetVisibility
        /// 
        /// <summary>
        /// Visibility の値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static Visibility GetVisibility(DependencyObject obj)
            => (Visibility)obj.GetValue(VisibilityProperty);

        /* ----------------------------------------------------------------- */
        ///
        /// SetVisibility
        /// 
        /// <summary>
        /// Visibility に値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void SetVisibility(DependencyObject obj, Visibility value)
            => obj.SetValue(VisibilityProperty, value);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenVisibilityChanged
        /// 
        /// <summary>
        /// Visibility の変化時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void WhenVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TreeViewItem tvi) tvi.Loaded += WhenLoaded;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenLoaded
        /// 
        /// <summary>
        /// TreeViewItem のロード時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void WhenLoaded(object s, RoutedEventArgs e)
        {
            var tvi = s as TreeViewItem;
            if (tvi == null) return;

            tvi.Loaded -= WhenLoaded;

            var button = Find<ToggleButton>(tvi);
            if (button != null) button.Visibility = GetVisibility(tvi);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Find
        /// 
        /// <summary>
        /// 目的とするオブジェクトを検索します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static T Find<T>(DependencyObject src) where T : DependencyObject
        {
            if (src == null) return default(T);
            if (src is T self) return self;

            var count = VisualTreeHelper.GetChildrenCount(src);
            for (var i = 0; i < count; i++)
            {
                var c = VisualTreeHelper.GetChild(src, i);
                var dest = Find<T>(c);
                if (dest != null) return dest;
            }
            return default(T);
        }

        #endregion
    }
}
