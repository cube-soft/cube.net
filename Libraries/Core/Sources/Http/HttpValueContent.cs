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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpValueContent(TValue)
    ///
    /// <summary>
    /// Represents the result of converting HttpContent object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal sealed class HttpValueContent<TValue> : HttpContent
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpValueContent
        ///
        /// <summary>
        /// Initializes a new instance of the HttpValueContent class with
        /// the specified arguments.
        /// </summary>
        ///
        /// <param name="src">Source HttpContent object.</param>
        /// <param name="value">Converting result.</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpValueContent(HttpContent src, TValue value)
        {
            Source = src;
            Value  = value;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Source
        ///
        /// <summary>
        /// Gets the source HttpContent object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HttpContent Source { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// Gets the converting result.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TValue Value { get; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// Releases the unmanaged resources used by the WakeableTimer
        /// and optionally releases the managed resources.
        /// </summary>
        ///
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_disposed) return;
                if (disposing) Source.Dispose();
            }
            finally
            {
                _disposed = true;
                base.Dispose(disposing);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SerializeToStreamAsync
        ///
        /// <summary>
        /// Asynchronously serializes HTTP content and copies it to the
        /// Stream object.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
            Source.CopyToAsync(stream, context);

        /* ----------------------------------------------------------------- */
        ///
        /// TryComputeLength
        ///
        /// <summary>
        /// Tries to get the number of bytes in the HTTP content.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override bool TryComputeLength(out long length)
        {
            length = Source.Headers?.ContentLength ?? -1;
            return length != -1;
        }

        #endregion

        #region Fields
        private bool _disposed = false;
        #endregion
    }
}
