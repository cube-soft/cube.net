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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Network
    ///
    /// <summary>
    /// Represents the condition of the network in the current machine.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class Network
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Status
        ///
        /// <summary>
        /// Gets the network status.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static OperationalStatus Status
        {
            get
            {
                var ns   = GetNetworkInterfaces();
                var dest = ns.FirstOrDefault(e => e.OperationalStatus == OperationalStatus.Up) ??
                           ns.FirstOrDefault(e => e.OperationalStatus == OperationalStatus.Testing);
                return dest?.OperationalStatus ?? OperationalStatus.Down;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Available
        ///
        /// <summary>
        /// Gets a value indicating whether the network is available.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static bool Available =>
            GetNetworkInterfaces().Any(e => e.OperationalStatus == OperationalStatus.Up);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// DisableOptions
        ///
        /// <summary>
        /// Disables some network options.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void DisableOptions()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            WebRequest.DefaultWebProxy = null;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetNetworkInterfaces
        ///
        /// <summary>
        /// Gets the collection of network interfaces in the current
        /// machine.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<NetworkInterface> GetNetworkInterfaces() =>
            NetworkInterface.GetAllNetworkInterfaces().Where(e =>
                e.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                e.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
               !e.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase)
            );

        #endregion
    }
}