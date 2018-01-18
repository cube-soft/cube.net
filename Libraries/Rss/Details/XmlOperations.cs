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
using System.Text.RegularExpressions;
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
        #region Methods

        #region GetElement

        /* ----------------------------------------------------------------- */
        ///
        /// GetElement
        /// 
        /// <summary>
        /// XElement オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="name">要素名</param>
        /// 
        /// <returns>XElement オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static XElement GetElement(this XElement e, string name)
            => GetElement(e, string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetElement
        /// 
        /// <summary>
        /// XElement オブジェクトを取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        /// 
        /// <returns>XElement オブジェクト</returns>
        /// 
        /// <remarks>
        /// 名前空間が空文字の場合、既定の名前空間を使用します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static XElement GetElement(this XElement e, string ns, string name)
            => !string.IsNullOrEmpty(ns) ?
               e.Element(XNamespace.Get(ns) + name) :
               e.Element(e.GetDefaultNamespace() + name);

        #endregion

        #region GetValue

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
        public static string GetValue(this XElement e, string name)
            => GetValue(e, string.Empty, name);

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
            => GetElement(e, ns, name)?.Value ?? string.Empty;

        #endregion

        #region GetUri

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
        public static Uri GetUri(this XElement e, string name)
            => GetUri(e, string.Empty, name);

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
        public static Uri GetUri(this XElement e, string ns, string name) => TryFunc(() =>
        {
            var node = e.GetElement(ns, name);
            if (node == null) return default(Uri);
            if (!string.IsNullOrEmpty(node.Value)) return new Uri(node.Value);

            var attr = node.Attribute("href")?.Value;
            return !string.IsNullOrEmpty(attr) ? new Uri(attr) : default(Uri);
        });

        #endregion

        #region GetDateTime

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
        public static DateTime GetDateTime(this XElement e, string name)
            => GetDateTime(e, string.Empty, name);

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
        public static DateTime GetDateTime(this XElement e, string ns, string name)
            => TryFunc(() =>
        {
            var value = e.GetValue(ns, name);
            return !string.IsNullOrEmpty(value) ? DateTime.Parse(value) : default(DateTime);
        });

        #endregion

        #region GetTitle

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
            => GetTitle(e, string.Empty);

        /* ----------------------------------------------------------------- */
        ///
        /// GetTitle
        /// 
        /// <summary>
        /// タイトルを取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// 
        /// <returns>タイトル</returns>
        /// 
        /// <remarks>
        /// title タグが存在しない場合、link タグの内容で代替します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetTitle(this XElement e, string ns)
        {
            var title = e.GetValue(ns, "title");
            if (!string.IsNullOrEmpty(title)) return title.Trim();

            var link = e.GetUri(ns, "link");
            return link?.ToString()?.Trim() ?? string.Empty;
        }

        #endregion

        #region Strip

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
                       Regex.Replace(src, "<.*?>", string.Empty).Trim();
            return dest.Length <= n ?
                   dest :
                   dest.Substring(0, n);
        }

        #endregion

        #endregion

        #region Implementations

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
        private static T TryFunc<T>(Func<T> func)
        {
            try { return func(); }
            catch (Exception /* err */) { return default(T); }
        }

        #endregion
    }
}
