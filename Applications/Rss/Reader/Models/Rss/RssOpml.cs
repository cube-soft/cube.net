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
using System.Xml.Linq;
using Cube.Conversions;
using Cube.FileSystem;
using Cube.FileSystem.Files;
using Cube.Net.Rss;
using Cube.Xui;
using Cube.Xml;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssOpml
    ///
    /// <summary>
    /// OPML 形式のデータを相互変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssOpml
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// OPML ファイルを読み込みます。
        /// </summary>
        ///
        /// <param name="path">ファイルのパス</param>
        /// <param name="filter">フィルタリング用オブジェクト</param>
        /// <param name="io">入出力用のオブジェクト</param>
        ///
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<IRssEntry> Load(string path,
            IDictionary<Uri, RssFeed> filter, Operator io) => io.Load(
            path,
            e =>
            {
                var body = XDocument.Load(e).Root.GetElement("body");
                return Convert(body, null, filter);
            },
            new IRssEntry[0]
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// OPML ファイルに保存します。
        /// </summary>
        ///
        /// <param name="src">保存元データ</param>
        /// <param name="path">ファイルのパス</param>
        /// <param name="io">入出力用のオブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Save(IEnumerable<IRssEntry> src, string path, Operator io) => io.Save(
            path,
            e =>
            {
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "true"),
                    new XElement("opml",
                        new XAttribute("version", "1.0"),
                        new XElement("head",
                            new XElement("title", "CubeRSS Reader subscriptions"),
                            new XElement("dateCreated", DateTime.Now.ToUniversalTime().ToString("r"))
                        ),
                        CreateBody(src)
                    )
                );
                doc.Save(e);
            }
        );

        #endregion

        #region Implementations

        #region Load

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// XElement オブジェクトを解析し、IRssEntry オブジェクトに
        /// 変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<IRssEntry> Convert(XElement src,
            IRssEntry parent, IDictionary<Uri, RssFeed> filter) =>
            src.GetElements("outline")
               .Select(e => IsEntry(e) ? ToEntry(e, parent) : ToCategory(e, parent, filter))
               .Where(e => e is RssCategory rc && rc.Children.Count > 0 ||
                           e is RssEntry re && !filter.ContainsKey(re.Uri));

        /* ----------------------------------------------------------------- */
        ///
        /// ToEntry
        ///
        /// <summary>
        /// XElement オブジェクトを解析し、RssEntry オブジェクトに
        /// 変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IRssEntry ToEntry(XElement src, IRssEntry parent) => new RssEntry
        {
            Parent = parent,
            Title  = (string)src.Attribute("title"),
            Uri    = src.Attribute("xmlUrl").Value.ToUri(),
            Link   = src.Attribute("htmlUrl").Value.ToUri(),
        } as IRssEntry;

        /* ----------------------------------------------------------------- */
        ///
        /// ToCategory
        ///
        /// <summary>
        /// XElement オブジェクトを解析し、RssCategory オブジェクトに
        /// 変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IRssEntry ToCategory(XElement src, IRssEntry parent,
            IDictionary<Uri, RssFeed> filter)
        {
            var dest = new RssCategory
            {
                Parent = parent,
                Title  = (string)src.Attribute("title"),
            };

            dest.Children = Convert(src, dest, filter).ToBindable();
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsEntry
        ///
        /// <summary>
        /// RssEntry を表す XElement かどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static bool IsEntry(XElement src) => src.Attribute("xmlUrl") != null;

        #endregion

        #region Save

        /* ----------------------------------------------------------------- */
        ///
        /// CreateBodyElement
        ///
        /// <summary>
        /// body を表す XElement オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static XElement CreateBody(IEnumerable<IRssEntry> src)
        {
            var dest = new XElement("body");
            foreach (var item in ConvertBack(src)) dest.Add(item);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        ///
        /// <summary>
        /// IRssEntry オブジェクトから XElement オブジェクトに変換します。
        /// </summary>
        ///
        /// <param name="src">IRssEntry オブジェクト一覧</param>
        ///
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<XElement> ConvertBack(IEnumerable<IRssEntry> src) =>
            src.Select(e =>
                e is RssCategory rc ?
                FromCategory(rc) :
                FromEntry(e as RssEntry)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// FromEntry
        ///
        /// <summary>
        /// RssEntry オブジェクトから XElement オブジェクトに変換します。
        /// </summary>
        ///
        /// <param name="src">RssEntry オブジェクト</param>
        ///
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static XElement FromEntry(RssEntry src) => new XElement(
            "outline",
            new XAttribute("type", "rss"),
            new XAttribute("title", src.Title),
            new XAttribute("xmlUrl", src.Uri.ToString()),
            new XAttribute("htmlUrl", src.Link.ToString())
        );

        /* ----------------------------------------------------------------- */
        ///
        /// FromCategory
        ///
        /// <summary>
        /// RssCategory オブジェクトから XElement オブジェクトに変換します。
        /// </summary>
        ///
        /// <param name="src">RssCategory オブジェクト</param>
        ///
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static XElement FromCategory(RssCategory src)
        {
            var dest = new XElement("outline", new XAttribute("title", src.Title));
            foreach (var item in ConvertBack(src.Children)) dest.Add(item);
            return dest;
        }

        #endregion

        #endregion
    }
}
