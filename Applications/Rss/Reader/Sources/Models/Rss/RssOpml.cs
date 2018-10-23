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
using Cube.Conversions;
using Cube.FileSystem;
using Cube.FileSystem.Mixin;
using Cube.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Cube.Net.Rss.App.Reader
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
    public class RssOpml
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssOpml
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="context">同期用コンテキスト</param>
        /// <param name="io">入出力用のオブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssOpml(SynchronizationContext context, IO io)
        {
            Context = context;
            IO      = io;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Context
        ///
        /// <summary>
        /// 同期用コンテキストを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SynchronizationContext Context { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        ///
        /// <summary>
        /// 入出力用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IO IO { get; }

        #endregion

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
        ///
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<IRssEntry> Load(string path,
            IDictionary<Uri, RssFeed> filter) => IO.LoadOrDefault(
            path,
            e =>
            {
                try {
                    var body = XDocument.Load(e).Root.GetElement("body");
                    return Convert(body, null, filter);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err.ToString());
                    return new IRssEntry[0];
                }
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
        ///
        /* ----------------------------------------------------------------- */
        public void Save(IEnumerable<IRssEntry> src, string path) => IO.Save(
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
        private IEnumerable<IRssEntry> Convert(XElement src,
            IRssEntry parent, IDictionary<Uri, RssFeed> filter) =>
            src.GetElements("outline")
               .Select(e => IsEntry(e) ? ToEntry(e, parent) : ToCategory(e, parent, filter))
               .OfType<IRssEntry>()
               .Where(e => e is RssCategory rc && rc.Children.Count > 0 ||
                           e is RssEntry re && re.Uri != null && !filter.ContainsKey(re.Uri));

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
        private IRssEntry ToEntry(XElement src, IRssEntry parent)
        {
            var uri = GetUri(src, "xmlUrl", null);
            return uri == null ?
                   default(IRssEntry) :
                   new RssEntry(Context)
                   {
                       Parent = parent,
                       Uri    = uri,
                       Link   = GetUri(src, "htmlUrl", uri),
                       Title  = GetTitle(src, uri.ToString()),
                   };
        }

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
        private IRssEntry ToCategory(XElement src, IRssEntry parent, IDictionary<Uri, RssFeed> filter)
        {
            var dest = new RssCategory(Context)
            {
                Parent = parent,
                Title  = GetTitle(src, Properties.Resources.MessageNewCategory),
            };

            foreach (var item in Convert(src, dest, filter)) dest.Children.Add(item);
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
        private bool IsEntry(XElement src) => src.Attribute("xmlUrl") != null;

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        ///
        /// <summary>
        /// Uri オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Uri GetUri(XElement src, string name, Uri alternate)
        {
            try { return src.Attribute(name).Value.ToUri(); }
            catch { return alternate; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetTitle
        ///
        /// <summary>
        /// タイトルを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetTitle(XElement src, string alternate) =>
            (string)src.Attribute("text")  ??
            (string)src.Attribute("title") ?? alternate;

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
