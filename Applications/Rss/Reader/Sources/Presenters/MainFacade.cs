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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Web.Extensions;

/* ------------------------------------------------------------------------- */
///
/// MainFacade
///
/// <summary>
/// Provides functionality to communicate with the MainViewModel object.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public sealed class MainFacade : DisposableBase
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// MainFacade
    ///
    /// <summary>
    /// Initializes a new instance of the MainFacade class with the specified
    /// arguments.
    /// </summary>
    ///
    /// <param name="src">User settings.</param>
    /// <param name="dispatcher">Context dispatcher.</param>
    ///
    /* --------------------------------------------------------------------- */
    public MainFacade(SettingFolder src, Dispatcher dispatcher)
    {
        Logger.Warn(src.Load);
        Logger.Info($"Owner:{src.Lock.UserName}@{src.Lock.MachineName} ({src.Lock.Sid})");
        Logger.Info($"User-Agent:{src.UserAgent}");

        Setting = src;
        Setting.PropertyChanged += WhenSettingChanged;
        Setting.AutoSave = true;

        var feeds = Io.Combine(Setting.DataDirectory, LocalSetting.FeedFileName);
        var cache = Io.Combine(Setting.DataDirectory, LocalSetting.CacheDirectoryName);

        _core = new RssSubscriber(dispatcher)
        {
            FileName       = feeds,
            CacheDirectory = cache,
            Capacity       = Setting.Shared.Capacity,
            IsReadOnly     = Setting.Lock.IsReadOnly,
            UserAgent      = Setting.UserAgent
        };
        _core.Set(RssCheckFrequency.High, Setting.Shared.HighInterval);
        _core.Set(RssCheckFrequency.Low, Setting.Shared.LowInterval);
        _core.Received += WhenReceived;

        _checker = new UpdateChecker(Setting);
        Data = new MainBindableData(_core, Setting, dispatcher);
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Data
    ///
    /// <summary>
    /// Gets a bindable data.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public MainBindableData Data { get; }

    /* --------------------------------------------------------------------- */
    ///
    /// Setting
    ///
    /// <summary>
    /// Gets the user settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public SettingFolder Setting { get; }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Setup
    ///
    /// <summary>
    /// Invokes the setup operation.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Setup()
    {
        Data.Message.Value = Properties.Resources.MessageLoading;

        Logger.Warn(_core.Load);

        var entry = _core.Find(Setting.Shared.StartUri) ??
                    _core.Flatten<RssEntry>().FirstOrDefault();
        if (entry != null)
        {
            Select(entry);
            entry.Parent.Expand();
        }

        _core.Start(Setting.Shared.InitialDelay);

        Data.Message.Value = string.Empty;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// NewEntry
    ///
    /// <summary>
    /// Registers a new URL.
    /// </summary>
    ///
    /// <param name="src">URL to be registered.</param>
    ///
    /* --------------------------------------------------------------------- */
    public async Task NewEntry(string src)
    {
        _core.Suspend();
        try { await _core.RegisterAsync(src.ToUri()); }
        finally { _core.Start(TimeSpan.Zero); }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// NewCategory
    ///
    /// <summary>
    /// Creates a new category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void NewCategory() => Select(_core.Create(Data.Current.Value));

    /* --------------------------------------------------------------------- */
    ///
    /// Move
    ///
    /// <summary>
    /// Moves the specified RSS entry.
    /// </summary>
    ///
    /// <param name="src">RSS entry of the moving source.</param>
    /// <param name="dest">Destination category.</param>
    /// <param name="index">Insertion position in category.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Move(IRssEntry src, IRssEntry dest, int index) =>
        _core.Move(src, dest, index);

    /* --------------------------------------------------------------------- */
    ///
    /// Remove
    ///
    /// <summary>
    /// Removes the selected RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Remove() => _core.Remove(Data.Current.Value);

    /* --------------------------------------------------------------------- */
    ///
    /// Rename
    ///
    /// <summary>
    /// Sets the selected RSS entry to renameable.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Rename() => Data.Current.Value.Editing = true;

    /* --------------------------------------------------------------------- */
    ///
    /// Read
    ///
    /// <summary>
    /// Sets the all articles of the selected RSS entry as read.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Read() => Data.Current.Value.Read();

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the content of the selected RSS entry. If a category is
    /// specified, the content of all RSS entries in the category is updated.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Update()
    {
        Data.Message.Value = Properties.Resources.MessageUpdating;
        _core.Update(Data.Current.Value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Reset
    ///
    /// <summary>
    /// Clears and reacquires the contents of the RSS feed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Reset()
    {
        Data.Message.Value = Properties.Resources.MessageUpdating;
        _core.Reset(Data.Current.Value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Select
    ///
    /// <summary>
    /// Selects the specified RSS entry.
    /// </summary>
    ///
    /// <param name="src">RSS entry to be selected.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Select(IRssEntry src)
    {
        Data.Current.Value = src;

        if (src is RssEntry current && current != Data.LastEntry.Value)
        {
            _core.Select(Data.LastEntry.Value, current);
            current.Selected = true;
            Data.LastEntry.Value = current;
            Select(current.Items.FirstOrDefault());
            Setting.Shared.StartUri = current.Uri;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Select
    ///
    /// <summary>
    /// Selects the specified RSS item.
    /// </summary>
    ///
    /// <param name="src">RSS item to be selected.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Select(RssItem src)
    {
        if (src == null) return;

        Debug.Assert(Data.LastEntry.Value != null);
        var entry = Data.LastEntry.Value;
        Data.Content.Value = entry.SkipContent ? src.Link as object : src;
        entry.Read(src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Import
    ///
    /// <summary>
    /// Imports the specified OPML format file.
    /// </summary>
    ///
    /// <param name="src">Path of the OPML format file.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Import(string src)
    {
        try
        {
            _core.Stop();
            Logger.Warn(() => _core.Import(src));
        }
        finally { _core.Start(TimeSpan.Zero); }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Export
    ///
    /// <summary>
    /// Exports in OPML format.
    /// </summary>
    ///
    /// <param name="dest">Destination path.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Export(string dest) => _core.Export(dest);

    /* --------------------------------------------------------------------- */
    ///
    /// Reschedule
    ///
    /// <summary>
    /// Resets the frequency of checking RSS feeds.
    /// </summary>
    ///
    /// <param name="src">Target RSS entry.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Reschedule(RssEntry src) => _core.Reschedule(src);

    /* ----------------------------------------------------------------- */
    ///
    /// Stop
    ///
    /// <summary>
    /// Stops monitoring RSS feed.
    /// </summary>
    ///
    /* ----------------------------------------------------------------- */
    public void Stop() => _core.Stop();

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally
    /// releases the managed resources.
    /// </summary>
    ///
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources;
    /// false to release only unmanaged resources.
    /// </param>
    ///
    /* --------------------------------------------------------------------- */
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _core?.Dispose();
            Setting.Dispose();
        }
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// WhenReceived
    ///
    /// <summary>
    /// Occurs when a new article is received.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenReceived(object s, ValueEventArgs<RssFeed> e)
    {
        var src  = e.Value;
        var dest = _core.Find(src.Uri);

        Debug.Assert(src != null);
        if (dest == null) return;
        if (dest != Data.LastEntry.Value) dest.Shrink();

        var count = dest.Update(src);

        Data.Message.Value = Setting.Shared.EnableMonitorMessage ?
                             src.ToMessage(count) :
                             string.Empty;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenSettingChanged
    ///
    /// <summary>
    /// Occurs when the PropertyChanged event is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenSettingChanged(object s, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Setting.Shared.HighInterval):
                _core.Set(RssCheckFrequency.High, Setting.Shared.HighInterval);
                break;
            case nameof(Setting.Shared.LowInterval):
                _core.Set(RssCheckFrequency.Low, Setting.Shared.LowInterval);
                break;
            case nameof(Setting.Shared.CheckUpdate):
                if (Setting.Shared.CheckUpdate) _checker.Start();
                else _checker.Stop();
                break;
            default:
                break;
        }
    }

    #endregion

    #region Fields
    private readonly RssSubscriber _core;
    private readonly UpdateChecker _checker;
    #endregion
}
