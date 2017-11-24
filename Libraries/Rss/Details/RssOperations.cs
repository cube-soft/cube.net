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
    /// RssOperations
    ///
    /// <summary>
    /// RSS 解析時に使用する拡張メソッドを定義するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    internal static class RssOperations
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetContent
        /// 
        /// <summary>
        /// Content を取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// 
        /// <returns>文字列</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public static string GetContent(this XElement e)
        {
            var ns = "http://purl.org/rss/1.0/modules/content/";
            var encoded = e.GetValue(ns, "encoded");

            return !string.IsNullOrEmpty(encoded) ?
                   encoded :
                   e.GetValue("content");
        }

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
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string name)
            => e.Element(name)?.Value ?? string.Empty;

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
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string ns, string name)
        {
            XNamespace cvt = ns;
            return e.Element(cvt + name)?.Value ?? string.Empty;
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
        public static Uri GetUri(this XElement e, string name) => TryFunc(() =>
        {
            var node = e.Element(name);
            if (node == null) return default(Uri);
            if (!string.IsNullOrEmpty(node.Value)) return new Uri(node.Value);

            var attr = node.Attribute("href")?.Value;
            return !string.IsNullOrEmpty(attr) ? new Uri(attr) : default(Uri);
        });

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
        public static DateTime GetDateTime(this XElement e, string name) => TryFunc(() =>
        {
            var value = e.GetValue(name);
            return !string.IsNullOrEmpty(value) ? DateTime.Parse(value) : default(DateTime);
        });

        /* ----------------------------------------------------------------- */
        ///
        /// TryFunc(T)
        /// 
        /// <summary>
        /// Func(T) を実行します。
        /// </summary>
        /// 
        /// <param name="func">実行する関数オブジェクト</param>
        /// 
        /// <returns>実行結果</returns>
        /// 
        /// <remarks>
        /// 例外が発生した場合 default(T) を返します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static T TryFunc<T>(Func<T> func)
        {
            try { return func(); }
            catch (Exception /* err */) { return default(T); }
        }
    }
}
