/* ------------------------------------------------------------------------- */
///
/// MessageTest.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using Cube.Net.Update;

namespace Cube.Tests.Net.Update
{
    /* --------------------------------------------------------------------- */
    ///
    /// MessageTest
    ///
    /// <summary>
    /// Update.Message に関するテストを行うクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class MessageTest
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// EndPoint
        /// 
        /// <summary>
        /// テスト用の URL を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri EndPoint => new Uri("http://www.cube-soft.jp/cubelab/cubenews/update.php");

        #endregion

        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// GetUpdateMessageAsync
        /// 
        /// <summary>
        /// アップデート用メッセージを非同期で取得するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("1.0.2", "", "")]
        [TestCase("1.0.0", "flag", "install")]
        public async Task GetUpdateMessageAsync(string version, string key, string value)
        {
            var query = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(key)) query.Add(key, value);

            var result = await Create().GetUpdateMessageAsync(
                EndPoint,
                new SoftwareVersion(version),
                query
            );

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Version, Is.EqualTo(version));
            Assert.That(result.Notify, Is.False);
            Assert.That(result.Uri, Is.EqualTo(new Uri("http://s.cube-soft.jp/widget/")));
            Assert.That(result.Text.Length, Is.AtLeast(1));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetUpdateMessageAsync_IsNull
        /// 
        /// <summary>
        /// アップデート用メッセージが見つからない場合のテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase("9.9.9")]
        public async Task GetUpdateMessageAsync_IsNull(string version)
        {
            var result = await Create().GetUpdateMessageAsync(
                EndPoint,
                new SoftwareVersion(version)
            );

            Assert.That(result, Is.Null);
        }

        #endregion

        #region Other private methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        /// 
        /// <summary>
        /// HttpClient オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private System.Net.Http.HttpClient Create()
            => new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        #endregion
    }
}
