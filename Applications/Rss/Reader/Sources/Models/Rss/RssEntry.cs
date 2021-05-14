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

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntry
    ///
    /// <summary>
    /// Represents the information about Web sites that subscribe to RSS
    /// feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssEntry : RssFeed, IRssEntry
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry
        ///
        /// <summary>
        /// Initializes a new instance of the RssEntry class with the
        /// specified dispatcher.
        /// </summary>
        ///
        /// <param name="dispatcher">Dispatcher object.</param>
        ///
        /// <remarks>
        /// 将来的に Items に対して ObservableCollection(T) を指定する
        /// 必要がある。その場合は base を BindableCollection(T) で
        /// 初期化する。現在は SafeItems を利用しているため必要として
        /// いない模様（SafeItems の項目も参照）。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry(Dispatcher dispatcher) { Dispatcher = dispatcher; }

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry
        ///
        /// <summary>
        /// Initializes a new instance of the RssEntry class with the
        /// specified arguments.
        /// </summary>
        ///
        /// <param name="cp">Source to be copied.</param>
        /// <param name="dispatcher">Dispatcher object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssEntry(RssFeed cp, Dispatcher dispatcher) : this(dispatcher)
        {
            Title         = cp.Title;
            Uri           = cp.Uri;
            Link          = cp.Link;
            Count         = cp.UnreadItems.Count();
            Description   = cp.Description;
            LastChecked   = cp.LastChecked;
            LastPublished = cp.LastPublished;

            foreach (var i in cp.Items.GetOrEmpty()) Items.Add(i);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Parent
        ///
        /// <summary>
        /// Gets or sets the parent entry.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IRssEntry Parent
        {
            get => Get<IRssEntry>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Frequency
        ///
        /// <summary>
        /// Get or set the frequency of RSS feed confirmation.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssCheckFrequency Frequency
        {
            get => Get(() => RssCheckFrequency.Auto);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        ///
        /// <summary>
        /// Gets or sets the number of unread articles.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count
        {
            get => Get<int>();
            set { if (Set(value) && Parent is RssCategory rc) rc.Refresh(nameof(Count)); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Selected
        ///
        /// <summary>
        /// Gets or sets a value indicating whether the entry is selected.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Selected
        {
            get => Get(() => false);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Expanded
        ///
        /// <summary>
        /// Gets a value indicating whether the child element is visible.
        /// </summary>
        ///
        /// <remarks>
        /// The property always returns false.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public bool Expanded
        {
            get => false;
            set { }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Editing
        ///
        /// <summary>
        /// Gets or sets a value indicating whether the entry is being
        /// edited by the user or not.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Editing
        {
            get => Get(() => false);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SkipContent
        ///
        /// <summary>
        /// Gets or sets a value indicating whether to skip the display
        /// of Content.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool SkipContent
        {
            get => Get(() => false);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SafeItems
        ///
        /// <summary>
        /// Get the list of new articles.
        /// </summary>
        ///
        /// <remarks>
        /// Items を BindableCollection(T) で初期化し、直接 Binding すると
        /// ListView の ItemsControl に対して InvalidOperationException が
        /// 発生する場合がある。該当の現象が解消でき次第、このプロパティは
        /// 削除する。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<RssItem> SafeItems => Items.Where(e => true);

        #endregion

        #region Json

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntry.Json
        ///
        /// <summary>
        /// JSON 解析用クラスです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataContract]
        internal class Json
        {
            [DataMember] public string Title { get; set; }
            [DataMember] public Uri Uri { get; set; }
            [DataMember] public Uri Link { get; set; }
            [DataMember] public int Count { get; set; }
            [DataMember] public RssCheckFrequency Frequency { get; set; }
            [DataMember] public bool SkipContent { get; set; }

            public Json(RssEntry src)
            {
                Title       = src.Title;
                Uri         = src.Uri;
                Link        = src.Link;
                Count       = src.Count;
                Frequency   = src.Frequency;
                SkipContent = src.SkipContent;
            }

            public RssEntry Convert(RssCategory src)
            {
                var dest = src.CreateEntry();
                dest.Title       = Title;
                dest.Uri         = Uri;
                dest.Link        = Link;
                dest.Count       = Count;
                dest.Frequency   = Frequency;
                dest.SkipContent = SkipContent;
                return dest;
            }
        }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssCheckFrequency
    ///
    /// <summary>
    /// Specifies the frequency of checking RSS feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum RssCheckFrequency
    {
        /// <summary>Auto.</summary>
        Auto = 0,
        /// <summary>High frequency.</summary>
        High = 1,
        /// <summary>Low frequency.</summary>
        Low = 2,
        /// <summary>Not checked.</summary>
        None = -1,
    }
}
