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
using System.Linq;
using System.Xml.Linq;
using Cube.Web.Extensions;

/* ------------------------------------------------------------------------- */
///
/// RssParser
///
/// <summary>
/// Provides functionality to parse the RSS and Atom feeds.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public static class RssParser
{
    #region Methods

    #region Parse

    /* --------------------------------------------------------------------- */
    ///
    /// Parse
    ///
    /// <summary>
    /// Reads data from the stream and creates a new instance of the
    /// RssFeed class.
    /// </summary>
    ///
    /// <param name="src">Source stream.</param>
    ///
    /// <returns>RssFeed object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static RssFeed Parse(System.IO.Stream src) =>
        Parse(XDocument.Load(src).Root);

    /* --------------------------------------------------------------------- */
    ///
    /// Parse
    ///
    /// <summary>
    /// Parses the specified XML object and creates a new instance of
    /// the RssFeed class.
    /// </summary>
    ///
    /// <param name="root">Root XML element.</param>
    ///
    /// <returns>RssFeed object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static RssFeed Parse(XElement root) =>
        Parse(root, root.GetRssVersion());

    /* --------------------------------------------------------------------- */
    ///
    /// Parse
    ///
    /// <summary>
    /// Parses the specified XML object and creates a new instance of
    /// the RssFeed class.
    /// </summary>
    ///
    /// <param name="root">Root XML element.</param>
    /// <param name="version">RSS version.</param>
    ///
    /// <returns>RssFeed object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static RssFeed Parse(XElement root, RssVersion version) => version switch
    {
        RssVersion.Atom   => AtomParser.Parse(root),
        RssVersion.Rss091 => Rss200Parser.Parse(root),
        RssVersion.Rss092 => Rss200Parser.Parse(root),
        RssVersion.Rss100 => Rss100Parser.Parse(root),
        RssVersion.Rss200 => Rss200Parser.Parse(root),
        _ => default,
    };

    #endregion

    #region GetRssUris

    /* --------------------------------------------------------------------- */
    ///
    /// GetRssUris
    ///
    /// <summary>
    /// Gets the sequence of the RSS feed URLs.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static IEnumerable<Uri> GetRssUris(this System.IO.Stream src)
    {
        var doc = ToXDocument(src);
        var ns = doc.Root.GetDefaultNamespace();

        return doc.Descendants(ns + "link")
                  .Where(e => IsRssLink(e))
                  .OrderBy(e => (string)e.Attribute("type"))
                  .Select(e => ((string)e.Attribute("href")).ToUri());
    }

    #endregion

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// IsRssLink
    ///
    /// <summary>
    /// Determines if the specified element is an RSS link.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static bool IsRssLink(XElement e)
    {
        if ((string)e.Attribute("rel") != "alternate") return false;
        var dest = ((string)e.Attribute("type") ?? "").ToLowerInvariant();
        return dest.Contains("rss") || dest.Contains("atom");
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ToXDocument
    ///
    /// <summary>
    /// Creates an XDocument object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static XDocument ToXDocument(System.IO.Stream src)
    {
        using var stream = new System.IO.StreamReader(src, System.Text.Encoding.UTF8);
        using var reader = new Sgml.SgmlReader
        {
            CaseFolding = Sgml.CaseFolding.ToLower,
            DocType = "HTML",
            IgnoreDtd = true,
            InputStream = stream,
        };
        return XDocument.Load(reader);
    }

    #endregion
}
