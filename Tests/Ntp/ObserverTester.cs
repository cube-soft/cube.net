/* ------------------------------------------------------------------------- */
///
/// ObserverTester.cs
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
using TaskEx = Cube.TaskEx;

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Tests.Net.Ntp.ObserverTester
    ///
    /// <summary>
    /// Observer クラスのテストをするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ObserverTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestObserve
        /// 
        /// <summary>
        /// NTP サーバとの通信のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestObserve()
        {
            var observer = new Cube.Net.Ntp.Observer("ntp.cube-soft.jp");
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var cts = new CancellationTokenSource();
                observer.ResultChanged += (s, e) => cts.Cancel();
                observer.Timeout = TimeSpan.FromSeconds(1);
                observer.Start();
                TaskEx.Delay(2000, cts.Token).Wait();
            });
            Assert.That(ex.InnerException, Is.TypeOf<TaskCanceledException>());
            Assert.That(observer.IsValid, Is.True);
        }
    }
}
