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
using System.Runtime.Serialization;

/* ------------------------------------------------------------------------- */
///
/// RssItem
///
/// <summary>
/// Represents the information of the RSS feed item.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class RssItem : ObservableBase
{
    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Title
    ///
    /// <summary>
    /// Gets or sets the article title.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Title
    {
        get => Get(() => string.Empty);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Summary
    ///
    /// <summary>
    /// Gets or sets the article summary.
    /// </summary>
    ///
    /// <remarks>
    /// The property corresponds to the description tag in RSS and the
    /// summary tag in Atom.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public string Summary
    {
        get => Get(() => string.Empty);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Content
    ///
    /// <summary>
    /// Gets or sets the article content.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Content
    {
        get => Get(() => string.Empty);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Link
    ///
    /// <summary>
    /// Gets or sets the URL of the article.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public Uri Link
    {
        get => Get<Uri>();
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// PublishTime
    ///
    /// <summary>
    /// Gets or sets the publish date of the article.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public DateTime? PublishTime
    {
        get => Get<DateTime?>();
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Status
    ///
    /// <summary>
    /// Gets or sets the status of the object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public RssItemStatus Status
    {
        get => Get(() => RssItemStatus.Uninitialized);
        set => Set(value);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Categories
    ///
    /// <summary>
    /// Get the list of categories to which the article belongs.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IList<string> Categories { get; } = new List<string>();

    #endregion

    #region  Methods

    /* --------------------------------------------------------------------- */
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
    /* --------------------------------------------------------------------- */
    protected override void Dispose(bool disposing) { }

    #endregion

    #region Json

    [DataContract]
    internal class Json
    {
        [DataMember] public string Title { get; set; }
        [DataMember] public string Summary { get; set; }
        [DataMember] public string Content { get; set; }
        [DataMember] public IList<string> Categories { get; set; }
        [DataMember] public Uri Link { get; set; }
        [DataMember] public DateTime? PublishTime { get; set; }
        [DataMember] public RssItemStatus Status { get; set; }

        public Json(RssItem src)
        {
            Title       = src.Title;
            Summary     = src.Summary;
            Content     = src.Content;
            Link        = src.Link;
            PublishTime = src.PublishTime;
            Status      = src.Status;
            Categories  = new List<string>();
            foreach (var c in src.Categories) Categories.Add(c);
        }

        public RssItem Convert()
        {
            var dest = new RssItem
            {
                Title       = Title,
                Summary     = Summary,
                Content     = Content,
                Link        = Link,
                PublishTime = PublishTime,
                Status      = Status,
            };

            if (Categories is not null)
            {
                foreach (var c in Categories) Categories.Add(c);
            }
            return dest;
        }
    }

    #endregion
}

/* ------------------------------------------------------------------------- */
///
/// RssItemStatus
///
/// <summary>
/// Specifies the status of the RssItem object.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public enum RssItemStatus
{
    /// <summary>Uninitialized.</summary>
    Uninitialized,
    /// <summary>Unread.</summary>
    Unread,
    /// <summary>Read.</summary>
    Read,
}
