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
using System.Threading.Tasks;

namespace Cube.Net.Http.Synchronous
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitorExtension
    ///
    /// <summary>
    /// Provides extended methods of HttpMonitorBase(T) and inherited
    /// classes.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class HttpMonitorExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// SubscribeSync
        ///
        /// <summary>
        /// Sets the specified action to the monitor.
        /// </summary>
        ///
        /// <param name="src">HTTP monitor object.</param>
        /// <param name="callback">User action.</param>
        ///
        /// <returns>Object to remove from the subscription.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IDisposable SubscribeSync<TValue>(this HttpMonitorBase<TValue> src,
            Action<Uri, TValue> callback) => src.Subscribe((uri, value) =>
        {
            callback(uri, value);
            return TaskEx.FromResult(0);
        });

        #endregion
    }
}
