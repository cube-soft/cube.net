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
using System.Linq;
using System.Runtime.Serialization;
using Cube.Mixin.Collections;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssFeed
    ///
    /// <summary>
    /// Represents the RSS feed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssFeed : ObservableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssFeed
        ///
        /// <summary>
        /// Initializes a new instance of the RssFeed class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed() : this(new List<RssItem>()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// RssFeed
        ///
        /// <summary>
        /// Initializes a new instance of the RssFeed class.
        /// </summary>
        ///
        /// <param name="items">Items buffer.</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssFeed(IList<RssItem> items) { Items = items; }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        ///
        /// <summary>
        /// Gets or sets the Web site title.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Title
        {
            get => Get(() => string.Empty);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Description
        ///
        /// <summary>
        /// Gets or sets the Web site description.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Description
        {
            get => Get(() => string.Empty);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Link
        ///
        /// <summary>
        /// Gets or sets the Web site URL.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Link
        {
            get => Get<Uri>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// Gets or sets the feed URL.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri
        {
            get => Get<Uri>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastChecked
        ///
        /// <summary>
        /// Gets or sets the date-time when the RSS feed was last checked.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastChecked
        {
            get => Get<DateTime?>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastPublished
        ///
        /// <summary>
        /// Gets or sets the date-time of the latest article update in the
        /// RSS feed.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastPublished
        {
            get => Get<DateTime?>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Items
        ///
        /// <summary>
        /// Gets or sets the list of new articles.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IList<RssItem> Items { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// UnreadItems
        ///
        /// <summary>
        /// Get the list of unread articles.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssItem> UnreadItems =>
            Items.Where(e => e.Status == RssItemStatus.Unread);

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        ///
        /// <summary>
        /// Gets or sets an exception object that occurs when handling.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Exception Error
        {
            get => Get<Exception>();
            set => Set(value);
        }

        #endregion

        #region  Methods

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
        protected override void Dispose(bool disposing) { }

        #endregion

        #region Json

        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public string Description { get; set; }
            [DataMember] public Uri Uri { get; set; }
            [DataMember] public Uri Link { get; set; }
            [DataMember] public DateTime? LastChecked { get; set; }
            [DataMember] public DateTime? LastPublished { get; set; }
            [DataMember] public IList<RssItem.Json> Items { get; set; }

            public Json(RssFeed src)
            {
                Title         = src.Title;
                Description   = src.Description;
                Uri           = src.Uri;
                Link          = src.Link;
                LastChecked   = src.LastChecked;
                LastPublished = src.LastPublished;
                Items         = new List<RssItem.Json>();
                foreach (var i in src.Items) Items.Add(new RssItem.Json(i));
            }

            public RssFeed Convert()
            {
                var dest = new RssFeed
                {
                    Title         = Title,
                    Description   = Description,
                    Uri           = Uri,
                    Link          = Link,
                    LastChecked   = LastChecked,
                    LastPublished = LastPublished,
                };

                foreach (var i in Items.GetOrEmpty()) dest.Items.Add(i.Convert());
                return dest;
            }
        }

        #endregion
    }
}
