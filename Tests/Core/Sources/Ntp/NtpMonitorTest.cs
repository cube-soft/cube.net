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
using System;
using System.Threading;
using System.Threading.Tasks;
using Cube.Net.Ntp;
using Cube.Tests;
using NUnit.Framework;

namespace Cube.Net.Tests.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpMonitorTest
    ///
    /// <summary>
    /// Tests the NtpMonitor class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpMonitorTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// Tests to communicate with the NTP server periodically.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor()
        {
            using var mon = new NtpMonitor();

            var start = DateTime.Now;
            var count = 0;
            var cts   = new CancellationTokenSource();

            mon.Server  = ""; // test
            mon.Server  = "ntp.cube-soft.jp";
            mon.Port    = 0; // test
            mon.Port    = 123;
            mon.Timeout = TimeSpan.FromMilliseconds(500);

            _ = mon.Subscribe(_ => throw new ArgumentException("Test"));
            _ = mon.Subscribe(_ => { ++count; cts.Cancel(); return Task.FromResult(0); });
            mon.Start();
            mon.Start(); // ignore
            Assert.That(Wait.For(cts.Token), "Timeout");
            mon.Stop();
            mon.Stop(); // ignore

            Assert.That(count, Is.AtLeast(1));
            Assert.That(mon.Last.Value, Is.GreaterThan(start));
            Assert.That(mon.Interval, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_NotFound
        ///
        /// <summary>
        /// Tests to monitor the inexistent NTP server.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_NotFound()
        {
            using var mon = new NtpMonitor { Interval = TimeSpan.FromMilliseconds(100) };
            var count = 0;

            mon.Server  = "dummy";
            mon.Port    = 999;
            mon.Timeout = TimeSpan.FromMilliseconds(100);

            _ = mon.Subscribe(_ => { ++count; return Task.FromResult(0); });
            mon.Start();
            Task.Delay(1000).Wait();
            mon.Stop();

            Assert.That(count, Is.EqualTo(0));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_NoSubscriptions
        ///
        /// <summary>
        /// Tests to monitor the NTP server with no subscriptions.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_NoSubscriptions()
        {
            using var mon = new NtpMonitor { Interval = TimeSpan.FromMilliseconds(500) };

            mon.Start();
            Task.Delay(1000).Wait();
            mon.Stop();
            Assert.That(mon.Last.HasValue, Is.True);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// Tests the Reset method.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Reset()
        {
            using var mon = new NtpMonitor { Interval = TimeSpan.FromSeconds(1) };
            var count = 0;
            var cts   = new CancellationTokenSource();

            mon.Server  = "ntp.cube-soft.jp";
            mon.Port    = 123;
            mon.Timeout = TimeSpan.FromMilliseconds(500);

            _ = mon.Subscribe(_ => { ++count; cts.Cancel(); return Task.FromResult(0); });
            mon.Start(mon.Interval);
            mon.Reset();
            Assert.That(mon.State, Is.EqualTo(TimerState.Run));
            Assert.That(Wait.For(cts.Token), "Timeout");
            mon.Stop();

            Assert.That(count, Is.EqualTo(1));
        }

        #endregion
    }
}
