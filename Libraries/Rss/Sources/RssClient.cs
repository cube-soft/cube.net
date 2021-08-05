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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssClient
    ///
    /// <summary>
    /// Provides functionality to get the RSS feed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssClient : DisposableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssClient
        ///
        /// <summary>
        /// Initializes a new instance of the RssClient class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssClient() : this(null) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssClient
        ///
        /// <summary>
        /// Initializes a new instance of the RssClient class with the
        /// specified handler..
        /// </summary>
        ///
        /// <param name="handler">HTTP handler.</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssClient(HttpClientHandler handler)
        {
            _http = HttpClientFactory.Create(handler, TimeSpan.FromSeconds(10));
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// Gets or setsh the timeout value.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get => _http.Timeout;
            set => _http.Timeout = value;
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Redirected
        ///
        /// <summary>
        /// Occurs when redirecting. The event occurs when the alternate
        /// meta tag exists in the received HTML.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueChangedEventHandler<Uri> Redirected;

        /* ----------------------------------------------------------------- */
        ///
        /// OnRedirected
        ///
        /// <summary>
        /// Raises the Redirected event.
        /// </summary>
        ///
        /// <param name="e">Event arguments.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnRedirected(ValueChangedEventArgs<Uri> e) =>
            Redirected?.Invoke(this, e);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// Get RSS feeds asynchronously.
        /// </summary>
        ///
        /// <param name="uri">URL to get feeds.</param>
        ///
        /// <returns>RssFeed object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<RssFeed> GetAsync(Uri uri)
        {
            var opt = HttpCompletionOption.ResponseContentRead;
            using var response = await _http.GetAsync(uri, opt).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            await response.Content.LoadIntoBufferAsync();
            var stream = await response.Content.ReadAsStreamAsync();
            return await ParseAsync(uri, stream).ConfigureAwait(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// Releases the unmanaged resources used by the object and
        /// optionally releases the managed resources.
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
            if (disposing) _http.Dispose();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ParseAsync
        ///
        /// <summary>
        /// Reads data from the specified stream and parses the RSS feeds.
        /// </summary>
        ///
        /// <remarks>
        /// If the communication result is HTML and the alternate meta tag
        /// exists, HTTP communication is attempted again for the
        /// corresponding URL.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<RssFeed> ParseAsync(Uri uri, System.IO.Stream content)
        {
            try
            {
                var root = XDocument.Load(content).Root;
                var version = root.GetRssVersion();
                if (version != RssVersion.Unknown)
                {
                    var dest = RssParser.Parse(root, version);
                    if (dest != null) dest.Uri = uri;
                    return dest;
                }
            }
            catch { /* try redirect */ }

            content.Seek(0, System.IO.SeekOrigin.Begin);
            var cvt = content.GetRssUris().FirstOrDefault();
            if (cvt == null) return default;

            OnRedirected(ValueEventArgs.Create(uri, cvt));
            return await GetAsync(cvt).ConfigureAwait(false);
        }

        #endregion

        #region Fields
        private readonly HttpClient _http;
        #endregion
    }
}
