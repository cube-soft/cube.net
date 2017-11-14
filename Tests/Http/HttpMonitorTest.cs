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
        public async Task Start()
        {
            using (var mon = new Cube.Net.Http.HttpMonitor<int>(Convert))
            {
                mon.UserAgent = $"Cube.Net.Tests/{AssemblyReader.Default.Version}";
                mon.Interval  = TimeSpan.FromMilliseconds(100);
                mon.Timeout   = TimeSpan.FromMilliseconds(1000);
                mon.Uri       = new Uri("http://www.cube-soft.jp/");

                var cts   = new CancellationTokenSource();
                var count = 0;

                mon.Subscribe((u, x) =>
                {
                    if (count >= 3) cts.Cancel();
                    count++;
                });

                mon.Start();
                mon.Start(); // ignore
                await WaitAsync(cts.Token);
                mon.Stop();
                mon.Stop(); // ignore

                Assert.That(count, Is.EqualTo(3));
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
        public async Task Reset()
        {
            using (var mon = new Cube.Net.Http.HttpMonitor<int>(Convert))
            {
                mon.UserAgent = $"Cube.Net.Tests/{AssemblyReader.Default.Version}";
                mon.Interval  = TimeSpan.FromMinutes(1);
                mon.Timeout   = TimeSpan.FromMilliseconds(1000);
                mon.Uri       = new Uri("http://www.cube-soft.jp/");

                var cts   = new CancellationTokenSource();
                var count = 0;

                mon.Subscribe((u, x) => { ++count; cts.Cancel(); });
                mon.Reset();
                mon.Start(mon.Interval);
                mon.Reset();
                await WaitAsync(cts.Token);
                mon.Stop();

                Assert.That(count, Is.EqualTo(1));
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
