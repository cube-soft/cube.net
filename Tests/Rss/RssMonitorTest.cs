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
using System.Collections.Generic;
using System.Threading;
using Cube.Net.Rss;
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
    class RssMonitorTest : FileHelper
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
        public void Start()
        {
            var uri = new Uri("http://blog.cube-soft.jp/?feed=rss2");
            var src = new Dictionary<Uri, RssFeed>();

            using (var mon = new RssMonitor(src))
            {
                var cts = new CancellationTokenSource();
                mon.Subscribe((u, v) => cts.Cancel());
                mon.Register(uri);
                mon.Start();
                WaitAsync(cts.Token).Wait();
                mon.Stop();
            }

            Assert.That(src.ContainsKey(uri), Is.True);
            Assert.That(src[uri].Title,       Is.Not.Null.And.Not.Empty);
            Assert.That(src[uri].LastChecked, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(src[uri].Items.Count, Is.GreaterThan(0));
        }
    }
}
