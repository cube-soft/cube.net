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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cube.Conversions;
using Cube.FileSystem;
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
        /// 
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<IRssEntry> Load(string path) =>
            Load(path, new Operator());

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// OPML ファイルを読み込みます。
        /// </summary>
        /// 
        /// <param name="path">ファイルのパス</param>
        /// <param name="io">入出力用のオブジェクト</param>
        /// 
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<IRssEntry> Load(string path, Operator io)
        {
            using (var ss = io.OpenRead(path))
            {
                var body = XDocument.Load(ss).Root.GetElement("body");
                return Convert(body, null);
            }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// XElement オブジェクトを解析し、IRssEntry オブジェクトに
        /// 変換します。
        /// </summary>
        /// 
        /// <param name="src">XElement オブジェクト</param>
        /// <param name="parent">親要素</param>
        /// 
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<IRssEntry> Convert(XElement src, IRssEntry parent) =>
            src.GetElements("outline").Select(
                e => e.Attribute("xmlUrl") != null ?
                ConvertEntry(e, parent) :
                ConvertCategory(e, parent)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertEntry
        /// 
        /// <summary>
        /// XElement オブジェクトを解析し、RssEntry オブジェクトに
        /// 変換します。
        /// </summary>
        /// 
        /// <param name="src">XElement オブジェクト</param>
        /// <param name="parent">親要素</param>
        /// 
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IRssEntry ConvertEntry(XElement src, IRssEntry parent) => new RssEntry
        {
            Parent = parent,
            Title  = (string)src.Attribute("title") ?? string.Empty,
            Uri    = src.Attribute("xmlUrl")?.Value.ToUri(),
            Link   = src.Attribute("htmlUrl")?.Value.ToUri(),
        } as IRssEntry;

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertCategory
        /// 
        /// <summary>
        /// XElement オブジェクトを解析し、RssCategory オブジェクトに
        /// 変換します。
        /// </summary>
        /// 
        /// <param name="src">XElement オブジェクト</param>
        /// <param name="parent">親要素</param>
        /// 
        /// <returns>変換オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        private static IRssEntry ConvertCategory(XElement src, IRssEntry parent)
        {
            var dest = new RssCategory
            {
                Parent = parent,
                Title  = (string)src.Attribute("title") ?? string.Empty,
            };

            dest.Children = Convert(src, dest).ToBindable();
            return dest;
        }

        #endregion
    }
}
