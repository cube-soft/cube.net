/* ------------------------------------------------------------------------- */
///
/// PacketTest.cs
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
using NUnit.Framework;

namespace Cube.Tests.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// PacketTest
    ///
    /// <summary>
    /// Ntp.Packet のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class PacketTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Properties
        /// 
        /// <summary>
        /// 各種プロパティのテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        #region Properties

        [TestCase(Cube.Net.Ntp.LeapIndicator.NoWarning)]
        public void LeapIndicator(Cube.Net.Ntp.LeapIndicator expected)
        {
            Assert.That(
                Create().LeapIndicator,
                Is.EqualTo(expected)
            );
        }

        [TestCase(3)]
        public void Version(int expected)
        {
            Assert.That(
                Create().Version,
                Is.EqualTo(expected)
            );
        }

        [TestCase(Cube.Net.Ntp.Mode.Client)]
        public void Mode(Cube.Net.Ntp.Mode expected)
        {
            Assert.That(
                Create().Mode,
                Is.EqualTo(expected)
            );
        }

        [TestCase(Cube.Net.Ntp.Stratum.Unspecified)]
        public void Stratum(Cube.Net.Ntp.Stratum expected)
        {
            Assert.That(
                Create().Stratum,
                Is.EqualTo(expected)
            );
        }

        [TestCase(0)]
        public void PollInterval(int expected)
        {
            Assert.That(
                Create().PollInterval,
                Is.EqualTo(expected)
            );
        }

        [TestCase(1.0)]
        public void Precision(double expected)
        {
            Assert.That(
                Create().Precision,
                Is.EqualTo(expected).Within(0.01)
            );
        }

        [TestCase(0.0)]
        public void RootDelay(double expected)
        {
            Assert.That(
                Create().RootDelay,
                Is.EqualTo(expected).Within(0.01)
            );
        }

        [TestCase(0.0)]
        public void RootDispersion(double expected)
        {
            Assert.That(
                Create().RootDispersion,
                Is.EqualTo(expected).Within(0.01)
            );
        }

        [Test]
        public void ReferenceID_IsNullOrEmpty()
        {
            Assert.That(
                Create().ReferenceID,
                Is.Null.Or.Empty
            );
        }

        [Test]
        public void KeyID_IsNullOrEmpty()
        {
            Assert.That(
                Create().KeyID,
                Is.Null.Or.Empty
            );
        }

        [Test]
        public void MessageDigest_IsNullOrEmpty()
        {
            Assert.That(
                Create().MessageDigest,
                Is.Null.Or.Empty
            );
        }

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// New_TooManyBytes_Throws
        /// 
        /// <summary>
        /// NTP パケットの初期化に失敗するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void New_TooManyBytes_Throws()
        {
            Assert.That(
                () => new Cube.Net.Ntp.Packet(new byte[47]),
                Throws.TypeOf<ArgumentException>()
            );
        }

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        /// 
        /// <summary>
        /// NTP パケットを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Cube.Net.Ntp.Packet Create()
        {
            return new Cube.Net.Ntp.Packet();
        }

        #endregion
    }
}
