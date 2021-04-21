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

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkMonitor
    ///
    /// <summary>
    /// Represents the base class for periodically invoking network
    /// transmissions.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class NetworkMonitor : WakeableTimerBase
    {
        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// RetryCount
        ///
        /// <summary>
        /// Gets or sets the retry count when the transmission was failed.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public int RetryCount { get; set; } = 3;

        /* --------------------------------------------------------------------- */
        ///
        /// RetryInterval
        ///
        /// <summary>
        /// Gets or sets the retry interval when the transmission was failed.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// Gets or sets the timeout of the transmission.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

        #endregion
    }
}
