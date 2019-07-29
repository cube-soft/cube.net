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
using Cube.Mixin.Logging;
using System;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkMonitorBase
    ///
    /// <summary>
    /// Represents the base class to invoke the transmission constantly.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class NetworkMonitorBase : DisposableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkMonitorBase
        ///
        /// <summary>
        /// Initializes a new instance of the NetworkMonitorBase class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected NetworkMonitorBase() { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// State
        ///
        /// <summary>
        /// Gets the monitor state.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimerState State => Timer.State;

        /* ----------------------------------------------------------------- */
        ///
        /// LastPublished
        ///
        /// <summary>
        /// Gets the value of date-time when Publish method was last called.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? Last => Timer.Last;

        /* ----------------------------------------------------------------- */
        ///
        /// Interval
        ///
        /// <summary>
        /// Gets or sets the transmission interval.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Interval
        {
            get => Timer.Interval;
            set => Timer.Interval = value;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// RetryCount
        ///
        /// <summary>
        /// Gets or sets the retry count when the transmission was failed.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public int RetryCount { get; set; } = 3;

        /* --------------------------------------------------------------------- */
        ///
        /// RetryInterval
        ///
        /// <summary>
        /// Gets or sets the retry interval when the transmission was failed.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// Gets or sets the timeout of the transmission.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /* ----------------------------------------------------------------- */
        ///
        /// Timer
        ///
        /// <summary>
        /// Gets the timer object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected WakeableTimer Timer { get; } = new WakeableTimer();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// Starts to monitor.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Start() => Start(TimeSpan.Zero);

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// Starts to monitor after waiting for the specified time.
        /// </summary>
        ///
        /// <param name="delay">Initial delay.</param>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Start(TimeSpan delay)
        {
            var state = Timer.State;
            Timer.Start(delay);
            if (state != TimerState.Stop) return;
            this.LogDebug(nameof(Start), $"Interval:{Interval}", $"Delay:{delay}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// Stops to monitor.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Stop()
        {
            var state = Timer.State;
            Timer.Stop();
            if (state == TimerState.Stop) return;
            this.LogDebug(nameof(Stop), $"{nameof(Timer.Last)}:{Timer.Last}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Suspend
        ///
        /// <summary>
        /// Suspends to monitor.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Suspend() => Timer.Suspend();

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// Resets some properties.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Reset()
        {
            var state = State;
            Timer.Reset();
            if (state == TimerState.Run)
            {
                Stop();
                Start();
            }
        }

        /* ----------------------------------------------------------------- */
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
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (disposing) Timer.Dispose();
        }

        #endregion
    }
}
