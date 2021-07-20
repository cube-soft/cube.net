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
using Cube.Xui;
using Microsoft.Xaml.Behaviors;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// EditBehavior
    ///
    /// <summary>
    /// TextBox の編集可能状態を切り替えるためのクラスです。
    /// 編集専用の TextBox をあらかじめ用意して置き、通常時は
    /// Visibility.Collapsed で非表示にします。そして、編集時のみ
    /// Visibility.Visible に変更して TextBox を表示させます。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class EditBehavior : Behavior<TextBox>
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Editing
        ///
        /// <summary>
        /// 編集状態かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Editing
        {
            get => (bool)GetValue(EditingProperty);
            set
            {
                SetValue(EditingProperty, value);
                if (AssociatedObject is TextBox tb)
                {
                    if (value) Enable(tb);
                    else Disable(tb);
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EditingProperty
        ///
        /// <summary>
        /// Editing を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty EditingProperty =
            DependencyFactory.Create<EditBehavior, bool>(nameof(Editing), (s, e) => s.Editing = e);

        #endregion

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

            if (Editing) Enable(AssociatedObject);
            else Disable(AssociatedObject);

            AssociatedObject.LostFocus += WhenLostFocus;
            AssociatedObject.KeyDown   += WhenKeyDown;
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
            AssociatedObject.LostFocus -= WhenLostFocus;
            AssociatedObject.KeyDown   -= WhenKeyDown;
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenLostFocus
        ///
        /// <summary>
        /// フォーカスが外れた時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenLostFocus(object s, RoutedEventArgs e) => Editing = false;

        /* ----------------------------------------------------------------- */
        ///
        /// WhenKeyDown
        ///
        /// <summary>
        /// キー押下時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenKeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) WhenLostFocus(s, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Enable
        ///
        /// <summary>
        /// 編集可能な状態に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void Enable(TextBox src)
        {
            src.Visibility = Visibility.Visible;
            src.Focus();
            src.SelectAll();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Disable
        ///
        /// <summary>
        /// 編集不可能な状態に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void Disable(TextBox src)
        {
            src.Select(0, 0);
            src.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
