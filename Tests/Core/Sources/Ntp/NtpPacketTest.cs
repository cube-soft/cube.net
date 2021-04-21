﻿/* ------------------------------------------------------------------------- */
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
using Cube.Net.Ntp;
using NUnit.Framework;

namespace Cube.Net.Tests.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpPacketTest
    ///
    /// <summary>
    /// Ntp.Packet のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpPacketTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Create_ArgumentException
        ///
        /// <summary>
        /// NTP パケットの初期化に失敗するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Create_ArgumentException()
        {
            Assert.That(() => new NtpPacket(new byte[47]), Throws.ArgumentException);
        }

        #endregion
    }
}
