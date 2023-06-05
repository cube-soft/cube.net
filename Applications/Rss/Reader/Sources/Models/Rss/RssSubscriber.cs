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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Cube.ByteFormat;
using Cube.Collections;
using Cube.FileSystem;
using Cube.Net.Http.Synchronous;
using Cube.Tasks.Extensions;

/* ------------------------------------------------------------------------- */
///
/// RssSubscriber
///
/// <summary>
/// Provides functionality to manage a list of subscription feeds.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public sealed class RssSubscriber : EnumerableBase<IRssEntry>, INotifyCollectionChanged
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// RssSubscriber
    ///
    /// <summary>
    /// Initializes a new instance of the RssSubscriber class with the
    /// specified dispatcher.
    /// </summary>
    ///
    /// <param name="dispatcher">Context dispatcher.</param>
    ///
    /* --------------------------------------------------------------------- */
    public RssSubscriber(Dispatcher dispatcher)
    {
        _context = dispatcher;

        _tree = new BindableCollection<IRssEntry>(dispatcher);
        _tree.CollectionChanged += (s, e) =>
        {
            AutoSaveCore();
            CollectionChanged?.Invoke(this, e);
        };

        _monitors[0] = new RssMonitor { Interval = TimeSpan.FromHours(1) };
        _monitors[0].SubscribeSync((_, e) => Received?.Invoke(this, new(e)));

        _monitors[1] = new RssMonitor { Interval = TimeSpan.FromHours(24) };
        _monitors[1].SubscribeSync((_, e) => Received?.Invoke(this, new(e)));

        _monitors[2] = new RssMonitor(); // for RssCheckFrequency.None
        _monitors[2].SubscribeSync((_, e) => Received?.Invoke(this, new(e)));

        _autosaver.AutoReset = false;
        _autosaver.Interval = 1000.0;
        _autosaver.Elapsed += WhenAutoSaved;
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// FileName
    ///
    /// <summary>
    /// Gets or sets the path to the JSON file where the list of RSS entries
    /// is stored.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string FileName { get; set; }

    /* --------------------------------------------------------------------- */
    ///
    /// Capacity
    ///
    /// <summary>
    /// Gets or sets the maximum number of unread articles to keep in memory.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public int Capacity
    {
        get => _feeds.Capacity;
        set => _feeds.Capacity = value;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// CacheDirectory
    ///
    /// <summary>
    /// Gets or sets the path of the directory for the cache.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string CacheDirectory
    {
        get => _feeds.Directory;
        set => _feeds.Directory = value;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// IsReadOnly
    ///
    /// <summary>
    /// Gets or sets a value indicating whether the cache file should be
    /// used on a read-only basis.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public bool IsReadOnly
    {
        get => _feeds.IsReadOnlyCache;
        set => _feeds.IsReadOnlyCache = value;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// UserAgent
    ///
    /// <summary>
    /// Gets or sets the User-Agent string.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string UserAgent
    {
        get => _monitors.First().UserAgent;
        set { foreach (var mon in _monitors) mon.UserAgent = value; }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Categories
    ///
    /// <summary>
    /// Gets or sets the collection of categories.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<RssCategory> Categories => this.OfType<RssCategory>();

    /* --------------------------------------------------------------------- */
    ///
    /// Entries
    ///
    /// <summary>
    /// Gets a list of RSS entries that do not belong to any category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<RssEntry> Entries => this.OfType<RssEntry>();

    #endregion

    #region Events

    /* --------------------------------------------------------------------- */
    ///
    /// CollectionChanged
    ///
    /// <summary>
    /// Occurs when some state of the collection changes.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /* --------------------------------------------------------------------- */
    ///
    /// Received
    ///
    /// <summary>
    /// Occurs when a new article is received.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public event ValueEventHandler<RssFeed> Received;

    #endregion

    #region Methods

    #region Entry

    /* --------------------------------------------------------------------- */
    ///
    /// Find
    ///
    /// <summary>
    /// Gets the RSS entry corresponding to the specified URL.
    /// </summary>
    ///
    /// <param name="uri">URL</param>
    ///
    /// <returns>RSS entry.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public RssEntry Find(Uri uri) =>
        uri != null && _feeds.ContainsKey(uri) ? _feeds[uri] as RssEntry : null;

    /* --------------------------------------------------------------------- */
    ///
    /// Select
    ///
    /// <summary>
    /// Change the selection status of RSS entries.
    /// </summary>
    ///
    /// <param name="from">
    /// RSS entry that was selected immediately before.
    /// </param>
    ///
    /// <param name="to">RSS entry to be selected.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Select(RssEntry from, RssEntry to)
    {
        if (to   != null) _ = _feeds.Get(to.Uri, true); // lock
        if (from != null) _feeds.Unlock(from.Uri);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Create
    ///
    /// <summary>
    /// Generates and inserts a new category.
    /// </summary>
    ///
    /// <param name="src">Insertion position.</param>
    ///
    /// <returns>New RSS category.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public RssCategory Create(IRssEntry src)
    {
        var parent = src is RssCategory rc ? rc : src?.Parent as RssCategory;
        var dest   = new RssCategory(_context)
        {
            Title   = Properties.Resources.MessageNewCategory,
            Parent  = parent,
            Editing = true,
        };

        var items = parent != null ? parent.Children : _tree;
        var count = parent != null ? parent.Entries.Count() : Entries.Count();
        items.Insert(items.Count - count, dest);
        parent.Expand();

        return dest;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterAsync
    ///
    /// <summary>
    /// Registers a new RSS feed URL asynchronously.
    /// </summary>
    ///
    /// <param name="uri">URL of the RSS feed.</param>
    ///
    /* --------------------------------------------------------------------- */
    public async Task RegisterAsync(Uri uri)
    {
        var rss = await _client.GetAsync(uri).ConfigureAwait(false);
        if (rss == null) throw Error(Properties.Resources.ErrorFeedNotFound);
        if (_feeds.ContainsKey(rss.Uri)) throw Error(Properties.Resources.ErrorFeedExists);

        AddCore(new RssEntry(rss, _context));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Add
    ///
    /// <summary>
    /// Adds new RSS entries.
    /// </summary>
    ///
    /// <param name="src">Collection of RSS entries.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Add(IEnumerable<IRssEntry> src)
    {
        foreach (var item in src)
        {
            if (item is RssCategory rc) AddCore(rc);
            else if (item is RssEntry re) AddCore(re);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Remove
    ///
    /// <summary>
    /// Removes the specified RSS entry.
    /// </summary>
    ///
    /// <param name="src">RSS entry to be removed.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Remove(IRssEntry src)
    {
        if (src is RssCategory rc) RemoveCore(rc);
        else if (src is RssEntry re) RemoveCore(re);
    }

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
    public void Move(IRssEntry src, IRssEntry dest, int index)
    {
        var same = dest != null && src.Parent == dest.Parent;
        var si   = (src.Parent as RssCategory)?.Children ?? _tree;
        if (same && si.IndexOf(src) < index) --index;

        var parent = src is RssEntry && dest is RssCategory ?
                     dest as RssCategory :
                     dest?.Parent as RssCategory;
        var di     = parent?.Children ?? _tree;
        src.Parent = parent;
        _ = si.Remove(src);
        if (index < 0 || index >= di.Count) di.Add(src);
        else di.Insert(index, src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Load
    ///
    /// <summary>
    /// Loads the setting file.
    /// </summary>
    ///
    /// <remarks>
    /// Backup files are generated asynchronously when loading succeeds.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public void Load() {
        var dest = RssExtension.Load(FileName, _context).SelectMany(e =>
            !string.IsNullOrEmpty(e.Title) ?
            new[] { e as IRssEntry } :
            e.Entries.Select(re =>
            {
                re.Parent = null;
                return re as IRssEntry;
            })
        );

        Logger.Debug(string.Format("Load:{0} ({1}) -> {2}",
            FileName,
            new Entity(FileName).Length.ToPrettyBytes(),
            dest.Count()
        ));

        Add(dest);
        if (!IsReadOnly && _feeds.Count > 0) RssExtension.Backup(FileName);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Save
    ///
    /// <summary>
    /// Save the current condition to the setting file.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Save()
    {
        var empty = new RssCategory(_context) { Title = string.Empty };
        foreach (var entry in Entries) empty.Children.Add(entry);
        Categories.Concat(new[] { empty }).Save(FileName);
        Logger.Debug(string.Format("Save:{0} ({1})",
            FileName, new Entity(FileName).Length.ToPrettyBytes()
        ));
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
        var dest = new RssOpml(_context).Load(src, _feeds);
        if (dest.Count() > 0) Add(dest);
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
    public void Export(string dest) => new RssOpml(_context).Save(this, dest);

    #endregion

    #region Monitor

    /* --------------------------------------------------------------------- */
    ///
    /// Start
    ///
    /// <summary>
    /// Starts monitoring.
    /// </summary>
    ///
    /// <param name="delay">Initial delay time.</param>
    ///
    /// <remarks>
    /// _monitors[2] does not start because the RssCheckFrequency of the
    /// monitor is set to None.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public void Start(TimeSpan delay)
    {
        _monitors[0].Start(delay);
        _monitors[1].Start(TimeSpan.FromSeconds(delay.TotalSeconds * 20));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Stop
    ///
    /// <summary>
    /// Stops monitoring.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Stop()
    {
        foreach (var mon in _monitors) mon.Stop();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Suspend
    ///
    /// <summary>
    /// Suspends monitoring.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Suspend()
    {
        foreach (var mon in _monitors) mon.Suspend();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the contents of the RSS feed.
    /// </summary>
    ///
    /// <param name="src">Target RSS entry or category.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Update(IRssEntry src) =>
        Update(src.Flatten<RssEntry>().ToArray());

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the contents of the RSS feed.
    /// </summary>
    ///
    /// <param name="src">Collection of target RSS entries.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Update(params RssEntry[] src)
    {
        var uris = src.Select(e => e.Uri);

        var m0 = uris.Where(e => _monitors[0].Contains(e));
        if (m0.Count() > 0) _monitors[0].Update(m0);

        var m1 = uris.Where(e => _monitors[1].Contains(e));
        if (m1.Count() > 0) _monitors[1].Update(m1);

        var m2 = uris.Where(e => _monitors[2].Contains(e));
        if (m2.Count() > 0) _monitors[2].Update(m2);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Reset
    ///
    /// <summary>
    /// Clears and reacquires the contents of the RSS feed.
    /// </summary>
    ///
    /// <param name="src">Target RSS entry.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Reset(IRssEntry src)
    {
        var entries = src.Flatten<RssEntry>().ToArray();

        foreach (var entry in entries)
        {
            _feeds.Delete(entry.Uri);
            entry.Items.Clear();
            entry.Count = 0;
            entry.LastChecked = null;
        }

        Update(entries);
    }

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
    public void Reschedule(RssEntry src)
    {
        var now  = DateTime.Now;
        var dest = src.IsHighFrequency(now) ? _monitors[0] :
                   src.IsLowFrequency(now)  ? _monitors[1] : _monitors[2];

        foreach (var mon in _monitors)
        {
            if (!mon.Contains(src.Uri)) continue;
            if (mon == dest) return;
            _ = mon.Remove(src.Uri);
            break;
        }

        dest.Register(src.Uri, src.LastChecked);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Set
    ///
    /// <summary>
    /// Sets the interval between checks of RSS feeds.
    /// </summary>
    ///
    /// <param name="kind">Kind of RSS check frequency.</param>
    /// <param name="time">Check interval</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Set(RssCheckFrequency kind, TimeSpan? time)
    {
        var mon = kind == RssCheckFrequency.High ? _monitors[0] :
                  kind == RssCheckFrequency.Low  ? _monitors[1] : null;

        if (mon != null && time.HasValue && !mon.Interval.Equals(time))
        {
            mon.Stop();
            mon.Interval = time.Value;
            mon.Start(time.Value);
        }
    }

    #endregion

    /* --------------------------------------------------------------------- */
    ///
    /// GetEnumerator
    ///
    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    ///
    /// <returns>
    /// Enumerator that can be used to iterate through the collection.
    /// </returns>
    ///
    /* --------------------------------------------------------------------- */
    public override IEnumerator<IRssEntry> GetEnumerator() => _tree.GetEnumerator();

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
            _autosaver.Stop();
            _autosaver.Elapsed -= WhenAutoSaved;

            foreach (var mon in _monitors) mon.Dispose();
            _feeds.Dispose();
        }

        if (!IsReadOnly) Save();
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// AddCore
    ///
    /// <summary>
    /// Adds the specified category and all RSS entries in that category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void AddCore(RssCategory src)
    {
        foreach (var re in src.Entries) AddCore(re);
        foreach (var rc in src.Categories) AddCore(rc);

        src.Children.CollectionChanged -= WhenChildrenChanged;
        src.Children.CollectionChanged += WhenChildrenChanged;

        if (src.Parent == null) _tree.Add(src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// AddCore
    ///
    /// <summary>
    /// Adds the specified RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void AddCore(RssEntry src)
    {
        if (_feeds.ContainsKey(src.Uri)) return;
        _feeds.Add(src);
        if (src.Parent == null) _tree.Add(src);
        Reschedule(src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RemoveCore
    ///
    /// <summary>
    /// Removes the specified category and all RSS entries in the category.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void RemoveCore(RssCategory src)
    {
        src.Children.CollectionChanged -= WhenChildrenChanged;

        foreach (var item in src.Children.ToList())
        {
            if (item is RssCategory c) RemoveCore(c);
            else if (item is RssEntry e) RemoveCore(e);
        }

        if (src.Parent is RssCategory rc) _ = rc.Children.Remove(src);
        else _ = _tree.Remove(src);
        src.Dispose();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RemoveCore
    ///
    /// <summary>
    /// Removes the specified RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void RemoveCore(RssEntry src)
    {
        foreach (var mon in _monitors) _ = mon.Remove(src.Uri);

        _ = _feeds.Remove(src.Uri, true);

        if (src.Parent is RssCategory rc) _ = rc.Children.Remove(src);
        else _ = _tree.Remove(src);
        src.Dispose();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// AutoSaveCore
    ///
    /// <summary>
    /// Invokes the auto save operation.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void AutoSaveCore()
    {
        if (!IsReadOnly)
        {
            _autosaver.Stop();
            _autosaver.Interval = 1000.0;
            _autosaver.Start();
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Error
    ///
    /// <summary>
    /// Converts the an Exception object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private Exception Error(string src) => new ArgumentException(src);

    #endregion

    #region Handlers
    private void WhenChildrenChanged(object s, EventArgs e) => AutoSaveCore();
    private void WhenAutoSaved(object s, EventArgs e) => TaskEx.Run(() => Save()).Forget();
    #endregion

    #region Fields
    private readonly BindableCollection<IRssEntry> _tree;
    private readonly RssCacheDictionary _feeds = new();
    private readonly RssMonitor[] _monitors = new RssMonitor[3];
    private readonly RssClient _client = new();
    private readonly Dispatcher _context;
    private readonly System.Timers.Timer _autosaver = new();
    #endregion
}
