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
using System.Xml.Linq;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssVersion
    ///
    /// <summary>
    /// RSS および Atom フィードの種類を表す列挙型です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public enum RssVersion
    {
        /// <summary>RSS 0.91</summary>
        Rss091,
        /// <summary>RSS 0.92</summary>
        Rss092,
        /// <summary>RSS 1.0</summary>
        Rss10,
        /// <summary>RSS 2.0</summary>
        Rss20,
        /// <summary>Atom</summary>
        Atom,
        /// <summary>不明</summary>
        Unknown,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssVersionConversions
    ///
    /// <summary>
    /// RssVersion の拡張用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class RssVersionConversions
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetRssVersion
        ///
        /// <summary>
        /// XML から RssVersion オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="src">XML</param>
        /// 
        /// <returns>RssVersion</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssVersion GetRssVersion(this XElement src)
        {
            var rss = src.Attribute("version");
            if (rss != null)
            {
                switch (rss.Value)
                {
                    case "0.91": return RssVersion.Rss091;
                    case "0.92": return RssVersion.Rss092;
                    case "2.0" : return RssVersion.Rss20;
                }
            }

            var ns = src.GetDefaultNamespace();
            if (ns.NamespaceName.ToLower().Contains("purl.org")) return RssVersion.Rss10;
            if (ns.NamespaceName.ToLower().Contains("atom")) return RssVersion.Atom;
            return RssVersion.Unknown;
        }
    }
}
