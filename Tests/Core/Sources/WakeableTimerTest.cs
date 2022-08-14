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
namespace Cube.Net.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Cube.Mixin.Tasks;
using Cube.Synchronous;
using Cube.Tests;
using Microsoft.Win32;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// WakeableTimerTest
///
/// <summary>
/// Tests for the WakeableTimer class.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class WakeableTimerTest
{
    #region Tests

    /* --------------------------------------------------------------------- */
    ///
    /// Create
    ///
    /// <summary>
    /// Tests the constructor and confirms properties.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Properties()
    {
        using var src = new WakeableTimer();
        Assert.That(src.State, Is.EqualTo(TimerState.Stop));
        Assert.That(src.Interval, Is.EqualTo(TimeSpan.FromSeconds(1)));
        Assert.That(src.Last.HasValue, Is.False);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Properties_Disposed
    ///
    /// <summary>
    /// Confirms properties after disposed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Properties_Disposed()
    {
        var src = new WakeableTimer();
        src.Start();
        Task.Delay(100).Wait();
        Assert.That(src.State, Is.EqualTo(TimerState.Run));
        src.Dispose();
        Assert.That(src.State, Is.EqualTo(TimerState.Stop));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Transition_State
    ///
    /// <summary>
    /// Confirms the transition of the State.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Transition_State()
    {
        using var src = new WakeableTimer();
        Assert.That(src.State, Is.EqualTo(TimerState.Stop));
        src.Start();
        Assert.That(src.State, Is.EqualTo(TimerState.Run));
        src.Start(); // ignore
        Assert.That(src.State, Is.EqualTo(TimerState.Run));
        src.Suspend();
        Assert.That(src.State, Is.EqualTo(TimerState.Suspend));
        src.Suspend();
        Assert.That(src.State, Is.EqualTo(TimerState.Suspend));
        src.Start();
        Assert.That(src.State, Is.EqualTo(TimerState.Run));
        src.Stop();
        Assert.That(src.State, Is.EqualTo(TimerState.Stop));
        src.Stop(); // ignore
        Assert.That(src.State, Is.EqualTo(TimerState.Stop));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Transition_PowerMode
    ///
    /// <summary>
    /// Confirms the transition of Power.Mode and State.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Transition_PowerMode()
    {
        var pmc   = new PowerModeContext(Power.Mode);
        var dummy = 0;

        Power.Configure(pmc);

        using (var src = new WakeableTimer())
        using (src.SubscribeSync(() => ++dummy))
        {
            src.Start();

            pmc.Mode = PowerModes.Suspend;
            Assert.That(Power.Mode, Is.EqualTo(PowerModes.Suspend));
            Assert.That(src.State, Is.EqualTo(TimerState.Suspend));

            pmc.Mode = PowerModes.Resume;
            Assert.That(Power.Mode, Is.EqualTo(PowerModes.Resume));
            Assert.That(src.State, Is.EqualTo(TimerState.Run));

            src.Stop();
            Assert.That(Power.Mode, Is.EqualTo(PowerModes.Resume));
            Assert.That(src.State, Is.EqualTo(TimerState.Stop));
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Start
    ///
    /// <summary>
    /// Tests the normal scenario of the WakeableTimer class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Start()
    {
        using var src = new WakeableTimer { Interval = TimeSpan.FromMilliseconds(100) };
        src.Interval = TimeSpan.FromMilliseconds(100); // ignore
        Assert.That(src.Last.HasValue, Is.False);
        Assert.That(Execute(src, 0, 1), "Timeout");

        var time = src.Last;
        Assert.That(time, Is.Not.EqualTo(DateTime.MinValue));

        src.Reset();
        Assert.That(src.Last, Is.EqualTo(time));
        Assert.That(src.Interval.TotalMilliseconds, Is.EqualTo(100).Within(1.0));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Start_InitialDelay
    ///
    /// <summary>
    /// Tests the Start method with the initial delay.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Start_InitialDelay()
    {
        using var src = new WakeableTimer { Interval = TimeSpan.FromHours(1) };
        Assert.That(src.Last.HasValue, Is.False);
        Assert.That(Execute(src, 200, 1), "Timeout");
        Assert.That(src.Last, Is.Not.EqualTo(DateTime.MinValue));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Start_Burstly
    ///
    /// <summary>
    /// Tests the Start method when the Interval is set to the very
    /// short time.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Start_Burstly()
    {
        var cts   = new CancellationTokenSource();
        var count = 0;

        using var src = new WakeableTimer { Interval = TimeSpan.FromMilliseconds(10) };
        using var ds  = src.Subscribe(async () =>
        {
            ++count;
            await Task.Delay(200).ConfigureAwait(false);
            src.Stop();
            cts.Cancel();
        });

        Assert.That(Execute(src, 0, cts), "Timeout");
        Assert.That(count, Is.EqualTo(1));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Resume
    ///
    /// <summary>
    /// Tests the Suspend/Resume commands.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Resume()
    {
        var cts   = new CancellationTokenSource();
        var count = 0;

        using var src = new WakeableTimer { Interval = TimeSpan.FromSeconds(1) };
        src.Start(TimeSpan.FromMilliseconds(100));
        using var ds = src.SubscribeSync(() =>
        {
            ++count;
            src.Stop();
            cts.Cancel();
        });
        src.Start();
        src.Suspend();

        Assert.That(count, Is.EqualTo(0));
        Task.Delay(300).Wait();
        Assert.That(count, Is.EqualTo(0));
        Assert.That(Execute(src, 0, cts), "Timeout");
        Assert.That(count, Is.EqualTo(1));
    }

    #endregion

    #region Others

    /* --------------------------------------------------------------------- */
    ///
    /// Execute
    ///
    /// <summary>
    /// Waits for the timer to execute the specified number of callbacks.
    /// </summary>
    ///
    /// <param name="src">Timer object.</param>
    /// <param name="msec">Initial delay.</param>
    /// <param name="cts">Cancellation token.</param>
    ///
    /// <returns>true for success.</returns>
    ///
    /* --------------------------------------------------------------------- */
    private bool Execute(WakeableTimer src, int msec, CancellationTokenSource cts)
    {
        Task.Run(() =>
        {
            if (msec <= 0) src.Start();
            else src.Start(TimeSpan.FromMilliseconds(msec));
        }).Forget();

        return Wait.For(cts.Token);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Execute
    ///
    /// <summary>
    /// Waits for the timer to execute the specified number of callbacks.
    /// </summary>
    ///
    /// <param name="src">Timer object.</param>
    /// <param name="msec">Initial delay.</param>
    /// <param name="count">
    /// Number of callbacks that the timer waits.
    /// </param>
    ///
    /// <returns>true for success.</returns>
    ///
    /* --------------------------------------------------------------------- */
    private bool Execute(WakeableTimer src, int msec, int count)
    {
        var n   = 0;
        var cts = new CancellationTokenSource();

        using (src.SubscribeSync(() =>
        {
            if (++n >= count)
            {
                src.Stop();
                cts.Cancel();
            }
        })) return Execute(src, msec, cts);
    }

    #endregion
}
