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
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkHelper
    /// 
    /// <summary>
    /// ネットワークのテストに関連する処理を定義するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class NetworkHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// SetUp
        /// 
        /// <summary>
        /// 各テストの直前に実行されます。
        /// </summary>
        /// 
        /// <remarks>
        /// ネットワークの利用可能状況を取得し、利用不可能な場合は
        /// Ignore を実行します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        [SetUp]
        public virtual void SetUp()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Assert.Ignore("Network is not available");
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WaitAsync
        /// 
        /// <summary>
        /// 一定時間待機します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected async Task WaitAsync(CancellationToken token)
        {
            try { await Task.Delay(5000, token); }
            catch (TaskCanceledException /* err */) { /* OK */ }
        }
    }
}
