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
    /// RssParser
    ///
    /// <summary>
    /// RSS および Atom 情報を解析するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class RssParser
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        /// 
        /// <summary>
        /// ストリームからデータを読み込み、RssFeed オブジェクトを
        /// 生成します。
        /// </summary>
        /// 
        /// <param name="src">ストリーム</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(System.IO.Stream src)
            => Parse(XDocument.Load(src).Root);

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        /// 
        /// <summary>
        /// XML オブジェクトから RssFeed オブジェクトを生成します。
        /// </summary>
        /// 
        /// <param name="src">XML</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement src)
            => Parse(src, src.GetRssVersion());

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        /// 
        /// <summary>
        /// XML オブジェクトから RssFeed オブジェクトを生成します。
        /// </summary>
        /// 
        /// <param name="src">XML</param>
        /// <param name="version">RSS バージョン</param>
        /// 
        /// <returns>RssFeed オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Parse(XElement src, RssVersion version)
        {
            switch (version)
            {
                case RssVersion.Atom:
                    break;
                case RssVersion.Rss091:
                    break;
                case RssVersion.Rss092:
                    break;
                case RssVersion.Rss10:
                    break;
                case RssVersion.Rss20: return Rss200Parser.Parse(src);
            }
            return default(RssFeed);
        }

        #endregion
    }
}
