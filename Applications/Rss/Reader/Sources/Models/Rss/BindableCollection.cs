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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cube.Generics.Extensions;

/* ------------------------------------------------------------------------- */
///
/// BindableCollection(T)
///
/// <summary>
/// Provides functionality to binding a collection.
/// </summary>
///
/// <remarks>
/// ObservableCollection(T) で発生する PropertyChanged および
/// CollectionChanged イベントをコンストラクタで指定された同期
/// コンテキストを用いて伝搬させます。
/// </remarks>
///
/* ------------------------------------------------------------------------- */
public class BindableCollection<T> : ObservableCollection<T>, IDisposable
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// BindableCollection
    ///
    /// <summary>
    /// Initializes a new instance of the <c>BindableCollection</c>
    /// class.
    /// </summary>
    ///
    /// <param name="dispatcher">Dispatcher object.</param>
    ///
    /* --------------------------------------------------------------------- */
    public BindableCollection(Dispatcher dispatcher) : this(default, dispatcher) { }

    /* --------------------------------------------------------------------- */
    ///
    /// BindableCollection
    ///
    /// <summary>
    /// Initializes a new instance of the <c>BindableCollection</c>
    /// class with the specified collection.
    /// </summary>
    ///
    /// <param name="collection">Collection to be copied.</param>
    /// <param name="dispatcher">Dispatcher object.</param>
    ///
    /* --------------------------------------------------------------------- */
    public BindableCollection(IEnumerable<T> collection, Dispatcher dispatcher) :
        base(collection ?? Enumerable.Empty<T>())
    {
        _dispose   = new OnceAction<bool>(Dispose);
        Dispatcher = dispatcher;
        SetHandler(this);
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Dispatcher
    ///
    /// <summary>
    /// Gets or sets the dispatcher object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public Dispatcher Dispatcher { get; set; }

    /* --------------------------------------------------------------------- */
    ///
    /// IsRedirected
    ///
    /// <summary>
    /// Gets a value indicating whether the element's events should be
    /// redirected.
    /// </summary>
    ///
    /// <remarks>
    /// If the property is set to true, when the PropertyChanged event of
    /// an element occurs, the CollectionChanged event is fired with
    /// NotifyCollectionChangedAction.Replace.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public bool IsRedirected { get; set; } = false;

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// ~BindableCollection
    ///
    /// <summary>
    /// Finalizes the BindableCollection.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    ~BindableCollection() { _dispose.Invoke(false); }

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases all resources used by the object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Dispose()
    {
        _dispose.Invoke(true);
        GC.SuppressFinalize(this);
    }

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
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) UnsetHandler(this);
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// OnPropertyChanged
    ///
    /// <summary>
    /// Occurs when the PropertyChanged event is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnPropertyChanged(PropertyChangedEventArgs e) =>
        Dispatcher.Invoke(() => base.OnPropertyChanged(e));

    /* --------------------------------------------------------------------- */
    ///
    /// OnCollectionChanged
    ///
    /// <summary>
    /// Occurs when the CollectionChanged event is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) =>
        Dispatcher.Invoke(() => OnCollectionChangedCore(e));

    /* --------------------------------------------------------------------- */
    ///
    /// OnCollectionChangedCore
    ///
    /// <summary>
    /// Raises the CollectionChanged event.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void OnCollectionChangedCore(NotifyCollectionChangedEventArgs e) => Logger.Warn(() =>
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                SetHandler(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                UnsetHandler(e.OldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                UnsetHandler(e.OldItems);
                SetHandler(e.NewItems);
                break;
            default:
                break;
        }
        base.OnCollectionChanged(e);
    });

    /* --------------------------------------------------------------------- */
    ///
    /// SetHandler
    ///
    /// <summary>
    /// Sets the handler to each specified item.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void SetHandler(IList items)
    {
        foreach (var item in items)
        {
            if (item is INotifyPropertyChanged e)
            {
                e.PropertyChanged -= WhenMemberChanged;
                e.PropertyChanged += WhenMemberChanged;
            }
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// UnsetHandler
    ///
    /// <summary>
    /// Removes the handler from each specified item.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void UnsetHandler(IList items)
    {
        foreach (var item in items)
        {
            if (item is INotifyPropertyChanged e) e.PropertyChanged -= WhenMemberChanged;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenMemberChanged
    ///
    /// <summary>
    /// Occurs when the PropertyChanged event of an element is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenMemberChanged(object s, PropertyChangedEventArgs e)
    {
        if (!IsRedirected) return;
        var index = IndexOf(s.TryCast<T>());
        if (index < 0) return;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace, s, s, index
        ));
    }

    #endregion

    #region Fields
    private readonly OnceAction<bool> _dispose;
    #endregion
}
