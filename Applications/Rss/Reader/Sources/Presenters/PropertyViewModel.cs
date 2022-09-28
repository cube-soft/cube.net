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
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Cube.Xui.Commands.Extensions;
using Cube.Xui;

/* ------------------------------------------------------------------------- */
///
/// PropertyViewModel
///
/// <summary>
/// Represents the ViewModel for the PropertyWindow.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class PropertyViewModel : ViewModelBase<RssEntry>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// PropertyViewModel
    ///
    /// <summary>
    /// Initializes a new instance of the PropertyViewModel class with the
    /// specified arguments.
    /// </summary>
    ///
    /// <param name="callback">Callback action.</param>
    /// <param name="entry">Source RSS entry.</param>
    /// <param name="context">Synchronization context.</param>
    ///
    /* --------------------------------------------------------------------- */
    public PropertyViewModel(Action<RssEntry> callback,
        RssEntry entry,
        SynchronizationContext context
    ) : base(entry, new Aggregator(), context)
    {
        Apply = Get(() => new DelegateCommand(() =>
        {
            Send(new ApplyMessage());
            Close.Execute();
            callback?.Invoke(Value);
        }), nameof(Apply));
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Value
    ///
    /// <summary>
    /// Gets the target RSS entry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public RssEntry Value => Facade;

    /* --------------------------------------------------------------------- */
    ///
    /// Frequencies
    ///
    /// <summary>
    /// Gets a list of objects representing update frequency.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<RssCheckFrequency> Frequencies { get; } =
        Enum.GetValues(typeof(RssCheckFrequency)).Cast<RssCheckFrequency>();

    #endregion

    #region Commands

    /* --------------------------------------------------------------------- */
    ///
    /// Apply
    ///
    /// <summary>
    /// Gets the apply command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Apply { get; }

    #endregion
}
