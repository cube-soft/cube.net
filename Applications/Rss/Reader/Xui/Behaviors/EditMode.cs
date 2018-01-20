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
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// EditMode
    ///
    /// <summary>
    /// TextBox の編集可能状態を切り替えるためのクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class EditMode : Behavior<TextBox>
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        /// 
        /// <summary>
        /// 編集可能かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);

                var control = AssociatedObject as TextBox;
                if (control == null) return;
                if (value) Enable(control);
                else Disable(control);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ValueProperty
        /// 
        /// <summary>
        /// Editable を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                nameof(Value),
                typeof(bool),
                typeof(EditMode),
                new PropertyMetadata(false, (s, e) =>
                {
                    if (s is EditMode em) em.Value = (bool)e.NewValue;
                })
            );

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
            if (AssociatedObject is TextBox c)
            {
                if (Value) Enable(c);
                else Disable(c);

                c.LostFocus += WhenLostFocus;
                c.KeyDown   += WhenKeyDown;
            }
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
            if (AssociatedObject is TextBox c)
            {
                c.LostFocus -= WhenLostFocus;
                c.KeyDown   -= WhenKeyDown;
            }
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
        private void WhenLostFocus(object sender, RoutedEventArgs e) => Value = false;

        /* ----------------------------------------------------------------- */
        ///
        /// WhenKeyDown
        /// 
        /// <summary>
        /// キー押下時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) WhenLostFocus(sender, e);
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
            src.Background      = Brushes.White;
            src.BorderBrush     = Brushes.Gray;
            src.BorderThickness = new Thickness(1);
            src.Cursor          = Cursors.IBeam;
            src.Focusable       = true;
            src.IsReadOnly      = false;
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
        public static void Disable(TextBox src)
        {
            src.Select(0, 0);
            src.Background      = Brushes.Transparent;
            src.BorderBrush     = Brushes.Transparent;
            src.BorderThickness = new Thickness(0);
            src.Cursor          = Cursors.Arrow;
            src.IsReadOnly      = true;
            src.Focusable       = false;
        }

        #endregion
    }
}
