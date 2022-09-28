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
namespace Cube;

using System;
using System.ComponentModel;
using System.Threading;
using Cube.Collections;
using Microsoft.Win32;

/* ------------------------------------------------------------------------- */
///
/// Power
///
/// <summary>
/// Provides functionality to observe power mode of the computer.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public static class Power
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// Power
    ///
    /// <summary>
    /// Initializes a static fields of the Power class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    static Power()
    {
        _context = new(PowerModes.Resume);
        _context.PropertyChanged += WhenPropertyChanged;
        SystemEvents.PowerModeChanged += WhenModeChanged;
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Mode
    ///
    /// <summary>
    /// Gets the power mode.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static PowerModes Mode => _context.Mode;

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Subscribe
    ///
    /// <summary>
    /// Registers the callback to subscription.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static IDisposable Subscribe(Action callback) => _subscription.Subscribe(callback);

    /* --------------------------------------------------------------------- */
    ///
    /// Configure
    ///
    /// <summary>
    /// Sets the PowerModeContext object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static void Configure(PowerModeContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        Interlocked.Exchange(ref _context, context).PropertyChanged -= WhenPropertyChanged;
        context.PropertyChanged -= WhenPropertyChanged;
        context.PropertyChanged += WhenPropertyChanged;
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// WhenPropertyChanged
    ///
    /// <summary>
    /// Occurs when the PropertyChanged event is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static void WhenPropertyChanged(object s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(_context.Mode)) return;
        foreach (var callback in _subscription) callback();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// WhenModeChanged
    ///
    /// <summary>
    /// Occurs when the PowerModeChanged event is fired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static void WhenModeChanged(object s, PowerModeChangedEventArgs e) =>
        _context.Mode = (PowerModes)(int)e.Mode;

    #endregion

    #region Fields
    private static readonly Subscription<Action> _subscription = new();
    private static PowerModeContext _context;
    #endregion
}

/* ------------------------------------------------------------------------- */
///
/// PowerModeContext
///
/// <summary>
/// Represents the power mode.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class PowerModeContext : ObservableBase
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// PowerModeContext
    ///
    /// <summary>
    /// Initializes a new instance of the PowerModeContext class
    /// with the specified value.
    /// </summary>
    ///
    /// <param name="mode">Initial value for power mode.</param>
    ///
    /* --------------------------------------------------------------------- */
    public PowerModeContext(PowerModes mode) => Mode = mode;

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Mode
    ///
    /// <summary>
    /// Gets or sets the power mode.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public PowerModes Mode
    {
        get => Get<PowerModes>();
        set
        {
            if (IgnoreStatusChanged && value == PowerModes.StatusChange) return;
            _ = Set(value);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// IgnoreStatusChanged
    ///
    /// <summary>
    /// Gets or sets the value indicating whether ignoring the
    /// StatusChanged value.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public bool IgnoreStatusChanged
    {
        get => Get(() => true);
        set => Set(value);
    }

    #endregion

    #region Methods

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
    protected override void Dispose(bool disposing) { }

    #endregion
}
