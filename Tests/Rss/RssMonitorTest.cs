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
using Cube.Net.Rss;
using NUnit.Framework;

namespace Cube.Net.Tests
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
            var start = DateTime.Now;
            var u0  = new Uri("http://blog.cube-soft.jp/?feed=rss2");
            var f0 = "722a6df5a86c7464e1eeaeb691ba50be";
            var u1  = new Uri("https://blogs.msdn.microsoft.com/dotnet/feed");
            var f1  = "3a9c5f4a720884dddb53fb356680ef82";

            using (var src = new RssCacheCollection() { Directory = Results })
            {
                using (var mon = new RssMonitor(src))
                {
                    var count = 0;
                    var cts   = new CancellationTokenSource();

                    mon.Register(u0);
                    mon.Register(u1);
                    mon.Subscribe(e => { if (++count >= 2) cts.Cancel(); });
                    mon.Start();
                    WaitAsync(cts.Token).Wait();
                    mon.Stop();
                }

                Assert.That(src[u0].Title,       Is.EqualTo("CubeSoft Blog"));
                Assert.That(src[u0].LastChecked, Is.GreaterThan(start));
                Assert.That(src[u0].Items.Count, Is.GreaterThan(0));
            }

            Assert.That(IO.Exists(Result(f0)), Is.True, f0);
            Assert.That(IO.Exists(Result(f1)), Is.True, f1);
        }
    }
}
