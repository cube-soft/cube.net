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
namespace Cube.Net.Rss.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using Cube.Collections.Extensions;
using Cube.DataContract;
using Cube.FileSystem;

/* ------------------------------------------------------------------------- */
///
/// RssExtension
///
/// <summary>
/// Provides extended methods for the RSS entry.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public static class RssExtension
{
    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Backup
    ///
    /// <summary>
    /// Create a backup of the specified files.
    /// </summary>
    ///
    /// <param name="src">Path of the source file.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Backup(string src)
    {
        var info = new Entity(src);
        var dir  = Io.Combine(info.DirectoryName, "Backup");
        var dest = Io.Combine(dir, $"{DateTime.Now:yyyyMMdd}{info.Extension}");

        if (Io.Exists(dest)) return;
        Io.Copy(src, dest, true);

        foreach (var f in Io.GetFiles(dir).OrderByDescending(e => e).Skip(30))
        {
            Logger.Try(() => Io.Delete(f));
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Load
    ///
    /// <summary>
    /// Reads RSS entries from the specified file.
    /// </summary>
    ///
    /// <param name="src">Path of the source file.</param>
    /// <param name="dispatcher">Context dispatcher.</param>
    ///
    /// <returns>Collection of RSS entries.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<RssCategory> Load(string src, Dispatcher dispatcher) =>
        Io.Exists(src) ?
        Format.Json.Deserialize<List<RssCategory.Json>>(src).Select(e => e.Convert(dispatcher)) :
        Enumerable.Empty<RssCategory>();

    /* --------------------------------------------------------------------- */
    ///
    /// Save
    ///
    /// <summary>
    /// Saves RSS entries to the specified file.
    /// </summary>
    ///
    /// <param name="src">Collection of RSS entries.</param>
    /// <param name="dest">Path of the destination file.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Save(this IEnumerable<RssCategory> src, string dest) =>
        Format.Json.Serialize(dest, src.Select(e => new RssCategory.Json(e)));

    /* --------------------------------------------------------------------- */
    ///
    /// Read
    ///
    /// <summary>
    /// Sets all articles of the specified object as read.
    /// </summary>
    ///
    /// <param name="src">RSS entry or category.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Read(this IRssEntry src)
    {
        if (src is RssCategory rc) ReadCore(rc);
        else if (src is RssEntry re) ReadCore(re);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Read
    ///
    /// <summary>
    /// Sets the specified entry as read.
    /// </summary>
    ///
    /// <param name="src">RssEntry object.</param>
    /// <param name="item">RssItem object.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Read(this RssEntry src, RssItem item)
    {
        if (item != null && item.Status != RssItemStatus.Read)
        {
            src.Count = Math.Max(src.UnreadItems.Count() - 1, 0);
            item.Status = RssItemStatus.Read;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Update
    ///
    /// <summary>
    /// Updates the specified RSS entry.
    /// </summary>
    ///
    /// <param name="dest">Target RSS entry.</param>
    /// <param name="src">Update information.</param>
    ///
    /// <returns>Number of new articles.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static int Update(this RssEntry dest, RssFeed src)
    {
        var threshold = dest.LastChecked;
        dest.LastChecked = src.LastChecked;

        if (src.Error != null)
        {
            Logger.Debug($"{dest.Uri} ({src.Error.GetType().Name})");
            src.Title = dest.Title;
            return 0;
        }

        var received = src.Items.Shrink(threshold).ToList();
        foreach (var item in received) dest.Items.Insert(0, item);

        dest.Description   = src.Description;
        dest.Count         = dest.UnreadItems.Count();
        dest.Link          = src.Link;
        dest.LastPublished = src.LastPublished;

        return received.Count;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Shrink
    ///
    /// <summary>
    /// Deletes unnecessary items based on the update date.
    /// </summary>
    ///
    /// <param name="src">List of new artices.</param>
    /// <param name="threshold">Threshold date-time.</param>
    ///
    /// <remarks>
    /// The method is assumed that the list of new articles is in descending
    /// order by date of publication.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<RssItem> Shrink(this IEnumerable<RssItem> src, DateTime? threshold) =>
        src.Reverse()
           .SkipWhile(e => e.PublishTime <= threshold);

    /* --------------------------------------------------------------------- */
    ///
    /// Shrink
    ///
    /// <summary>
    /// Deletes read articles.
    /// </summary>
    ///
    /// <param name="src">Target RSS entry.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Shrink(this RssEntry src)
    {
        for (var i = src.Items.Count - 1; i >= 0; --i)
        {
            if (src.Items[i].Status == RssItemStatus.Read) src.Items.RemoveAt(i);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Expand
    ///
    /// <summary>
    /// Sets the RSS category with its child elements expanded.
    /// </summary>
    ///
    /// <param name="src">RSS category.</param>
    ///
    /* --------------------------------------------------------------------- */
    public static void Expand(this IRssEntry src)
    {
        var current = src;
        while (current is RssCategory category)
        {
            category.Expanded = true;
            current = category.Parent;
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Flatten
    ///
    /// <summary>
    /// Converts a list of RSS entries in a tree structure into a
    /// one-dimensional array.
    /// </summary>
    ///
    /// <param name="src">List of RSS entries for tree structure.</param>
    ///
    /// <returns>Conversion result.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<IRssEntry> Flatten(this IEnumerable<IRssEntry> src) =>
        src.Flatten(e => (e is RssCategory c) ? c.Children : null);

    /* --------------------------------------------------------------------- */
    ///
    /// Flatten
    ///
    /// <summary>
    /// Converts a list of RSS entries in a tree structure into a
    /// one-dimensional array.
    /// </summary>
    ///
    /// <param name="src">RSS entry for tree structure.</param>
    ///
    /// <returns>Conversion result.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<IRssEntry> Flatten(this IRssEntry src) =>
        new[] { src }.Flatten();

    /* --------------------------------------------------------------------- */
    ///
    /// Flatten
    ///
    /// <summary>
    /// Converts a list of RSS entries in a tree structure into a
    /// one-dimensional array.
    /// </summary>
    ///
    /// <param name="src">List of RSS entries for tree structure.</param>
    ///
    /// <returns>Conversion result.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IRssEntry> src) =>
        src.Flatten().OfType<T>();

    /* --------------------------------------------------------------------- */
    ///
    /// Flatten
    ///
    /// <summary>
    /// Converts a list of RSS entries in a tree structure into a
    /// one-dimensional array.
    /// </summary>
    ///
    /// <param name="src">RSS entry for tree structure.</param>
    ///
    /// <returns>Conversion result.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<T> Flatten<T>(this IRssEntry src) =>
        src.Flatten().OfType<T>();

    /* --------------------------------------------------------------------- */
    ///
    /// IsHighFrequency
    ///
    /// <summary>
    /// Determines if the check interval corresponds to high frequency.
    /// </summary>
    ///
    /// <param name="src">RSS Entry.</param>
    /// <param name="now">Base date-time.</param>
    ///
    /// <returns>true for high frequency.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static bool IsHighFrequency(this RssEntry src, DateTime now) =>
        src.Frequency == RssCheckFrequency.High ||
        src.Frequency == RssCheckFrequency.Auto &&
        (!src.LastChecked.HasValue || now - src.LastPublished <= TimeSpan.FromDays(30));

    /* --------------------------------------------------------------------- */
    ///
    /// IsLowFrequency
    ///
    /// <summary>
    /// Determines if the check interval corresponds to low frequency.
    /// </summary>
    ///
    /// <param name="src">RSS Entry.</param>
    /// <param name="now">Base date-time.</param>
    ///
    /// <returns>true for low frequency.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static bool IsLowFrequency(this RssEntry src, DateTime now) =>
        src.Frequency != RssCheckFrequency.None && !IsHighFrequency(src, now);

    /* --------------------------------------------------------------------- */
    ///
    /// ToMessage
    ///
    /// <summary>
    /// Gets a message representing the result of the RSS feed.
    /// </summary>
    ///
    /// <param name="src">RSS feed.</param>
    /// <param name="count">Number of new articles.</param>
    ///
    /// <returns>Message.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static string ToMessage(this RssFeed src, int count) =>
        src.Error != null ? string.Format(Properties.Resources.ErrorFeed, src.Title) :
        count > 0         ? string.Format(Properties.Resources.MessageReceived, count, src.Title) :
                            string.Format(Properties.Resources.MessageNoReceived, src.Title);

    /* --------------------------------------------------------------------- */
    ///
    /// ToMessage
    ///
    /// <summary>
    /// Gets a message representing a Frequency object.
    /// </summary>
    ///
    /// <param name="src">Frequency object.</param>
    ///
    /// <returns>Message</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static string ToMessage(this RssCheckFrequency src) =>
        new Dictionary<RssCheckFrequency, string>
        {
            { RssCheckFrequency.Auto, Properties.Resources.MessageAutoFrequency },
            { RssCheckFrequency.High, Properties.Resources.MessageHighFrequency },
            { RssCheckFrequency.Low,  Properties.Resources.MessageLowFrequency  },
            { RssCheckFrequency.None, Properties.Resources.MessageNoneFrequency },
        }[src];

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// ReadCore
    ///
    /// <summary>
    /// Sets all articles in the specified category as read.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static void ReadCore(RssCategory src)
    {
        foreach (var item in src.Children) Read(item);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ReadCore
    ///
    /// <summary>
    /// Sets all articles in the specified RSS entry as read.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static void ReadCore(RssEntry src)
    {
        foreach (var item in src.UnreadItems) item.Status = RssItemStatus.Read;
        src.Count = 0;
    }

    #endregion
}
