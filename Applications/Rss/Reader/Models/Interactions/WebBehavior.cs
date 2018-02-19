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
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using Cube.Forms;
using Cube.Log;
using Cube.Net.Rss;
using Cube.Xui.Behaviors;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// WebBehavior
    ///
    /// <summary>
    /// WebBrowser のドキュメント内容と関連付けるための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class WebBehavior : WindowsFormsBehavior<WebControl>
    {
        #region Properties

        #region Content

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        ///
        /// <summary>
        /// 表示内容を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Content
        {
            get => _content;
            set
            {
                _content = value;
                Source.DocumentCompleted -= WhenLoading;

                if (value is Uri uri)
                {
                    if (Source.IsBusy) Source.Stop();
                    Source.DocumentCompleted += WhenLoading;
                    Source.DocumentText = Properties.Resources.Loading;
                }
                else if (value is RssItem src)
                {
                    Source.DocumentText = string.Format(
                        Properties.Resources.Skeleton,
                        Properties.Resources.SkeletonStyle,
                        src.Link,
                        HttpUtility.HtmlEncode(src.Title),
                        src.PublishTime,
                        !string.IsNullOrEmpty(src.Content) ? src.Content : HttpUtility.HtmlEncode(src.Summary)
                    );
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ContentProperty
        ///
        /// <summary>
        /// 表示内容を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                nameof(Content),
                typeof(object),
                typeof(WebBehavior),
                new PropertyMetadata((s, e) =>
                {
                    if (s is WebBehavior wb) wb.Content = e.NewValue;
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
                typeof(WebBehavior),
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
        private void WhenBeforeNewWindow(object sender, NavigatingEventArgs e) =>
            this.LogWarn(() =>
        {
            e.Cancel = true;
            var uri = new Uri(e.Url);
            System.Diagnostics.Process.Start(uri.ToString());
        });

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
            if (Content is Uri uri) Source.Navigate(uri);
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
        private object _content;
        #endregion
    }
}
