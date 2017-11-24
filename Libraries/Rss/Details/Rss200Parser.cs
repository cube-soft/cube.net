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
using System.Xml.Linq;
using Cube.Net.Rss.Parsing;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// Rss200Parser
    ///
    /// <summary>
    /// RSS 2.0 を解析するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    internal static class Rss200Parser
    {
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
        public static RssFeed Parse(XElement src) => new RssFeed
        {
            Title       = src.Element("title").GetValue(),
            Description = src.Element("description").GetValue(),
            Link        = src.Element("link").GetUri(),
            Items       = null,
            LastChecked = DateTime.Now,
        };
    }
}
