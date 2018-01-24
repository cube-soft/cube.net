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
            var uri0  = new Uri("http://blog.cube-soft.jp/?feed=rss2");
            var uri1  = new Uri("https://blogs.msdn.microsoft.com/dotnet/feed");
            var file0 = "722a6df5a86c7464e1eeaeb691ba50be";
            var file1 = "3a9c5f4a720884dddb53fb356680ef82";

            using (var src = new RssCacheDictionary() { Directory = Results })
            {
                using (var mon = new RssMonitor(src))
                {
                    var count = 0;
                    var cts   = new CancellationTokenSource();

                    mon.Register(uri0);
                    mon.Register(uri1);
                    mon.Subscribe(e => { if (++count >= 2) cts.Cancel(); });
                    mon.Start();
                    WaitAsync(cts.Token).Wait();
                    mon.Stop();
                }

                Assert.That(src[uri0].Title,       Is.EqualTo("CubeSoft Blog"));
                Assert.That(src[uri0].LastChecked, Is.GreaterThan(start));
                Assert.That(src[uri0].Items.Count, Is.GreaterThan(0));
            }

            Assert.That(IO.Exists(Result(file0)), Is.True, file0);
            Assert.That(IO.Exists(Result(file1)), Is.True, file1);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Register_Remove
        /// 
        /// <summary>
        /// RSS フィード URL の登録および削除処理のテストを実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [Test]
        public void Register_Remove()
        {
            var src  = new Dictionary<Uri, RssFeed>();
            var uri0 = new Uri("http://blog.cube-soft.jp/?feed=rss2");
            var uri1 = new Uri("https://blogs.msdn.microsoft.com/dotnet/feed");

            using (var mon = new RssMonitor(src))
            {
                mon.Register(uri0);
                Assert.That(src[uri0].Uri,   Is.EqualTo(uri0));
                Assert.That(src[uri0].Title, Is.EqualTo(uri0.ToString()));
                mon.Register(uri1);
                Assert.That(src[uri1].Uri,   Is.EqualTo(uri1));
                Assert.That(src[uri1].Title, Is.EqualTo(uri1.ToString()));

                mon.Remove(uri0);
                Assert.That(src.ContainsKey(uri0), Is.False);
                mon.Clear();
                Assert.That(src.Count, Is.EqualTo(0));
            }
        }
    }
}
