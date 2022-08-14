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
using System.Threading.Tasks;
using System.Timers;
using Cube.Collections;
using Microsoft.Win32;

#region WakeableTimerBase

/* ------------------------------------------------------------------------- */
///
/// WakeableTimerBase
///
/// <summary>
/// Represents the base class of timer that suspends/resumes
/// corresponding to the power mode.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public abstract class WakeableTimerBase : DisposableBase
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// WakeableTimerBase
    ///
    /// <summary>
    /// Initializes a new instance of the WakeableTimerBase class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected WakeableTimerBase()
    {
        _interval = TimeSpan.FromSeconds(1);
        _power    = Power.Subscribe(OnPowerModeChanged);
        _inner    = new() { AutoReset = false };
        _inner.Elapsed += RaiseTick;
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// State
    ///
    /// <summary>
    /// Gets the current timer state.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public TimerState State { get; private set; } = TimerState.Stop;

    /* --------------------------------------------------------------------- */
    ///
    /// Interval
    ///
    /// <summary>
    /// Gets or sets the execution interval.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            if (_interval == value) return;
            _interval = value;
            Reset();
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Last
    ///
    /// <summary>
    /// Gets the last time to invoke the action.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public DateTime? Last { get; private set; }

    /* --------------------------------------------------------------------- */
    ///
    /// Next
    ///
    /// <summary>
    /// Gets or sets the time when the registered actions are invoked
    /// next time.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected DateTime Next { get; set; } = DateTime.Now;

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Reset
    ///
    /// <summary>
    /// Resets some condition.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Reset() => OnReset();

    /* --------------------------------------------------------------------- */
    ///
    /// Start
    ///
    /// <summary>
    /// Starts or resumes the timer.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Start() => Start(TimeSpan.Zero);

    /* --------------------------------------------------------------------- */
    ///
    /// Start
    ///
    /// <summary>
    /// Starts or resumes the timer with the specified time.
    /// </summary>
    ///
    /// <param name="delay">Initial delay.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Start(TimeSpan delay)
    {
        if (State == TimerState.Run) return;
        if (State == TimerState.Suspend) Resume(delay);
        else
        {
            State = TimerState.Run;
            Restart(Math.Max(delay.TotalMilliseconds, 1));
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Stop
    ///
    /// <summary>
    /// Stops the timer.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Stop()
    {
        if (State == TimerState.Stop) return;
        if (_inner.Enabled) _inner.Stop();
        State = TimerState.Stop;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Suspend
    ///
    /// <summary>
    /// Suspends the timer.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Suspend()
    {
        if (State != TimerState.Run) return;
        _inner.Stop();
        State = TimerState.Suspend;
        GetType().LogDebug(nameof(Suspend), $"{nameof(Interval)}:{Interval}");
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OnTick
    ///
    /// <summary>
    /// Occurs when the timer is expired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected abstract Task OnTick();

    /* --------------------------------------------------------------------- */
    ///
    /// OnReset
    ///
    /// <summary>
    /// Resets inner fields.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected virtual void OnReset()
    {
        Next = DateTime.Now + Interval;
        _inner.Interval = Interval.TotalMilliseconds;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OnPowerModeChanged
    ///
    /// <summary>
    /// Raises the PowerModeChanged event.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected virtual void OnPowerModeChanged()
    {
        switch (Power.Mode)
        {
            case PowerModes.Resume:
                Resume(TimeSpan.FromMilliseconds(100));
                break;
            case PowerModes.Suspend:
                Suspend();
                break;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Resume
    ///
    /// <summary>
    /// Resumes the timer.
    /// </summary>
    ///
    /// <param name="min">Minimum delay.</param>
    ///
    /* --------------------------------------------------------------------- */
    protected void Resume(TimeSpan min)
    {
        if (State != TimerState.Suspend) return;

        var delta = Next - DateTime.Now;
        var value = delta > min ? delta : min;

        State = TimerState.Run;
        Next  = DateTime.Now + value;

        GetType().LogDebug(nameof(Resume),
            $"{nameof(Interval)}:{Interval}",
            $"{nameof(Last)}:{Last}",
            $"{nameof(Next)}:{Next}"
        );

        _inner.Interval = Math.Max(value.TotalMilliseconds, 1);
        _inner.Start();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases the unmanaged resources used by the WakeableTimer
    /// and optionally releases the managed resources.
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
        if (!disposing) return;
        State = TimerState.Stop;
        _power?.Dispose();
        _inner?.Dispose();
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// Restart
    ///
    /// <summary>
    /// Restarts the timer.
    /// </summary>
    ///
    /// <remarks>
    /// As a general rule, execution will start at the interval set by
    /// the user. However, if the total processing time of the handlers
    /// registered in Subscribe exceeds the interval set by the user,
    /// the next processing will be executed after an interval of
    /// at least 1/10 of a second.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    private void Restart(DateTime time)
    {
        var delta = (DateTime.Now - time).TotalMilliseconds;
        var msec  = Interval.TotalMilliseconds;
        Restart(Math.Max(Math.Max(msec - delta, msec / 10.0), 1.0));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Restart
    ///
    /// <summary>
    /// Restarts the timer.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void Restart(double msec)
    {
        Next = DateTime.Now + TimeSpan.FromMilliseconds(msec);
        if (State != TimerState.Run) return;
        _inner.Interval = msec;
        _inner.Start();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RaiseTick
    ///
    /// <summary>
    /// Occurs at the provided intervals.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private async void RaiseTick(object s, ElapsedEventArgs e)
    {
        if (State != TimerState.Run) return;
        try
        {
            Last = e.SignalTime;
            await OnTick().ConfigureAwait(false);
        }
        finally { Restart(e.SignalTime); }
    }

    #endregion

    #region Fields
    private readonly IDisposable _power;
    private readonly Timer _inner;
    private TimeSpan _interval;
    #endregion
}

#endregion

#region WakeableTimer

/* ------------------------------------------------------------------------- */
///
/// WakeableTimer
///
/// <summary>
/// Represents the timer that suspends/resumes corresponding to the
/// power mode.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public sealed class WakeableTimer : WakeableTimerBase
{
    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Subscribe
    ///
    /// <summary>
    /// Sets the specified asynchronous action to the timer.
    /// </summary>
    ///
    /// <param name="callback">Asynchronous user action.</param>
    ///
    /// <returns>Object to remove from the subscription.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public IDisposable Subscribe(AsyncAction callback) => _subscription.Subscribe(callback);

    /* --------------------------------------------------------------------- */
    ///
    /// OnTick
    ///
    /// <summary>
    /// Occurs when the timer is expired.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override async Task OnTick()
    {
        foreach (var cb in _subscription) await cb().ConfigureAwait(false);
    }

    #endregion

    #region Fields
    private readonly Subscription<AsyncAction> _subscription = new();
    #endregion
}

#endregion

#region TimerState

/* ------------------------------------------------------------------------- */
///
/// TimerState
///
/// <summary>
/// Specifies the timer state.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public enum TimerState
{
    /// <summary>Run</summary>
    Run = 0,
    /// <summary>Stop</summary>
    Stop = 1,
    /// <summary>Suspend</summary>
    Suspend = 2,
    /// <summary>Unknown</summary>
    Unknown = -1
}

#endregion
