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
namespace Cube.Net.Http;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

/* ------------------------------------------------------------------------- */
///
/// HttpMonitor(TValue)
///
/// <summary>
/// Provides functionality to periodically  communicate with the
/// provided HTTP server.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class HttpMonitor<TValue> : HttpMonitorBase<TValue>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
    public HttpMonitor(Func<Stream, TValue> func) :
        this(new ContentConverter<TValue>(func)) { }

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
    public HttpMonitor(IContentConverter<TValue> converter) :
        this(new HttpContentHandler<TValue>(converter)) { }

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
    public HttpMonitor(HttpContentHandler<TValue> handler) : base(handler)
    {
        _http = HttpClientFactory.Create(handler);
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Uri
    ///
    /// <summary>
    /// Gets or sets the HTTP request URL.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public Uri Uri { get; set; }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// GetRequests
    ///
    /// <summary>
    /// Gets the sequence of HTTP request URLs.
    /// </summary>
    ///
    /// <returns>Sequence of HTTP Request URLs.</returns>
    ///
    /* --------------------------------------------------------------------- */
    protected override IEnumerable<Uri> GetRequests() => new[] { Uri };

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
    protected override async Task Publish(Uri uri)
    {
        Logger.Warn(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });
        using var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
        var code = response.StatusCode;

        if (response.Content is HttpValueContent<TValue> cvt)
        {
            foreach (var cb in Subscription)
            {
                try { await cb(uri, cvt.Value).ConfigureAwait(false); }
                catch (Exception err) { Logger.Warn(err); }
            }
        }
        else throw new InvalidOperationException($"Convert failed ({(int)code} {code})");
    }

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
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
