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
using System.Xml.Linq;
using Cube.Mixin.Xml;
using Cube.Web.Extensions;

/* ------------------------------------------------------------------------- */
///
/// RssOpml
///
/// <summary>
/// Provides functionality to interconvert data in OPML format.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class RssOpml
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// RssOpml
    ///
    /// <summary>
    /// Initializes a new instance of the RssOpml class.
    /// </summary>
    ///
    /// <param name="dispatcher">Context dispatcher.</param>
    ///
    /* --------------------------------------------------------------------- */
    public RssOpml(Dispatcher dispatcher) => Dispatcher = dispatcher;

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Dispatcher
    ///
    /// <summary>
    /// Gets the context dispatcher.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public Dispatcher Dispatcher { get; }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Load
    ///
    /// <summary>
    /// Reads the specified OPML format file.
    /// </summary>
    ///
    /// <param name="path">Path of the OPML format file.</param>
    /// <param name="filter">Filtering rules.</param>
    ///
    /// <returns>Loading result.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public IEnumerable<IRssEntry> Load(string path, IDictionary<Uri, RssFeed> filter)
    {
        try
        {
            var body = XDocument.Load(path).Root.GetElement("body");
            return Convert(body, null, filter);
        }
        catch (Exception err)
        {
            Logger.Warn(err);
            return Enumerable.Empty<IRssEntry>();
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Save
    ///
    /// <summary>
    /// Saves the specified RSS entries to a file in OPML format.
    /// </summary>
    ///
    /// <param name="src">Source RSS entries.</param>
    /// <param name="dest">Path of the destination file.</param>
    ///
    /* --------------------------------------------------------------------- */
    public void Save(IEnumerable<IRssEntry> src, string dest) => new XDocument(
        new XDeclaration("1.0", "utf-8", "true"),
        new XElement("opml",
            new XAttribute("version", "1.0"),
            new XElement("head",
                new XElement("title", "CubeRSS Reader subscriptions"),
                new XElement("dateCreated", DateTime.Now.ToUniversalTime().ToString("r"))
            ),
            CreateBody(src)
        )
    ).Save(dest);

    #endregion

    #region Implementations

    #region Load

    /* --------------------------------------------------------------------- */
    ///
    /// Convert
    ///
    /// <summary>
    /// Parses the specified XElement object and converts it into IRssEntry
    /// objects.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private IEnumerable<IRssEntry> Convert(XElement src,
        IRssEntry parent, IDictionary<Uri, RssFeed> filter) =>
        src.GetElements("outline")
           .Select(e => IsEntry(e) ? ToEntry(e, parent) : ToCategory(e, parent, filter))
           .OfType<IRssEntry>()
           .Where(e => e is RssCategory rc && rc.Children.Count > 0 ||
                       e is RssEntry re && re.Uri != null && !filter.ContainsKey(re.Uri));

    /* --------------------------------------------------------------------- */
    ///
    /// ToEntry
    ///
    /// <summary>
    /// Parses the specified XElement object and converts it into IRssEntry
    /// objects.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private IRssEntry ToEntry(XElement src, IRssEntry parent)
    {
        var uri = GetUri(src, "xmlUrl", null);
        return uri == null ?
               default(IRssEntry) :
               new RssEntry(Dispatcher)
               {
                   Parent = parent,
                   Uri    = uri,
                   Link   = GetUri(src, "htmlUrl", uri),
                   Title  = GetTitle(src, uri.ToString()),
               };
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ToCategory
    ///
    /// <summary>
    /// Parses the specified XElement object and converts it into RssCategory
    /// objects.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private IRssEntry ToCategory(XElement src, IRssEntry parent, IDictionary<Uri, RssFeed> filter)
    {
        var dest = new RssCategory(Dispatcher)
        {
            Parent = parent,
            Title  = GetTitle(src, Properties.Resources.MessageNewCategory),
        };

        foreach (var item in Convert(src, dest, filter)) dest.Children.Add(item);
        return dest;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// IsEntry
    ///
    /// <summary>
    /// Determines whether or not the specified XElement represents an
    /// RssEntry.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private bool IsEntry(XElement src) => src.Attribute("xmlUrl") != null;

    /* --------------------------------------------------------------------- */
    ///
    /// GetUri
    ///
    /// <summary>
    /// Gets the Uri object from the specified XElement object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private Uri GetUri(XElement src, string name, Uri alternate)
    {
        try { return src.Attribute(name).Value.ToUri(); }
        catch { return alternate; }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GetTitle
    ///
    /// <summary>
    /// Gets the title of the specified XElement object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private string GetTitle(XElement src, string alternate) =>
        (string)src.Attribute("text")  ??
        (string)src.Attribute("title") ?? alternate;

    #endregion

    #region Save

    /* --------------------------------------------------------------------- */
    ///
    /// CreateBodyElement
    ///
    /// <summary>
    /// Generates an XElement object representing a body.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static XElement CreateBody(IEnumerable<IRssEntry> src)
    {
        var dest = new XElement("body");
        foreach (var item in ConvertBack(src)) dest.Add(item);
        return dest;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ConvertBack
    ///
    /// <summary>
    /// Converts the specified IRssEntry objects to XElement objects.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static IEnumerable<XElement> ConvertBack(IEnumerable<IRssEntry> src) =>
        src.Select(e =>
            e is RssCategory rc ?
            FromCategory(rc) :
            FromEntry(e as RssEntry)
        );

    /* --------------------------------------------------------------------- */
    ///
    /// FromEntry
    ///
    /// <summary>
    /// Converts the specified RssEntry object to an XElement object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static XElement FromEntry(RssEntry src) => new(
        "outline",
        new XAttribute("type", "rss"),
        new XAttribute("title", src.Title),
        new XAttribute("xmlUrl", src.Uri.ToString()),
        new XAttribute("htmlUrl", src.Link.ToString())
    );

    /* --------------------------------------------------------------------- */
    ///
    /// FromCategory
    ///
    /// <summary>
    /// Converts the specified RssCategory object to an XElement object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private static XElement FromCategory(RssCategory src)
    {
        var dest = new XElement("outline", new XAttribute("title", src.Title));
        foreach (var item in ConvertBack(src.Children)) dest.Add(item);
        return dest;
    }

    #endregion

    #endregion
}
