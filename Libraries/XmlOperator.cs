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

namespace Cube.Xml
{
    /* --------------------------------------------------------------------- */
    ///
    /// XmlOperator
    ///
    /// <summary>
    /// XElement の拡張用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class XmlOperator
    {
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
        public static XElement GetElement(this XElement e, string name) =>
            GetElement(e, string.Empty, name);

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
        public static XElement GetElement(this XElement e, string ns, string name) =>
            !string.IsNullOrEmpty(ns) ?
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
            GetElement(e, ns, name)?.Value ?? string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// GetValue
        /// 
        /// <summary>
        /// 値を取得します。Value が存在しない場合はヒントに指定された名前に
        /// 対応する属性値を返します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        /// <param name="hint">ヒントとなる要素名</param>
        /// 
        /// <returns>文字列</returns>
        ///
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValue(this XElement e, string ns, string name, string hint)
        {
            var node = e.GetElement(ns, name);
            if (node == null) return string.Empty;
            if (!string.IsNullOrEmpty(node.Value)) return node.Value;
            if (string.IsNullOrEmpty(hint)) return string.Empty;
            return node.Attribute(hint)?.Value ?? string.Empty;
        }

        #endregion

        #region GetAttribute

        /* ----------------------------------------------------------------- */
        ///
        /// GetAttribute
        /// 
        /// <summary>
        /// 属性値を取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="name">要素名</param>
        /// <param name="attr">属性名</param>
        /// 
        /// <returns>文字列</returns>
        /// 
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetAttribute(this XElement e, string name, string attr) =>
            GetAttribute(e, string.Empty, name, attr);

        /* ----------------------------------------------------------------- */
        ///
        /// GetAttribute
        /// 
        /// <summary>
        /// 属性値を取得します。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        /// <param name="attr">属性名</param>
        /// 
        /// <returns>文字列</returns>
        /// 
        /// <remarks>
        /// null を string.Empty に正規化します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetAttribute(this XElement e, string ns, string name, string attr) =>
            e.GetElement(ns, name)?.Attribute(attr)?.Value ?? string.Empty;

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
        public static DateTime? GetDateTime(this XElement e, string name) =>
            GetDateTime(e, string.Empty, name);

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
        public static DateTime? GetDateTime(this XElement e, string ns, string name) =>
            e.GetDateTime(ns, name, string.Empty);

        /* ----------------------------------------------------------------- */
        ///
        /// GetDateTime
        /// 
        /// <summary>
        /// DateTime オブジェクトを取得します。Value が存在しない場合は
        /// ヒントに指定された名前を基に属性値を取得し Uri オブジェクトへの
        /// 変換を試みます。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        /// <param name="hint">ヒントとなる要素名</param>
        /// 
        /// <returns>DateTime オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DateTime? GetDateTime(this XElement e, string ns, string name, string hint)
        {
            var value = e.GetValue(ns, name, hint);
            return DateTime.TryParse(value, out DateTime dest) ? dest : (DateTime?)null;
        }

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
        public static Uri GetUri(this XElement e, string ns, string name) =>
            e.GetUri(ns, name, "href");

        /* ----------------------------------------------------------------- */
        ///
        /// GetUri
        /// 
        /// <summary>
        /// Uri オブジェクトを取得します。Value が存在しない場合はヒントに
        /// 指定された名前を基に属性値を取得し Uri オブジェクトへの変換を
        /// 試みます。
        /// </summary>
        /// 
        /// <param name="e">XML オブジェクト</param>
        /// <param name="ns">名前空間</param>
        /// <param name="name">要素名</param>
        /// <param name="hint">ヒントとなる要素名</param>
        /// 
        /// <returns>Uri オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri GetUri(this XElement e, string ns, string name, string hint)
        {
            try
            {
                var value = e.GetValue(ns, name, hint);
                return !string.IsNullOrEmpty(value) ? new Uri(value) : default(Uri);
            }
            catch (Exception /* err */) { return default(Uri); }
        }

        #endregion
    }
}
