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

namespace Cube.Net.Rss.Parsing
{
    /* --------------------------------------------------------------------- */
    ///
    /// Parsing.Conversions
    ///
    /// <summary>
    /// RSS 解析時に使用する拡張メソッドを定義するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    internal static class Operations
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        /// 
        /// <summary>
        /// 値を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement src)
            => src != null ? src.Value : string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        /// 
        /// <summary>
        /// Uri オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement src)
            => src != null ? new Uri(src.Value) : null;
    }
}
