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
using Cube.Xml;
using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Cube.Net.Rss.Parsing
{
    /* --------------------------------------------------------------------- */
    ///
    /// FeedExtension
    ///
    /// <summary>
    /// RSS/Atom 解析時に使用する拡張メソッドを定義するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class FeedExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        ///
        /// <summary>
        /// 値を取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>文字列</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string name) =>
            GetValue(e, string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        ///
        /// <summary>
        /// 値を取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>文字列</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string ns, string name) =>
            e.GetElement(ns, name)?.Value ?? string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// GetTitle
        ///
        /// <summary>
        /// タイトルを取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        ///
        /// <returns>タイトル</returns>
        ///
        /// <remarks>
        /// title タグが存在しない場合、link タグの内容で代替します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetTitle(this XElement e)
        {
            var title = e.GetValue("title");
            if (!string.IsNullOrEmpty(title)) return title.Trim();

            var uri = e.GetUri("link");
            return uri?.ToString() ?? string.Empty;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetDateTime
        ///
        /// <summary>
        /// DateTime オブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>DateTime オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime? GetDateTime(this XElement e, string name) =>
            e.GetDateTime(string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetDateTime
        ///
        /// <summary>
        /// DateTime オブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>DateTime オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime? GetDateTime(this XElement e, string ns, string name)
        {
            var value = e.GetValue(ns, name);
            return DateTime.TryParse(value, out DateTime dest) ? dest : default(DateTime?);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        ///
        /// <summary>
        /// Uri オブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>Uri オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement e, string name) =>
            e.GetUri(string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        ///
        /// <summary>
        /// Uri オブジェクトを取得します。
        /// </summary>
        ///
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        ///
        /// <returns>Uri オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement e, string ns, string name)
        {
            try
            {
                var value = e.GetValueOrAttribute(ns, name, "href");
                return !string.IsNullOrEmpty(value) ? new Uri(value.Trim()) : default(Uri);
            }
            catch { return default(Uri); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Strip
        ///
        /// <summary>
        /// HTML タグを除去する等して文字列を正規化します。
        /// </summary>
        ///
        /// <param name="src">オリジナルの文字列</param>
        /// <param name="n">最大文字長</param>
        ///
        /// <returns>正規化後の文字列</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string Strip(this string src, int n)
        {
            var dest = string.IsNullOrEmpty(src) ?
                       string.Empty :
                       Regex.Replace(src, @"<(""[^""]*""|'[^']*'|[^'"">])*>|\t|\n|\r", string.Empty).Trim();
            return dest.Length <= n ?
                   dest :
                   dest.Substring(0, n);
        }

        #endregion
    }


    /* --------------------------------------------------------------------- */
    ///
    /// RssExtension
    ///
    /// <summary>
    /// RSS 解析時に使用する拡張メソッドを定義するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class RssExtension
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetRssSummary
        ///
        /// <summary>
        /// Summary を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// description タグが存在せず、かつ content:encoded タグが存在
        /// する場合、content:encoded から html タグを除去した内容を
        /// 返します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetRssSummary(this XElement src)
        {
            var n    = RssParseOptions.MaxSummaryLength;
            var dest = src.GetValue("description").Strip(n);
            return !string.IsNullOrEmpty(dest) ?
                   dest :
                   src.GetValue("content", "encoded").Strip(n);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetRssContent
        ///
        /// <summary>
        /// Content を取得します。
        /// </summary>
        ///
        /// <remarks>
        /// RSS 1.0/2.0 ともに content:encoded タグを利用される事が多い
        /// ため、該当タグが存在する場合はその内容を返します。
        /// content:encoded タグが存在しない場合は description の内容を
        /// 返します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetRssContent(this XElement src)
        {
            var enc  = src.GetValue("content", "encoded");
            var dest = !string.IsNullOrEmpty(enc) ?
                       enc :
                       src.GetValue("description") ??
                       string.Empty;
            return dest.Trim();
        }
    }
}
