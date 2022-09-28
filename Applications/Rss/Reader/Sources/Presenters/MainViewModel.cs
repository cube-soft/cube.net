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
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Cube.Observable.Extensions;
using Cube.Xui;

/* ------------------------------------------------------------------------- */
///
/// MainViewModel
///
/// <summary>
/// Represents the ViewModel for the MainWindow.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public sealed class MainViewModel : ViewModelBase<MainFacade>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// MainViewModel
    ///
    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public MainViewModel() : this(new(typeof(App).Assembly), SynchronizationContext.Current) { }

    /* --------------------------------------------------------------------- */
    ///
    /// MainViewModel
    ///
    /// <summary>
    /// Initializes a new instance of the MainViewModel class with the
    /// specified arguments.
    /// </summary>
    ///
    /// <param name="settings">User settings.</param>
    /// <param name="context">Synchronization context.</param>
    ///
    /* --------------------------------------------------------------------- */
    public MainViewModel(SettingFolder settings, SynchronizationContext context) : base(
        new MainFacade(settings, new ContextDispatcher(context, false)),
        new Aggregator(),
        context
    ) {
        DropTarget = new RssDropTarget((s, d, i) => Facade.Move(s, d, i))
        {
            IsReadOnly = Data.Lock.Value.IsReadOnly
        };
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Data
    ///
    /// <summary>
    /// Gets the bindable data.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public MainBindableData Data => Facade.Data;

    /* --------------------------------------------------------------------- */
    ///
    /// DropTarget
    ///
    /// <summary>
    /// Gets an object for processing during drag and drop.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public RssDropTarget DropTarget { get; }

    #endregion

    #region Commands

    /* --------------------------------------------------------------------- */
    ///
    /// Setup
    ///
    /// <summary>
    /// Gets the Setup command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Setup => Get(() => new DelegateCommand(() => Run(Facade.Setup, false)));

    /* --------------------------------------------------------------------- */
    ///
    /// Property
    ///
    /// <summary>
    /// Gets the command to show the RSS feed property dialog.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Property => Get(() => new DelegateCommand(
        () => Send(new PropertyViewModel(
            e => Run(() => Facade.Reschedule(e), true),
            Data.Current.Value as RssEntry,
            Context
        )),
        () => !Data.Lock.Value.IsReadOnly && Data.Current.Value is RssEntry
    ).Hook(Data.Current).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Setting
    ///
    /// <summary>
    /// Gets the command to show the user setting dialog.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Setting => Get(() => new DelegateCommand(() =>
        Send(new SettingViewModel(Facade.Setting, Context))
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// Import
    ///
    /// <summary>
    /// Gets the command to import RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Import => Get(() => new DelegateCommand(
        () => Send(Message.Import(), e => Facade.Import(e.First()), false),
        () => !Data.Lock.Value.IsReadOnly
    ).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Export
    ///
    /// <summary>
    /// Gets the command to export RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Export =>Get(() => new DelegateCommand(
        () => Send(Message.Export(), e => Facade.Export(e), false)
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// NewEntry
    ///
    /// <summary>
    /// Gets the command to register a new URL.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand NewEntry => Get(() => new DelegateCommand(
        () => Send(new RegisterViewModel(e => Facade.NewEntry(e), Context)),
        () => !Data.Lock.Value.IsReadOnly
    ).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// NewCategory
    ///
    /// <summary>
    /// Gets the command to create a new category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand NewCategory => Get(() => new DelegateCommand(
        () => Run(Facade.NewCategory, true),
        () => !Data.Lock.Value.IsReadOnly
    ).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Remove
    ///
    /// <summary>
    /// Gets the command to remove the selected RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Remove => Get(() => new DelegateCommand(
        () => {
            var m = Message.RemoveWarning(Data.Current.Value.Title);
            Send(m);
            if (m.Value == DialogStatus.Yes) Facade.Remove();
        },
        () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
    ).Hook(Data.Current).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Rename
    ///
    /// <summary>
    /// Gets the command to rename the selected RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Rename => Get(() => new DelegateCommand(
        () => Run(Facade.Rename, true),
        () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
    ).Hook(Data.Current).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Read
    ///
    /// <summary>
    /// Gets the command to set all articles in the selected RSS entry as read.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Read => Get(() => new DelegateCommand(
        () => Run(Facade.Read, true),
        () => Data.Current.Value != null
    ).Hook(Data.Current));

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Gets the command to update the selected RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Update => Get(() => new DelegateCommand(
        () => Run(Facade.Update, true),
        () => Data.Current.Value != null
    ).Hook(Data.Current));

    /* --------------------------------------------------------------------- */
    ///
    /// Reset
    ///
    /// <summary>
    /// Gets a command to clear and reacquire the contents of the selected
    /// RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Reset => Get(() => new DelegateCommand(
        () => Run(Facade.Reset, true),
        () => !Data.Lock.Value.IsReadOnly && Data.Current.Value != null
    ).Hook(Data.Current).Hook(Data.Lock));

    /* --------------------------------------------------------------------- */
    ///
    /// Select
    ///
    /// <summary>
    /// Gets the command to select an RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Select => Get(() => new DelegateCommand<object>(
        e => Run(() =>
        {
            Facade.Select(e as IRssEntry);
            if (e is RssEntry) Send(new ScrollToTopMessage());
        }, true),
        e => e is IRssEntry
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// SelectArticle
    ///
    /// <summary>
    /// Gets the command to select an RSS item.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand SelectArticle => Get(() => new DelegateCommand<object>(
        e => Run(() => Facade.Select(e as RssItem), true),
        e => e is RssItem
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// Hover
    ///
    /// <summary>
    /// Gets the command to hover the mouse.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Hover => Get(() => new DelegateCommand<object>(
        e => Data.Message.Value = e.ToString(),
        e => e != null
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// Navigate
    ///
    /// <summary>
    /// Gets the command to navigate to the contents of the specified URL.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Navigate => Get(() => new DelegateCommand<Uri>(e => Data.Content.Value = e));

    /* --------------------------------------------------------------------- */
    ///
    /// Stop
    ///
    /// <summary>
    /// Gets the command to stop the periodic retrieval of RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Stop => Get(() => new DelegateCommand(() => Run(Facade.Stop, true)));

    #endregion
}
