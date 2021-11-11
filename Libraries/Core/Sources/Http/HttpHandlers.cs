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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpHandler
    ///
    /// <summary>
    /// Provides functionality to treat HTTP request/response headers.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HttpHandler : HttpClientHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpHandler
        ///
        /// <summary>
        /// Initializes a new instance of the HttpHandler class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HttpHandler()
        {
            if (SupportsAutomaticDecompression)
            {
                AutomaticDecompression = DecompressionMethods.Deflate |
                                         DecompressionMethods.GZip;
            }
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// ConnectionClose
        ///
        /// <summary>
        /// Gets or sets the value of ConnectionClose header.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool ConnectionClose { get; set; } = true;

        /* ----------------------------------------------------------------- */
        ///
        /// UseEntityTag
        ///
        /// <summary>
        /// Gets or sets a value indicating whether to use EntityTag (ETag).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool UseEntityTag { get; set; } = true;

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTag
        ///
        /// <summary>
        /// Gets or sets the value of EntityTag (ETag).
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string EntityTag { get; protected set; }

        /* ----------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string UserAgent { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SendAsync
        ///
        /// <summary>
        /// Sends an HTTP request asynchronously.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SetConnectionClose(request.Headers);
            SetEntityTag(request.Headers);
            SetUserAgent(request.Headers);

            var response = await base.SendAsync(request, cancellationToken);

            GetEntityTag(response.Headers);

            return response;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetConnectionClose
        ///
        /// <summary>
        /// Sets the ConnectionClose value to the HTTP request header.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetConnectionClose(HttpRequestHeaders headers) => GetType().LogWarn(() =>
        {
            if (headers.ConnectionClose.HasValue &&
                headers.ConnectionClose == ConnectionClose) return;
            headers.ConnectionClose = ConnectionClose;
        });

        /* ----------------------------------------------------------------- */
        ///
        /// SetUserAgent
        ///
        /// <summary>
        /// Sets the UserAgent value to the HTTP request header.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetUserAgent(HttpRequestHeaders headers) => GetType().LogWarn(() =>
        {
            if (string.IsNullOrEmpty(UserAgent)) return;
            headers.UserAgent.ParseAdd(UserAgent);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// SetEntityTag
        ///
        /// <summary>
        /// Sets the EntityTag value to the HTTP request header.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetEntityTag(HttpRequestHeaders headers) => GetType().LogWarn(() =>
        {
            if (!UseEntityTag || string.IsNullOrEmpty(EntityTag)) return;
            var etag = EntityTagHeaderValue.Parse(EntityTag);
            headers.IfNoneMatch.Add(etag);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// GetEntityTag
        ///
        /// <summary>
        /// Gets the EntityTag value from the HTTP response header.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void GetEntityTag(HttpResponseHeaders headers) =>
            EntityTag = headers?.ETag?.Tag ?? string.Empty;

        #endregion
    }
}
