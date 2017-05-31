/* ------------------------------------------------------------------------- */
///
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Threading;
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
    [Parallelizable]
    [TestFixture]
    class NtpMonitorTest : NetworkResource
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        /// 
        /// <summary>
        /// NTP サーバを監視するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task Monitor()
        {
            using (var monitor = new Ntp.Monitor())
            {
                Assert.That(monitor.NetworkAvailable, Is.True);

                monitor.Server  = "ntp.cube-soft.jp";
                monitor.Port    = 123;
                monitor.Timeout = TimeSpan.FromSeconds(2);

                var result = TimeSpan.Zero;
                monitor.Subscribe(x => result = x);
                monitor.Start();
                await Task.Delay((int)(monitor.Timeout.TotalMilliseconds * 2));
                monitor.Stop();

                Assert.That(result, Is.Not.EqualTo(TimeSpan.Zero));
                Assert.That(monitor.FailedCount, Is.EqualTo(0));
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
        public async Task Reset()
        {
            using (var monitor = new Ntp.Monitor())
            {
                Assert.That(monitor.NetworkAvailable, Is.True);

                monitor.Server  = "ntp.cube-soft.jp";
                monitor.Port    = 123;
                monitor.Timeout = TimeSpan.FromSeconds(2);

                var count = 0;
                monitor.Subscribe(_ => ++count);
                monitor.Start(monitor.Interval);
                monitor.Reset();
                await Task.Delay((int)(monitor.Timeout.TotalMilliseconds * 2));
                monitor.Stop();
                Assert.That(count, Is.EqualTo(1));
            }
        }
    }
}
