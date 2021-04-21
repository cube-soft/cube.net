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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cube.Collections;
using Cube.Mixin.Logging;

namespace Cube.Net.Http
{
    #region HttpMonitorBase

    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitorBase
    ///
    /// <summary>
    /// Represents the base class for periodic communication with HTTP
    /// servers.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class HttpMonitorBase<TValue> : NetworkMonitor
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitorBase
        ///
        /// <summary>
        /// Initializes a new instance of the HttpMonitorBase class with
        /// the specified handler.
        /// </summary>
        ///
        /// <param name="handler">HTTP handler.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected HttpMonitorBase(HeaderHandler handler)
        {
            Timeout = TimeSpan.FromSeconds(2);
            Handler = handler;
        }

        #endregion

        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// UserAgent
        ///
        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public string UserAgent
        {
            get => Handler.UserAgent;
            set => Handler.UserAgent = value;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// Handler
        ///
        /// <summary>
        /// Gets the HTTP handler.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        protected HeaderHandler Handler { get; }

        /* --------------------------------------------------------------------- */
        ///
        /// Subscription
        ///
        /// <summary>
        /// Gets the subscription.
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        protected Subscription<HttpAsyncAction<TValue>> Subscription { get; } = new();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Subscribe
        ///
        /// <summary>
        /// Sets the specified asynchronous action to the monitor.
        /// </summary>
        ///
        /// <param name="callback">Asynchronous user action.</param>
        ///
        /// <returns>Object to remove from the subscription.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IDisposable Subscribe(HttpAsyncAction<TValue> callback) =>
            Subscription.Subscribe(callback);

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// Gets the HTTP response from the specified URL and publishes
        /// the result.
        /// </summary>
        ///
        /// <param name="uri">HTTP request URL.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected abstract Task Publish(Uri uri);

        /* ----------------------------------------------------------------- */
        ///
        /// OnError
        ///
        /// <summary>
        /// Occurs when some errors are detected.
        /// </summary>
        ///
        /// <param name="errors">Error list.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnError(IDictionary<Uri, Exception> errors)
        {
            foreach (var e in errors) this.LogWarn(e.Key.ToString(), e.Value.Message);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequests
        ///
        /// <summary>
        /// Gets the sequence of HTTP request URLs.
        /// </summary>
        ///
        /// <returns>Sequence of HTTP Request URLs</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected abstract IEnumerable<Uri> GetRequests();

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
            if (disposing) Handler.Dispose();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnTick
        ///
        /// <summary>
        /// Occurs when the timer is expired.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task OnTick()
        {
            if (Subscription.Count <= 0) return;
            if (!Network.Available) { this.LogDebug("Network not available"); return; }

            async Task task(IEnumerable<Uri> s, IDictionary<Uri, Exception> e)
            {
                foreach (var uri in s)
                {
                    try { await Publish(uri).ConfigureAwait(false); }
                    catch (Exception err) { e.Add(uri, err); }
                }
            }

            var src = GetRequests().ToList();
            var errors = new Dictionary<Uri, Exception>();
            await task(src, errors).ConfigureAwait(false);

            for (var i = 0; i < RetryCount && errors.Count > 0; ++i)
            {
                await Task.Delay(RetryInterval).ConfigureAwait(false);
                var retry = errors.Keys.ToList();
                errors.Clear();
                await task(retry, errors).ConfigureAwait(false);
            }

            if (errors.Count > 0) OnError(errors);
        }

        #endregion
    }

    #endregion

    #region HttpMonitor

    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitorBase
    ///
    /// <summary>
    /// Provides functionality to periodically  communicate with the
    /// provided HTTP server.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HttpMonitor<TValue> : HttpMonitorBase<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the HttpMonitor class with the
        /// specified converting function.
        /// </summary>
        ///
        /// <param name="func">
        /// Function to convert to the provided type.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(Func<Stream, TValue> func) :
            this(new ContentConverter<TValue>(func)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the HttpMonitor class with the
        /// specified converter object.
        /// </summary>
        ///
        /// <param name="converter">Converter object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(IContentConverter<TValue> converter) :
            this(new ContentHandler<TValue>(converter)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// Initializes a new instance of the HttpMonitor class with the
        /// specified handler.
        /// </summary>
        ///
        /// <param name="handler">HTTP handler.</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(ContentHandler<TValue> handler) : base(handler)
        {
            _http = HttpClientFactory.Create(handler);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// Gets or sets the HTTP request URL.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequests
        ///
        /// <summary>
        /// Gets the sequence of HTTP request URLs.
        /// </summary>
        ///
        /// <returns>Sequence of HTTP Request URLs.</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected override IEnumerable<Uri> GetRequests() => new[] { Uri };

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        ///
        /// <summary>
        /// Gets the HTTP response from the specified URL and publishes
        /// the result.
        /// </summary>
        ///
        /// <param name="uri">HTTP request URL.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task Publish(Uri uri)
        {
            this.LogWarn(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });
            using var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
            var code = response.StatusCode;

            if (response.Content is HttpValueContent<TValue> cvt)
            {
                foreach (var cb in Subscription)
                {
                    try { await cb(uri, cvt.Value).ConfigureAwait(false); }
                    catch (Exception err) { this.LogWarn(err); }
                }
            }
            else throw new InvalidOperationException($"Convert failed ({(int)code} {code})");
        }

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
            try { if (disposing) _http.Dispose(); }
            finally { base.Dispose(disposing); }
        }

        #endregion

        #region Fields
        private readonly HttpClient _http;
        #endregion
    }

    #endregion

    #region HttpAsyncAction

    /* --------------------------------------------------------------------- */
    ///
    /// NtpAsyncAction
    ///
    /// <summary>
    /// Represents the method to invoke as an asynchronous method.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public delegate Task HttpAsyncAction<TValue>(Uri uri, TValue value);

    #endregion
}
