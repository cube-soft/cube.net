/* ------------------------------------------------------------------------- */
///
/// MonitorTest.cs
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

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// MonitorTest
    ///
    /// <summary>
    /// Cube.Net.Ntp.Monitor のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class MonitorTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Start_RunOnce
        /// 
        /// <summary>
        /// NTP サーバを監視するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Start_RunOnce()
        {
            var monitor = new Cube.Net.Ntp.Monitor("ntp.cube-soft.jp");
            Assert.That(
                async () =>
                {
                    var cts = new CancellationTokenSource();
                    monitor.ResultChanged += (s, e) => cts.Cancel();
                    monitor.Timeout = TimeSpan.FromMilliseconds(100);
                    monitor.Start();
                    await Task.Delay((int)(monitor.Timeout.TotalMilliseconds * 2), cts.Token);
                },
                Throws.TypeOf<TaskCanceledException>()
            );
            Assert.That(monitor.IsValid, Is.True);
        }
    }
}
