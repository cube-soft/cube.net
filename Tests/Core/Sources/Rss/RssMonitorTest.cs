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
using System.Reflection;
using System.Threading;
using Cube.Mixin.Assembly;
using Cube.Net.Rss;
using Cube.Tests;
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
    class RssMonitorTest : FileFixture
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_RssCacheDictionary
        ///
        /// <summary>
        /// RssCacheDirectory オブジェクトを利用した監視テストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_RssCacheDictionary()
        {
            var start = DateTime.Now;
            var uris  = new[]
            {
                new Uri("https://clown.cube-soft.jp/feed"),
            };

            var files = new[]
            {
                "f5acb23c3de871999c2034f864464c92",
            };

            using (var src = new RssCacheDictionary())
            {
                src.Directory = Results;
                foreach (var uri in uris) src.Add(uri, default);

                using (var mon = Create())
                {
                    var cts   = new CancellationTokenSource();
                    var asm   = Assembly.GetExecutingAssembly();
                    var agent = $"CubeRssMonitorTest/{asm.GetVersion()}";

                    mon.Timeout = TimeSpan.FromSeconds(5);
                    mon.UserAgent = agent;
                    Assert.That(mon.UserAgent, Is.EqualTo(agent));

                    mon.Register(uris);
                    mon.Register(uris); // ignore
                    mon.Subscribe(e => throw new ArgumentException("Test"));
                    mon.Subscribe(e =>
                    {
                        src[e.Uri] = e;
                        cts.Cancel();
                    });
                    mon.Start();
                    Assert.That(Wait.For(cts.Token), "Timeout");
                    mon.Stop();
                }

                foreach (var item in src) Assert.That(item.Value, Is.Not.Null);

                Assert.That(src.TryGetValue(uris[0], out var feed0), Is.True);
                Assert.That(feed0.Title,       Is.EqualTo("Life like a clown"));
                Assert.That(feed0.LastChecked, Is.GreaterThan(start), uris[0].ToString());
                Assert.That(feed0.Items.Count, Is.GreaterThan(0), uris[0].ToString());
                Assert.That(src.TryGetValue(new Uri("http://www.example.com/"), out _), Is.False);
            }

            Assert.That(IO.Get(Get(files[0])).Length, Is.GreaterThan(0), files[0]);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor_Error
        ///
        /// <summary>
        /// RSS の取得に失敗した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Monitor_Error()
        {
            var src  = new Uri("http://www.cube-soft.jp/");
            var dest = default(RssFeed);

            using (var mon = Create())
            {
                var cts = new CancellationTokenSource();

                mon.RetryCount = 0;
                mon.Register(src);
                mon.Subscribe(e => { dest = e; cts.Cancel(); });
                mon.Start();
                Assert.That(Wait.For(cts.Token), "Timeout");
                mon.Stop();
            }

            Assert.That(dest.Title,                  Is.Empty);
            Assert.That(dest.Uri,                    Is.EqualTo(src));
            Assert.That(dest.Link,                   Is.Null);
            Assert.That(dest.Items.Count,            Is.EqualTo(0));
            Assert.That(dest.Error,                  Is.Not.Null);
            Assert.That(dest.LastChecked.HasValue,   Is.True);
            Assert.That(dest.LastPublished.HasValue, Is.False);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RssMonitor を用いて手動で更新するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Update()
        {
            var dest = new Dictionary<Uri, RssFeed>();
            var src  = new[]
            {
                new Uri("https://clown.cube-soft.jp/feed"),
                new Uri("https://blogs.msdn.microsoft.com/dotnet/feed"),
            };

            using (var mon = Create())
            {
                var cts = new CancellationTokenSource();

                mon.Register(src[0]);
                mon.Subscribe(e => { dest.Add(e.Uri, e); cts.Cancel(); });
                mon.Update(src[1]);
                mon.Update(src[0]);
                Assert.That(Wait.For(cts.Token), "Timeout");
            }

            Assert.That(dest.ContainsKey(src[0]), Is.True);
            Assert.That(dest.ContainsKey(src[1]), Is.False);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update_Error
        ///
        /// <summary>
        /// Update 失敗時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Update_Error()
        {
            var dest = new Dictionary<Uri, RssFeed>();
            var src  = new Uri("https://wwww.cube-soft.jp/");

            using (var mon = Create())
            {
                var cts = new CancellationTokenSource();
                mon.Register(src);
                mon.Subscribe(e => { dest.Add(e.Uri, e); cts.Cancel(); });
                mon.Update(src);
                Assert.That(Wait.For(cts.Token), "Timeout");
            }

            Assert.That(dest.ContainsKey(src), Is.True);
            Assert.That(dest[src].Error,       Is.Not.Null);
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
            var uris = new[]
            {
                new Uri("http://www.example.com/rss"),
                new Uri("http://www.example.com/rss2"),
            };

            using (var mon = Create())
            {
                mon.Register(uris[0]);
                Assert.That(mon.Contains(uris[0]), Is.True);
                Assert.That(mon.LastChecked(uris[0]).HasValue, Is.False);
                mon.Register(uris[1]);
                Assert.That(mon.Contains(uris[1]), Is.True);
                Assert.That(mon.LastChecked(uris[1]).HasValue, Is.False);

                mon.Remove(uris[0]);
                Assert.That(mon.Contains(uris[0]), Is.False);
                mon.Clear();
                Assert.That(mon.Contains(uris[1]), Is.False);
            }
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// RssMonitor オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private RssMonitor Create()
        {
            var asm = Assembly.GetExecutingAssembly();
            return new RssMonitor { UserAgent = $"{asm.GetProduct()}/{asm.GetVersion()}" };
        }

        #endregion
    }
}
