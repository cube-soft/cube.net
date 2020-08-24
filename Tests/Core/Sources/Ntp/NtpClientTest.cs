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
using System.Collections.Generic;
using System.Net.Sockets;
using Cube.Net.Ntp;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpClientTest
    ///
    /// <summary>
    /// Tests the NtpClient class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class NtpClientTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Tests the default constructor and confirms values of properties.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Create()
        {
            using (var client = new NtpClient())
            {
                Assert.That(client.Host.HostName, Does.StartWith("time.microsoft"));
                Assert.That(client.Host.AddressList.Length, Is.AtLeast(1));
                Assert.That(client.Port, Is.EqualTo(123));
                Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create_SocketException
        ///
        /// <summary>
        /// Tests the constructor with the non-existent NTP server.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Create_SocketException()
        {
            var src = "404.not.found";
            Assert.That(() => new NtpClient(src), Throws.TypeOf<SocketException>());
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// Tests the GetAsync method.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCaseSource(nameof(TestCases))]
        public void GetAsync(string src, uint version, uint poll, Stratum stratum)
        {
            using (var client = new NtpClient(src))
            {
                var pkt = client.GetAsync().Result;
                Assert.That(pkt.IsValid,            Is.True);
                Assert.That(pkt.LeapIndicator,      Is.EqualTo(LeapIndicator.NoWarning));
                Assert.That(pkt.Version,            Is.EqualTo(version));
                Assert.That(pkt.Mode,               Is.EqualTo(NtpMode.Server));
                Assert.That(pkt.Stratum,            Is.EqualTo(stratum));
                Assert.That(pkt.PollInterval,       Is.EqualTo(poll));
                Assert.That(pkt.Precision,          Is.GreaterThan(0.0).And.LessThan(1.0));
                Assert.That(pkt.RootDelay,          Is.GreaterThanOrEqualTo(0.0));
                Assert.That(pkt.RootDispersion,     Is.GreaterThanOrEqualTo(0.0));
                Assert.That(pkt.ReferenceID,        Is.Not.Null.And.Not.Empty);
                Assert.That(pkt.ReferenceTimestamp, Is.Not.Null);
                Assert.That(pkt.KeyID,              Is.Empty);
                Assert.That(pkt.MessageDigest,      Is.Empty);
                Assert.That(pkt.NetworkDelay.TotalSeconds,     Is.GreaterThan(0.0).And.LessThan(1.0));
                Assert.That(pkt.LocalClockOffset.TotalSeconds, Is.EqualTo(0).Within(60));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync_Timeout
        ///
        /// <summary>
        /// Tests the GetAsync method with a very short timeout.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void GetAsync_Timeout()
        {
            using (var client = new NtpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromMilliseconds(1);
                    client.GetAsync().Wait();
                    Assert.Ignore("GetAsync success bofore timeout");
                }
                catch (AggregateException err)
                {
                    Assert.That(err.InnerException, Is.TypeOf<SocketException>());
                }
            }
        }

        #endregion

        #region TestCases

        /* ----------------------------------------------------------------- */
        ///
        /// TestCases
        ///
        /// <summary>
        /// Gets the test cases.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData(
                    "ntp.cube-soft.jp",
                    3u,
                    0u,
                    Stratum.SecondaryReference
                );
            }
        }

        #endregion
    }
}
