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
using System.Security.Cryptography;
using System.Text;
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
        public async Task Start()
        {
            var uri = new Uri("http://blog.cube-soft.jp/?feed=rss2");

            using (var mon = new Cube.Net.Rss.RssMonitor())
            {
                mon.Timeout = TimeSpan.FromMilliseconds(1000);
                mon.CacheDirectory = Results;
                mon.Add(uri);
                mon.Start();
                await Task.Delay(2000);
                mon.Stop();
            }

            var path = GetPath(uri, Results);
            Assert.That(IO.Exists(path), Is.True);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetPath
        ///
        /// <summary>
        /// 保存用パスを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private string GetPath(Uri uri, string directory)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(uri.ToString());
            var hash = md5.ComputeHash(data);
            var name = BitConverter.ToString(hash).ToLower().Replace("-", "");
            return IO.Combine(directory, name);
        }
    }
}
