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
using System;
using System.Net;
using System.Net.Http;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpClientFactory
    ///
    /// <summary>
    /// Provides functionality to create a HTTP client object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class HttpClientFactory
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// DefaultTimeout
        ///
        /// <summary>
        /// Gets the default timeout value.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Creates a new instance of the HttpClient class with the specified
        /// arguments.
        /// </summary>
        ///
        /// <param name="handler">HTTP handler.</param>
        /// <param name="timeout">Timeout value.</param>
        ///
        /// <returns>HttpClient object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(HttpClientHandler handler, TimeSpan timeout)
        {
            var http = (handler != null) ? new HttpClient(handler, false) : new HttpClient();
            http.Timeout = timeout;
            return http;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Creates a new instance of the HttpClient class with the specified
        /// arguments.
        /// </summary>
        ///
        /// <param name="handler">HTTP handler.</param>
        ///
        /// <returns>HttpClient object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(HttpClientHandler handler) => Create(handler, DefaultTimeout);

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Creates a new instance of the HttpClient class with the specified
        /// arguments.
        /// </summary>
        ///
        /// <param name="timeout">Timeout value.</param>
        ///
        /// <returns>HttpClient object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create(TimeSpan timeout)
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate |
                                                 DecompressionMethods.GZip;
            }

            return Create(handler, timeout);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// Creates a new instance of the HttpClient class.
        /// </summary>
        ///
        /// <returns>HttpClient object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static HttpClient Create() => Create(DefaultTimeout);

        #endregion
    }
}
