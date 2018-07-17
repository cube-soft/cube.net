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
using Cube.FileSystem.TestService;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpMonitorTest
    ///
    /// <summary>
    /// NtpMonitor のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpMonitorTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_CubeNtpServer
        ///
        /// <summary>
        /// NTP サーバを監視するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_CubeNtpServer()
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                var start = DateTime.Now;
                var count = 0;
                var cts   = new CancellationTokenSource();

                mon.Server  = ""; // test
                mon.Server  = "ntp.cube-soft.jp";
                mon.Port    = 0; // test
                mon.Port    = 123;
                mon.Timeout = TimeSpan.FromMilliseconds(500);

                mon.Subscribe(_ => throw new ArgumentException("Test"));
                mon.Subscribe(_ => { ++count; cts.Cancel(); });
                mon.Start();
                mon.Start(); // ignore
                Assert.That(Wait.For(cts.Token), "Timeout");
                mon.Stop();
                mon.Stop(); // ignore

                Assert.That(count, Is.AtLeast(1));
                Assert.That(mon.LastPublished.Value, Is.GreaterThan(start));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_InvalidNtpServer
        ///
        /// <summary>
        /// 無効な NTP サーバを指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_InvalidNtpServer()
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                var count = 0;

                mon.Server  = "dummy";
                mon.Port    = 999;
                mon.Timeout = TimeSpan.FromMilliseconds(100);

                mon.Subscribe(_ => ++count);
                mon.Start();
                Task.Delay(150).Wait();
                mon.Stop();

                Assert.That(count, Is.EqualTo(0));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_NoSubscriptions
        ///
        /// <summary>
        /// コールバック関数を指定しなかった時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_NoSubscriptions() => Assert.DoesNotThrow(() =>
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                mon.Start();
                Task.Delay(150).Wait();
                mon.Stop();
            }
        });

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
        public void Reset()
        {
            using (var mon = new Ntp.NtpMonitor())
            {
                var count = 0;
                var cts   = new CancellationTokenSource();

                mon.Server  = "ntp.cube-soft.jp";
                mon.Port    = 123;
                mon.Timeout = TimeSpan.FromMilliseconds(500);

                mon.Subscribe(_ => { ++count; cts.Cancel(); });
                mon.Start(mon.Interval);
                mon.Reset();
                Assert.That(Wait.For(cts.Token), "Timeout");
                mon.Stop();

                Assert.That(count, Is.EqualTo(1));
            }
        }

        #endregion
    }
}
