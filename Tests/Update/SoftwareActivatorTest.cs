/* ------------------------------------------------------------------------- */
///
/// SoftwareActivatorTest.cs
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
using NUnit.Framework;

namespace Cube.Tests.Net.Update
{
    /* --------------------------------------------------------------------- */
    ///
    /// SoftwareActivatorTest
    ///
    /// <summary>
    /// SoftwareActivator のテストを行うクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Parallelizable]
    [TestFixture]
    class SoftwareActivatorTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsync_Primary
        /// 
        /// <summary>
        /// インストールログを送信するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public async Task RunAsync_Primary()
        {
            var expected = string.Format("{0}?{1}",
                Cube.Net.Update.SoftwareActivator.EndPoint,
                "utm_source=cube&utm_medium=cube_tests&utm_content=cubenews,ie_fav,ie_start"
            );

            var client = Create();
            client.Received += (s, e) =>
            {
                Assert.That(
                    e.Value.RequestMessage.RequestUri.ToString(),
                    Is.EqualTo(expected.ToString())
                );
            };
            await client.RunAsync();
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        /// 
        /// <summary>
        /// SoftwareActivator オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Cube.Net.Update.SoftwareActivator Create()
        {
            var dest = new Cube.Net.Update.SoftwareActivator();

            dest.Utm.Source = "cube";
            dest.Utm.Medium = "cube_tests";
            dest.Utm.Content = "cubenews,ie_fav,ie_start";
            dest.Required = true;
            dest.Secondary = null;
            dest.Version = new SoftwareVersion
            {
                Number    = new Version(1, 0, 0),
                Available = 3,
                Postfix   = string.Empty
            };

            return dest;
        }

        #endregion
    }
}
