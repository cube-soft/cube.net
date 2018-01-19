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
using System.Windows.Input;
using System.Windows.Forms;
using Cube.Xui.Behaviors;

namespace Cube.Net.App.Rss.Reader
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
    public class WebDocument : WindowsFormsBehavior<WebBrowser>
    {
        #region Properties

        #region Text

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
                if (Source != null) Source.DocumentText = value;
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
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                nameof(Text),
                typeof(string),
                typeof(WebDocument),
                new PropertyMetadata((s, e) =>
                {
                    if (s is WebDocument d) d.Text = e.NewValue as string;
                })
            );

        #endregion

        #region Uri

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        /// 
        /// <summary>
        /// Web ページの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri
        {
            get => _uri;
            set
            {
                if (_uri == value) return;
                _uri = value;
                if (value != null && Source != null) Source.Navigate(value);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UriProperty
        /// 
        /// <summary>
        /// Uri を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty UriProperty =
            DependencyProperty.RegisterAttached(
                nameof(Uri),
                typeof(Uri),
                typeof(WebDocument),
                new PropertyMetadata((s, e) =>
                {
                    if (s is WebDocument d) d.Uri = e.NewValue as Uri;
                })
            );

        #endregion

        #region Hover

        /* ----------------------------------------------------------------- */
        ///
        /// Hover
        /// 
        /// <summary>
        /// Hover 時に実行されるコマンドを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Hover
        {
            get => GetValue(HoverProperty) as ICommand;
            set => SetValue(HoverProperty, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HoverProperty
        /// 
        /// <summary>
        /// Hover を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty HoverProperty =
            DependencyProperty.RegisterAttached(
                nameof(Hover),
                typeof(ICommand),
                typeof(WebDocument),
                new PropertyMetadata(null)
            );

        #endregion

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
            if (Source != null) Source.DocumentCompleted += WhenDocumentCompleted;
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
            if (Source != null) Source.DocumentCompleted -= WhenDocumentCompleted;
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenDocumentCompleted
        /// 
        /// <summary>
        /// Web ドキュメントの読み込み完了時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Source.Document.MouseOver -= WhenMouseOver;
            Source.Document.MouseOver += WhenMouseOver;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMouseOver
        /// 
        /// <summary>
        /// Web ドキュメントへのマウスオーバ時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMouseOver(object sender, HtmlElementEventArgs e)
        {
            if (sender is HtmlDocument doc)
            {
                var node = doc.GetElementFromPoint(e.ClientMousePosition);
                var link = node?.GetAttribute("href") ?? string.Empty;
                if (Hover.CanExecute(link)) Hover.Execute(link);
            }
        }

        #endregion

        #region Fields
        private string _text;
        private Uri _uri;
        #endregion
    }
}
