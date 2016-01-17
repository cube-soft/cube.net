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
    /// Cube.Net.Ntp.Packet のテストをするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class PacketTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// CreatePacket
        /// 
        /// <summary>
        /// 送信用の NTP パケットを生成するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void CreatePacket()
        {
            Assert.DoesNotThrow(() =>
            {
                var packet = new Cube.Net.Ntp.Packet();
                Assert.That(packet.IsValid, Is.True);
                Assert.That(packet.LeapIndicator, Is.EqualTo(Cube.Net.Ntp.LeapIndicator.NoWarning));
                Assert.That(packet.Version, Is.EqualTo(3));
                Assert.That(packet.Mode, Is.EqualTo(Cube.Net.Ntp.Mode.Client));
                Assert.That(packet.Stratum, Is.EqualTo(Cube.Net.Ntp.Stratum.Unspecified));
                Assert.That(packet.PollInterval, Is.EqualTo(0));
                Assert.That(packet.Precision, Is.EqualTo(1.0).Within(0.01));
                Assert.That(packet.RootDelay, Is.EqualTo(0.0).Within(0.01));
                Assert.That(packet.RootDispersion, Is.EqualTo(0.0).Within(0.01));
                Assert.That(packet.ReferenceID, Is.Null.Or.Empty);
                Assert.That(packet.KeyID, Is.Null.Or.Empty);
                Assert.That(packet.MessageDigest, Is.Null.Or.Empty);
                Assert.That(packet.TransmitTimestamp.Date, Is.EqualTo(packet.CreationTime.Date));
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreatePacketException
        /// 
        /// <summary>
        /// NTP パケットの初期化に失敗するテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void CreatePacketException()
        {
            var err = Assert.Throws<ArgumentException>(() =>
            {
                var packet = new Cube.Net.Ntp.Packet(new byte[47]);
            });
        }
    }
}
