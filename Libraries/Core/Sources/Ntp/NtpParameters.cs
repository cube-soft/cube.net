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

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// NtpMode
    ///
    /// <summary>
    /// Specifies the NTP operation mode.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum NtpMode
    {
        /// <summary>Unknown (Reserved).</summary>
        Unknown = 0,   // 0, 6, 7 - Reserved
        /// <summary>Symmetric active</summary>
        SymmetricActive = 1,
        /// <summary>Symmetric pasive</summary>
        SymmetricPassive = 2,
        /// <summary>Client.</summary>
        Client = 3,
        /// <summary>Server.</summary>
        Server = 4,
        /// <summary>Broadcast.</summary>
        Broadcast = 5,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// LeapIndicator
    ///
    /// <summary>
    /// Specifies the leap indicator (LI).
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum LeapIndicator : uint
    {
        /// <summary>No warning.</summary>
        NoWarning = 0,
        /// <summary>There are 61 seconds in this minute.</summary>
        LastMinute61 = 1,
        /// <summary>There are 59 seconds in this minute.</summary>
        LastMinute59 = 2,
        /// <summary>Alarm.</summary>
        Alarm = 3,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Stratum
    ///
    /// <summary>
    /// Specifies the stratum.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum Stratum
    {
        /// <summary>Unspecified or Unavailable</summary>
        Unspecified,
        /// <summary>Primary reference (e.g. radio-clock)</summary>
        PrimaryReference,
        /// <summary>Secondary reference (via NTP or SNTP)</summary>
        SecondaryReference,
        /// <summary>reserved (16-255)</summary>
        Reserved,
    }
}
