/* ------------------------------------------------------------------------- */
///
/// MonitorTester.cs
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
using TaskEx = System.Threading.Tasks.Task;

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.Ntp.MonitorTester
    ///
    /// <summary>
    /// Ntp.Monitor クラスのテストを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class MonitorTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestRun
        /// 
        /// <summary>
        /// NTP サーバを監視するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestRun()
        {
            var monitor = new Cube.Net.Ntp.Monitor("ntp.cube-soft.jp");
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var cts = new CancellationTokenSource();
                monitor.ResultChanged += (s, e) => cts.Cancel();
                monitor.Timeout  = TimeSpan.FromMilliseconds(1000);
                monitor.Start();
                TaskEx.Delay((int)(monitor.Timeout.TotalMilliseconds * 2), cts.Token).Wait();
            });
            Assert.That(ex.InnerException, Is.TypeOf<TaskCanceledException>());
            Assert.That(monitor.IsValid, Is.True);
        }
    }
}
