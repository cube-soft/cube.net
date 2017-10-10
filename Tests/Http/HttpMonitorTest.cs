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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Net.Tests.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitorTest
    ///
    /// <summary>
    /// Http.Monitor(T) のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    [Ignore("NUnit for .NET 3.5 does not support async/await")]
    class HttpMonitorTest : NetworkHelper
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        /// 
        /// <summary>
        /// 監視テストを実行します。
        /// </summary>
        /// 
        /// <remarks>
        /// 過去、TCP コネクションが CLOSE_WAIT のまま残存し、3 回目以降の
        /// 通信に失敗すると言う不都合（.NET の初期設定では一つのエンド
        /// ポイントに対して同時に確立できるコネクションは 2 つ）が
        /// 確認されたので、その確認テストも兼ねます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Start()
        {
            using (var mon = new Cube.Net.Http.HttpMonitor<int>(Convert))
            {
                Assert.That(mon.NetworkAvailable, Is.True);

                mon.Version  = new SoftwareVersion("1.0.0");
                mon.Interval = TimeSpan.FromMilliseconds(100);
                mon.Timeout  = TimeSpan.FromMilliseconds(1000);
                mon.Uris.Add(new Uri("http://www.cube-soft.jp/"));
                mon.Uris.Add(new Uri("http://s.cube-soft.jp/"));

                var cts = new CancellationTokenSource();
                var sum = 0;

                mon.Subscribe((u, x) => { sum += x; cts.Cancel(); });
                mon.Start();
                mon.Start(); // ignore

                Assert.That(
                    async() => await TaskEx.Delay((int)(mon.Timeout.TotalMilliseconds * 2), cts.Token),
                    Throws.TypeOf<TaskCanceledException>()
                );

                mon.Stop();
                mon.Stop(); // ignore

                Assert.That(sum, Is.AtLeast(1));
                Assert.Pass($"{nameof(mon.FailedCount)}:{mon.FailedCount}");
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
        public void Reset()
        {
            using (var mon = new Cube.Net.Http.HttpMonitor<int>(Convert))
            {
                mon.Version  = new SoftwareVersion("1.0.0");
                mon.Interval = TimeSpan.FromMinutes(1);
                mon.Timeout  = TimeSpan.FromMilliseconds(1000);
                mon.Uris.Add(new Uri("http://www.cube-soft.jp/"));

                var cts   = new CancellationTokenSource();
                var count = 0;

                mon.Subscribe((u, x) => { ++count; cts.Cancel(); });
                mon.Reset();
                mon.Start(mon.Interval);
                mon.Reset();

                Assert.That(
                    async() => await TaskEx.Delay((int)(mon.Timeout.TotalMilliseconds * 2), cts.Token),
                    Throws.TypeOf<TaskCanceledException>()
                );

                mon.Stop();

                Assert.That(count, Is.EqualTo(1));
                Assert.Pass($"{nameof(mon.FailedCount)}:{mon.FailedCount}");
            }
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// HttpContent の変換処理を実行する関数オブジェクトです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private Func<HttpContent, Task<int>> Convert = async (s) =>
        {
            var str = await s.ReadAsStringAsync();
            return str.Length;
        };

        #endregion
    }
}
