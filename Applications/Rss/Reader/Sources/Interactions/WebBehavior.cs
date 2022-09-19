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
namespace Cube.Net.Rss.Reader;

using System;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Cube.Forms;
using Cube.Forms.Controls;
using Cube.Xui.Behaviors;

/* ------------------------------------------------------------------------- */
///
/// WebBehavior
///
/// <summary>
/// Represents the behavior for extending WebControl.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class WebBehavior : WindowsFormsBehavior<WebControl>
{
    #region Properties

    #region Content

    /* --------------------------------------------------------------------- */
    ///
    /// Content
    ///
    /// <summary>
    /// Gets or sets the display content.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// ContentProperty
    ///
    /// <summary>
    /// Gets the DependencyProperty object to hold the Content property.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// EnableNewWindow
    ///
    /// <summary>
    /// Gets or sets a value indicating whether or not open in new window
    /// is enabled.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public bool EnableNewWindow
    {
        get => _enableNewWindow;
        set { if (_enableNewWindow != value) _enableNewWindow = value; }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// EnableNewWindowProperty
    ///
    /// <summary>
    /// Gets the DependencyProperty object to hold the EnableNewWindow
    /// property.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// Hover
    ///
    /// <summary>
    /// Gets or sets the command to be executed during Hover.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Hover
    {
        get => _hover;
        set { if (_hover != value) _hover = value; }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// HoverProperty
    ///
    /// <summary>
    /// Gets the DependencyProperty object to hold the Hover property.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// OnAttached
    ///
    /// <summary>
    /// Occurs when the component is attached.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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

    /* --------------------------------------------------------------------- */
    ///
    /// OnDetaching
    ///
    /// <summary>
    /// Occurs before the component is detached.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnDetaching()
    {
        if (Source != null)
        {
            Source.BeforeNewWindow   -= WhenBeforeNewWindow;
            Source.DocumentCompleted -= WhenDocumentCompleted;
        }
        base.OnDetaching();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenBeforeNewWindow
    ///
    /// <summary>
    /// Occurs before a new window is open.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenBeforeNewWindow(object s, NavigatingEventArgs e) => Logger.Warn(() =>
    {
        e.Cancel = true;
        var uri = new Uri(e.Url);
        if (EnableNewWindow) _ = System.Diagnostics.Process.Start(uri.ToString());
        else Content = uri;
    });

    /* --------------------------------------------------------------------- */
    ///
    /// WhenDocumentCompleted
    ///
    /// <summary>
    /// Occurs when document loading is complete.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenDocumentCompleted(object s, WebBrowserDocumentCompletedEventArgs e)
    {
        Source.Document.MouseOver -= WhenMouseOver;
        Source.Document.MouseOver += WhenMouseOver;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenLoading
    ///
    /// <summary>
    /// Occurs when the loading.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenLoading(object s, WebBrowserDocumentCompletedEventArgs e)
    {
        Source.DocumentCompleted -= WhenLoading;
        if (Content is Uri uri) Source.Navigate(uri);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenMouseOver
    ///
    /// <summary>
    /// Occurs on mouse-over to a Web document.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenMouseOver(object s, HtmlElementEventArgs e)
    {
        if (s is HtmlDocument doc)
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
    private bool _enableNewWindow;
    private ICommand _hover;
    #endregion
}
