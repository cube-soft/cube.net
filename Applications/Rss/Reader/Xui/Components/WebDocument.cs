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

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// WebDocument
    ///
    /// <summary>
    /// WebBrowser のドキュメント内容と関連付けるためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class WebDocument
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// TextProperty
        /// 
        /// <summary>
        /// Text を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(WebDocument),
                new PropertyMetadata(OnTextChanged)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// GetText
        /// 
        /// <summary>
        /// Text の内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetText(DependencyObject sender)
            => sender.GetValue(TextProperty) as string;

        /* ----------------------------------------------------------------- */
        ///
        /// SetText
        /// 
        /// <summary>
        /// Text の内容を更新します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void SetText(DependencyObject sender, string value)
            => sender.SetValue(TextProperty, value);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnTextChanged
        /// 
        /// <summary>
        /// TextProperty 変更時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is WebBrowser wb) wb.NavigateToString(e.NewValue as string);
        }

        #endregion
    }
}
