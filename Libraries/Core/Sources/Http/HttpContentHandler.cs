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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpContentHandler(TValue)
    ///
    /// <summary>
    /// Provides functionality to convert from the HttpContent object to
    /// the provided type.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HttpContentHandler<TValue> : HttpHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpContentHandler
        ///
        /// <summary>
        /// Initializes a new instance of the HttpContentHandler class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HttpContentHandler() { }

        /* ----------------------------------------------------------------- */
        ///
        /// ContentHandler
        ///
        /// <summary>
        /// Initializes a new instance of the HttpContentHandler class
        /// with the specified converter.
        /// </summary>
        ///
        /// <param name="converter">Converter object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpContentHandler(IContentConverter<TValue> converter) : this()
        {
            Converter = converter;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ContentHandler
        ///
        /// <summary>
        /// Initializes a new instance of the HttpContentHandler class
        /// with the specified conversion function.
        /// </summary>
        ///
        /// <param name="func">
        /// Function to convert the HttpContent object.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpContentHandler(Func<Stream, TValue> func) :
            this(new ContentConverter<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Converter
        ///
        /// <summary>
        /// Gets or sets the converter object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IContentConverter<TValue> Converter { get; set; }

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
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response == null || Converter == null) return response;
            if (response.IsSuccessStatusCode)
            {
                await response.Content.LoadIntoBufferAsync();
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                response.Content = new HttpValueContent<TValue>(
                    response.Content,
                    Converter.Convert(stream)
                );
            }
            return response;
        }

        #endregion
    }
}
