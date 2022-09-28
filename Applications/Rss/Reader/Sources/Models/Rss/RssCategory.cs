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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

/* ------------------------------------------------------------------------- */
///
/// RssCategory
///
/// <summary>
/// Represents the category to which the RSS entry belongs.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class RssCategory : ObservableBase, IRssEntry
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// RssCategory
    ///
    /// <summary>
    /// Initializes a new instance of the RssCategory class with the
    /// specified dispatcher.
    /// </summary>
    ///
    /// <param name="dispatcher">Dispatcher object.
    /// </param>
    ///
    /* --------------------------------------------------------------------- */
    public RssCategory(Dispatcher dispatcher)
    {
        Dispatcher = dispatcher;
        Children   = new BindableCollection<IRssEntry>(dispatcher);
        Children.CollectionChanged += WhenChildrenChanged;
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Parent
    ///
    /// <summary>
    /// Gets or sets the parent element.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IRssEntry Parent
    {
        get => Get<IRssEntry>();
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Title
    ///
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Title
    {
        get => Get(() => string.Empty);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Count
    ///
    /// <summary>
    /// Gets the number of unread items.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public int Count
    {
        get
        {
            if (!_count.HasValue)
            {
                _count = Children.Aggregate(0, (x, i) => x + i.Count);
            }
            return _count.Value;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Expanded
    ///
    /// <summary>
    /// Gets or sets a value indicating whether the tree view is
    /// expanded.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public bool Expanded
    {
        get => Get(() => false);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Editing
    ///
    /// <summary>
    /// Gets or sets a value indicating whether to be editing or not.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public bool Editing
    {
        get => Get(() => false);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Categories
    ///
    /// <summary>
    /// Gets the sequence of sub-categories.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<RssCategory> Categories => Children.OfType<RssCategory>();

    /* --------------------------------------------------------------------- */
    ///
    /// Entries
    ///
    /// <summary>
    /// Gets the sequence of entries.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<RssEntry> Entries => Children.OfType<RssEntry>();

    /* --------------------------------------------------------------------- */
    ///
    /// Children
    ///
    /// <summary>
    /// Gets the collection of child elements.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public BindableCollection<IRssEntry> Children { get; }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// CreateEntry
    ///
    /// <summary>
    /// Creates a new instance of the RssEntry class that the Parent
    /// property is set to the object.
    /// </summary>
    ///
    /// <returns>New RssEntry object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public RssEntry CreateEntry() => new(Dispatcher) { Parent = this };

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases the unmanaged resources used by the object and
    /// optionally releases the managed resources.
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
        if (disposing) Children.CollectionChanged -= WhenChildrenChanged;
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// OnPropertyChanged
    ///
    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Count))
        {
            _count = null;
            if (Parent is RssCategory rc) rc.Refresh(nameof(Count));
        }
        base.OnPropertyChanged(e);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenChildrenChanged
    ///
    /// <summary>
    /// Occurs when the condition of child elements are changed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenChildrenChanged(object s, NotifyCollectionChangedEventArgs e) =>
        Refresh(nameof(Count));

    #region Json

    /* --------------------------------------------------------------------- */
    ///
    /// RssEntry.Json
    ///
    /// <summary>
    /// Represents the data structure for the target JSON.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    internal class Json
    {
        [DataMember] string Title { get; set; }
        [DataMember] IEnumerable<Json> Categories { get; set; }
        [DataMember] IEnumerable<RssEntry.Json> Entries { get; set; }

        public Json(RssCategory src)
        {
            Title      = src.Title;
            Entries    = src.Entries?.Select(e => new RssEntry.Json(e));
            Categories = src.Categories?.Select(e => new Json(e));
        }

        public  RssCategory Convert(Dispatcher dispatcher) => Convert(null, dispatcher);
        public  RssCategory Convert(RssCategory src) => Convert(src, src.Dispatcher);
        private RssCategory Convert(RssCategory src, Dispatcher dispatcher)
        {
            var dest = new RssCategory(dispatcher)
            {
                Title  = Title,
                Parent = src,
            };
            Add(dest, Categories?.Select(e => e.Convert(dest) as IRssEntry));
            Add(dest, Entries.Select(e => e.Convert(dest) as IRssEntry));
            return dest;
        }

        private void Add(RssCategory dest, IEnumerable<IRssEntry> items)
        {
            System.Diagnostics.Debug.Assert(dest != null);
            if (items is not null)
            {
                foreach (var item in items) dest.Children.Add(item);
            }
        }
    }

    #endregion

    #endregion

    #region Fields
    private int? _count;
    #endregion
}
