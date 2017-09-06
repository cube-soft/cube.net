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
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Net.Tests.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssMonitorTest
    ///
    /// <summary>
    /// RssMonitor のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class RssMonitorTest : NetworkHandler
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Start
        /// 
        /// <summary>
        /// 監視テストを実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task Start()
        {
            using (var mon = new Cube.Net.Rss.RssMonitor())
            {
                Assert.That(mon.NetworkAvailable, Is.True);

                mon.Version  = new SoftwareVersion("1.0.0");
                mon.Interval = TimeSpan.FromMilliseconds(100);
                mon.Timeout  = TimeSpan.FromMilliseconds(500);
                mon.Uris.Add(new Uri("http://blog.cube-soft.jp/?feed=rss2"));
                mon.Uris.Add(new Uri("http://clown.hatenablog.jp/rss"));

                var sum = 0;
                mon.Subscribe((u, x) => sum += x.Items.Count());
                mon.Start();
                mon.Start(); // ignore
                await Task.Delay((int)(mon.Timeout.TotalMilliseconds * 4));
                mon.Stop();
                mon.Stop(); // ignore

                Assert.That(mon.FailedCount, Is.EqualTo(0));
                Assert.That(sum, Is.AtLeast(2));
            }
        }
    }
}
