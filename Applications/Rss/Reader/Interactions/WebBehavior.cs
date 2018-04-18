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
using CefSharp;
using CefSharp.WinForms;
using Cube.Log;
using Cube.Net.Rss;
using Cube.Xui.Behaviors;
using System;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// WebBehavior
    ///
    /// <summary>
    /// WebControl を拡張するための Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class WebBehavior : WindowsFormsBehavior<ChromiumWebBrowser>
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

                Sync(() =>
                {
                    Source.LoadingStateChanged -= WhenLoading;

                    if (value is Uri uri)
                    {
                        Source.LoadingStateChanged += WhenLoading;
                        Source.LoadHtml(Properties.Resources.Loading);
                    }
                    else if (value is RssItem src)
                    {
                        Source.LoadHtml(string.Format(
                            Properties.Resources.Skeleton,
                            Properties.Resources.SkeletonStyle,
                            src.Link,
                            HttpUtility.HtmlEncode(src.Title),
                            src.PublishTime,
                            !string.IsNullOrEmpty(src.Content) ? src.Content : HttpUtility.HtmlEncode(src.Summary)
                        ));
                    }
                });
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

        #region EnableNewWindow

        /* ----------------------------------------------------------------- */
        ///
        /// EnableNewWindow
        ///
        /// <summary>
        /// 新しいウィンドウをで開くを有効するかどうかを示す値を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool EnableNewWindow
        {
            get => _handler.EnableNewWindow;
            set
            {
                if (_handler.EnableNewWindow == value) return;
                _handler.EnableNewWindow = value;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableNewWindowProperty
        ///
        /// <summary>
        /// EnableNewWindow を保持するための DependencyProperty です。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly DependencyProperty EnableNewWindowProperty =
            DependencyProperty.RegisterAttached(
                nameof(EnableNewWindow),
                typeof(bool),
                typeof(WebBehavior),
                new PropertyMetadata((s, e) =>
                {
                    if (s is WebBehavior wb) wb.EnableNewWindow = (bool)e.NewValue;
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
            get => _hover;
            set { if (_hover != value) _hover = value; }
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
                new PropertyMetadata((s, e) =>
                {
                    if (s is WebBehavior wb) wb.Hover = (ICommand)e.NewValue;
                })
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
            _context = SynchronizationContext.Current;
            if (Source != null)
            {
                Source.LifeSpanHandler = _handler;
                Source.MenuHandler     = new NullMenuHandler();
                Source.StatusMessage  -= WhenMouseOver;
                Source.StatusMessage  += WhenMouseOver;
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
                Source.LifeSpanHandler = null;
                Source.StatusMessage  -= WhenMouseOver;
            }
            base.OnDetaching();
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
        private void WhenLoading(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading) return;
            Sync(() =>
            {
                Source.LoadingStateChanged -= WhenLoading;
                if (Content is Uri uri) Source.Load(uri.ToString());
            });
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
        private void WhenMouseOver(object sender, StatusMessageEventArgs e)
        {
            if (Hover.CanExecute(e.Value)) Hover.Execute(e.Value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Sync
        ///
        /// <summary>
        /// UI スレッドで実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Sync(Action action) => _context.Send(_ => action(), null);

        #endregion

        #region Fields
        private object _content;
        private LifSpanHandler _handler = new LifSpanHandler();
        private ICommand _hover;
        private SynchronizationContext _context;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// LifSpanHandler
    ///
    /// <summary>
    /// ChromiumWebBrowser のポップアップに関する挙動を制御するための
    /// クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal class LifSpanHandler : ILifeSpanHandler
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// EnableNewWindow
        ///
        /// <summary>
        /// 新しいウィンドウをで開くを有効するかどうかを示す値を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool EnableNewWindow { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnBeforePopup
        ///
        /// <summary>
        /// ポップアップ前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool OnBeforePopup(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            string targetUrl,
            string targetFrameName,
            WindowOpenDisposition targetDisposition,
            bool userGesture,
            IPopupFeatures popupFeatures,
            IWindowInfo windowInfo,
            IBrowserSettings browserSettings,
            ref bool noJavascriptAccess,
            out IWebBrowser newBrowser
        )
        {
            newBrowser = null;

            try
            {
                if (EnableNewWindow) System.Diagnostics.Process.Start(targetUrl);
                else browserControl.Load(targetUrl);
            }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
            return true;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DoClose
        ///
        /// <summary>
        /// ブラウザを閉じます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool DoClose(IWebBrowser browserControl, IBrowser browser) => false;

        /* ----------------------------------------------------------------- */
        ///
        /// OnAfterCreated
        ///
        /// <summary>
        /// ブラウザ生成後に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) { }

        /* ----------------------------------------------------------------- */
        ///
        /// OnBeforeClose
        ///
        /// <summary>
        /// ブラウザが閉じる前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) { }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// NullMenuHandler
    ///
    /// <summary>
    /// コンテキストメニューを無効化するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal class NullMenuHandler : IContextMenuHandler
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnBeforeContextMenu
        ///
        /// <summary>
        /// Called before a context menu is displayed.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void OnBeforeContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model)
        {
            model.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnContextMenuCommand
        ///
        /// <summary>
        /// Called to execute a command selected from the context menu.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool OnContextMenuCommand(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            CefMenuCommand commandId,
            CefEventFlags eventFlags) => false;

        /* ----------------------------------------------------------------- */
        ///
        /// OnContextMenuDismissed
        ///
        /// <summary>
        /// Called when the context menu is dismissed irregardless of
        /// whether the menu was empty or a command was selected.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void OnContextMenuDismissed(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RunContextMenu
        ///
        /// <summary>
        /// Called to allow custom display of the context menu.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool RunContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model,
            IRunContextMenuCallback callback) => false;

        #endregion
    }
}
