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
namespace Cube.Net.Rss;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cube.Net.Http;
using Cube.Tasks.Extensions;

/* ------------------------------------------------------------------------- */
///
/// RssMonitor
///
/// <summary>
/// Provides functionality to periodically get RSS feeds from the
/// provided HTTP servers.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class RssMonitor : HttpMonitorBase<RssFeed>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// RssMonitor
    ///
    /// <summary>
    /// Initializes a new instance of the RssMonitor class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public RssMonitor() : base(new() { UseEntityTag = false })
    {
        _http      = new(Handler);
        Timeout    = _http.Timeout;
        RetryCount = 2;
    }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Contains
    ///
    /// <summary>
    /// Determines if the specified URL is monitored or not.
    /// </summary>
    ///
    /// <param name="uri">RSS feed URL</param>
    ///
    /// <returns>true for monitored.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public bool Contains(Uri uri) => _feeds.ContainsKey(uri);

    /* --------------------------------------------------------------------- */
    ///
    /// Register
    ///
    /// <summary>
    /// Registers the RSS feed URL to be monitored.
    /// </summary>
    ///
    /// <param name="uri">RSS feed URL</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Register(Uri uri) => Register(uri, null);

    /* --------------------------------------------------------------------- */
    ///
    /// Register
    ///
    /// <summary>
    /// Registers the RSS feed URL to be monitored.
    /// </summary>
    ///
    /// <param name="uri">RSS feed URL</param>
    /// <param name="last">Last checked date-time.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Register(Uri uri, DateTime? last)
    {
        if (Contains(uri)) return;
        _feeds.Add(uri, last);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Register
    ///
    /// <summary>
    /// Registers the collection of RSS feed URLs to be monitored.
    /// </summary>
    ///
    /// <param name="items">Collection of RSS feed URLs.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Register(IEnumerable<Uri> items) => Register(
        items.Select(e => new KeyValuePair<Uri, DateTime?>(e, default))
    );

    /* --------------------------------------------------------------------- */
    ///
    /// Register
    ///
    /// <summary>
    /// Registers the collection of RSS feed URL and last checked date-time
    /// pairs to be monitored.
    /// </summary>
    ///
    /// <param name="items">
    /// Collection of RSS feed URL and last checked date-time pairs.
    /// </param>
    ///
    /* --------------------------------------------------------------------- */
    public void Register(IEnumerable<KeyValuePair<Uri, DateTime?>> items)
    {
        foreach (var kv in items) Register(kv.Key, kv.Value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Remove
    ///
    /// <summary>
    /// Removes the specified RSS feed URL from monitored list.
    /// </summary>
    ///
    /// <param name="uri">RSS feed URL</param>
    ///
    /// <returns>true for success.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public bool Remove(Uri uri) => _feeds.Remove(uri);

    /* --------------------------------------------------------------------- */
    ///
    /// Clear
    ///
    /// <summary>
    /// Clears the monitored list.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Clear() => _feeds.Clear();

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the content of the RSS feed. When the update is complete,
    /// the result is notified via the Publish method.
    /// </summary>
    ///
    /// <param name="src">List of target feed URLs.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Update(params Uri[] src) => Update((IEnumerable<Uri>)src);

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the content of the RSS feed. Asynchronous processing is
    /// executed asynchronously, and when finished, the result is
    /// notified through the callback function registered in Subscribe
    /// method.
    /// </summary>
    ///
    /// <param name="src">List of target feed URLs.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Update(IEnumerable<Uri> src) => Task.Run(async () =>
    {
        Suspend();
        foreach (var uri in src)
        {
            try { await Publish(uri).ConfigureAwait(false); }
            catch (Exception err)
            {
                await PublishError(new Dictionary<Uri, Exception> {
                    { uri, err }
                }).ConfigureAwait(false);
            }
        }
        if (State == TimerState.Suspend) Start();
    }).Forget();

    /* --------------------------------------------------------------------- */
    ///
    /// GetTimestamp
    ///
    /// <summary>
    /// Gets the last checked time of the specified URL.
    /// </summary>
    ///
    /// <param name="uri">RSS feed URL.</param>
    ///
    /// <returns>Last checked time.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public DateTime? GetTimestamp(Uri uri) => _feeds[uri];

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// GetRequests
    ///
    /// <summary>
    /// Gets the sequence of HTTP request URLs.
    /// </summary>
    ///
    /// <returns>Sequence of HTTP Request URLs</returns>
    ///
    /* --------------------------------------------------------------------- */
    protected override IEnumerable<Uri> GetRequests() =>
        _feeds.OrderBy(e => e.Value).Select(e => e.Key).ToList();

    /* --------------------------------------------------------------------- */
    ///
    /// Publish
    ///
    /// <summary>
    /// Gets the HTTP response from the specified URL, parse it into
    /// an RSS feed, and publishes the result.
    /// </summary>
    ///
    /// <param name="uri">HTTP request URL.</param>
    ///
    /* --------------------------------------------------------------------- */
    protected override async Task Publish(Uri uri)
    {
        if (!Contains(uri)) return;

        Logger.Try(() => { if (_http.Timeout != Timeout) _http.Timeout = Timeout; });
        var sw = Stopwatch.StartNew();
        var dest = await _http.GetAsync(uri).ConfigureAwait(false);
        Logger.Debug($"{uri} ({sw.Elapsed})");

        _feeds[uri] = dest.LastChecked;
        await Publish(uri, dest).ConfigureAwait(false);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Publish
    ///
    /// <summary>
    /// Publishes the specified RSS feed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private async Task Publish(Uri uri, RssFeed feed)
    {
        foreach (var cb in Subscription)
        {
            try { await cb(uri, feed); }
            catch (Exception err) { Logger.Warn(err); }
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// PublishError
    ///
    /// <summary>
    /// Publishes error results.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private async Task PublishError(IDictionary<Uri, Exception> errors)
    {
        foreach (var kv in errors)
        {
            await Publish(kv.Key, new()
            {
                Uri         = kv.Key,
                LastChecked = DateTime.Now,
                Error       = kv.Value,
            }).ConfigureAwait(false);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OnError
    ///
    /// <summary>
    /// Occurs when some errors are detected.
    /// </summary>
    ///
    /// <param name="errors">Error list.</param>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnError(IDictionary<Uri, Exception> errors) =>
        PublishError(errors).Forget();

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
    private readonly RssClient _http;
    private readonly Dictionary<Uri, DateTime?> _feeds = new();
    #endregion
}
