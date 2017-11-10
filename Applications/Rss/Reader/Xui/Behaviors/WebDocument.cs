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
using System.Windows.Interactivity;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// WebDocument
    ///
    /// <summary>
    /// WebBrowser のドキュメント内容と関連付けるための Behavior です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class WebDocument : Behavior<WebBrowser>
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Text
        /// 
        /// <summary>
        /// ドキュメント内容を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                AssociatedObject.NavigateToString(value);
            }
        }

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
                nameof(Text),
                typeof(string),
                typeof(WebDocument),
                new UIPropertyMetadata(null, OnTextChanged)
            );

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
            if (sender is WebDocument doc) doc.Text = e.NewValue as string;
        }

        #region Fields
        private string _text;
        #endregion

        #endregion
    }
}
