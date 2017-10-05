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
using System.Collections.Generic;
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
    [TestFixture]
    class RssMonitorTest : NetworkHelper
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

                mon.Version = new SoftwareVersion("1.0.0");
                mon.Timeout = TimeSpan.FromMilliseconds(500);
                mon.Uris.Add(new Uri("http://blog.cube-soft.jp/?feed=rss2"));
                mon.Uris.Add(new Uri("http://clown.hatenablog.jp/rss"));

                var count = 0;
                mon.Subscribe((u, x) => count++);
                mon.Start();
                await Task.Delay((int)(mon.Timeout.TotalMilliseconds * 4));
                mon.Stop();

                Assert.That(count, Is.EqualTo(2));
                Assert.Pass($"{nameof(mon.FailedCount)}:{mon.FailedCount}");
            }
        }
    }
}
