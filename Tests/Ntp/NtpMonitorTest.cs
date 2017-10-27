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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpMonitorTest
    ///
    /// <summary>
    /// Ntp.Monitor のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpMonitorTest : NetworkHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Start
        /// 
        /// <summary>
        /// NTP サーバを監視するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async void Start()
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                Assert.That(mon.NetworkAvailable, Is.True);

                mon.Server  = "ntp.nict.jp";
                mon.Port    = 123;
                mon.Timeout = TimeSpan.FromMilliseconds(500);

                var count = 0;
                mon.Subscribe(_ => ++count);
                mon.Start();
                mon.Start(); // ignore
                await TaskEx.Delay((int)(mon.Timeout.TotalMilliseconds * 2));
                mon.Stop();
                mon.Stop(); // ignore

                Assert.That(count, Is.AtLeast(1));
                Assert.Pass($"{nameof(mon.FailedCount)}:{mon.FailedCount}");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// リセット処理のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async void Reset()
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                Assert.That(mon.NetworkAvailable, Is.True);

                mon.Server  = "ntp.cube-soft.jp";
                mon.Port    = 123;
                mon.Timeout = TimeSpan.FromMilliseconds(500);

                var count = 0;
                mon.Subscribe(_ => ++count);
                mon.Start(mon.Interval);
                mon.Reset();
                await TaskEx.Delay((int)(mon.Timeout.TotalMilliseconds * 2));
                mon.Stop();

                Assert.That(count, Is.EqualTo(1));
                Assert.Pass($"{nameof(mon.FailedCount)}:{mon.FailedCount}");
            }
        }
    }
}
