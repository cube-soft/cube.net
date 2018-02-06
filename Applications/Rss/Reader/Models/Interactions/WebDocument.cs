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
using Cube.Forms;
using Cube.Log;
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
    public class WebDocument : WindowsFormsBehavior<WebControl>
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
            get => _shared as string;
            set
            {
                _shared = value;
                if (Source != null)
                {
                    Source.DocumentCompleted -= WhenLoading;
                    Source.DocumentText = value;
                }
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
        /// <remarks>
        /// URL 経由の Web ページの読み込みは時間を要するので、先に
        /// ローディングページを読み込みます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri
        {
            get => _shared as Uri;
            set
            {
                _shared = value;
                if (value != null && Source != null)
                {
                    if (Source.IsBusy) Source.Stop();
                    Source.DocumentCompleted -= WhenLoading;
                    Source.DocumentCompleted += WhenLoading;
                    Source.DocumentText = Properties.Resources.Loading;
                }
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
            if (Source != null)
            {
                Source.ScriptErrorsSuppressed = true;
                Source.BeforeNewWindow   -= WhenBeforeNewWindow;
                Source.BeforeNewWindow   += WhenBeforeNewWindow;
                Source.DocumentCompleted -= WhenDocumentCompleted;
                Source.DocumentCompleted += WhenDocumentCompleted;
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
            if (Source != null)
            {
                Source.BeforeNewWindow   -= WhenBeforeNewWindow;
                Source.DocumentCompleted -= WhenDocumentCompleted;
            }
            base.OnDetaching();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenBeforeNewWindow
        ///
        /// <summary>
        /// 新しいウィンドウが開く直前に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenBeforeNewWindow(object sender, NavigatingEventArgs e)
        {
            try
            {
                e.Cancel = true;
                var uri = new Uri(e.Url);
                System.Diagnostics.Process.Start(uri.ToString());
            }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
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
        /// WhenLoading
        ///
        /// <summary>
        /// ローディング画面の読み込み完了時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenLoading(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Source.DocumentCompleted -= WhenLoading;
            Source.Navigate(Uri);
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
                var link = node != null && node.TagName.ToLower() == "a" ?
                           node.GetAttribute("href") :
                           string.Empty;

                if (Hover.CanExecute(link)) Hover.Execute(link);
            }
        }

        #endregion

        #region Fields
        private object _shared;
        #endregion
    }
}
