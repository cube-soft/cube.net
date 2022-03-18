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
using Cube.Mixin.String;

namespace Cube.Mixin.Xml
{
    /* --------------------------------------------------------------------- */
    ///
    /// XmlExtension
    ///
    /// <summary>
    /// Provides extended methods of the XML related classes.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class XmlExtension
    {
        #region GetElement

        /* ----------------------------------------------------------------- */
        ///
        /// GetElement
        ///
        /// <summary>
        /// Gets the XElement object corresponding to the specified name
        /// in the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>XElement object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static XElement GetElement(this XElement e, string name) =>
            GetElement(e, string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetElement
        ///
        /// <summary>
        /// Gets the XElement object corresponding to the specified name
        /// and namespace. When the specified namespace is empty,
        /// the method uses the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="ns">Element namespace.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>XElement object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static XElement GetElement(this XElement e, string ns, string name) =>
            e.GetNamespace(ns) is XNamespace xns ? e.Element(xns + name) : default;

        #endregion

        #region GetElements

        /* ----------------------------------------------------------------- */
        ///
        /// GetElements
        ///
        /// <summary>
        /// Gets the collection of XElement objects corresponding to the
        /// specified name in the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>Collection of XElements objects.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<XElement> GetElements(this XElement e, string name) =>
            GetElements(e, string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetElements
        ///
        /// <summary>
        /// Gets the collection of XElement objects corresponding to the
        /// specified name and namespace. When the specified namespace is
        /// empty, the method uses the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="ns">Element namespace.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>Collection of XElements objects.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<XElement> GetElements(this XElement e, string ns, string name) =>
            e.GetNamespace(ns) is XNamespace xns ? e.Elements(xns + name) : Enumerable.Empty<XElement>();

        #endregion

        #region GetDecendants

        /* ----------------------------------------------------------------- */
        ///
        /// GetDecendants
        ///
        /// <summary>
        /// Gets the collection of descendant XElement objects corresponding
        /// to the specified name in the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>Collection of XElements objects.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<XElement> GetDescendants(this XElement e, string name) =>
            e.GetDescendants(string.Empty, name);

        /* ----------------------------------------------------------------- */
        ///
        /// GetDecendants
        ///
        /// <summary>
        /// Gets the collection of descendant XElement objects corresponding
        /// to the specified name in the default namespace.
        /// When the specified namespace is empty, the method uses the
        /// default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="ns">Element namespace.</param>
        /// <param name="name">Element name.</param>
        ///
        /// <returns>Collection of XElements objects.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<XElement> GetDescendants(this XElement e, string ns, string name) =>
            e.GetNamespace(ns) is XNamespace xns ? e.Descendants(xns + name) : Enumerable.Empty<XElement>();

        #endregion

        #region GetValueOrAttribute

        /* ----------------------------------------------------------------- */
        ///
        /// GetValueOrAttribute
        ///
        /// <summary>
        /// Gets the value corresponding to the specified name,
        /// or attribute value corresponding to the specified hint in the
        /// default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="name">Element name.</param>
        /// <param name="hint">Attribute name to be a hint.</param>
        ///
        /// <returns>Value or attribute.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValueOrAttribute(this XElement e, string name, string hint) =>
            e.GetValueOrAttribute(string.Empty, name, hint);

        /* ----------------------------------------------------------------- */
        ///
        /// GetValueOrAttribute
        ///
        /// <summary>
        /// Gets the value corresponding to the specified name,
        /// or attribute value corresponding to the specified hint in the
        /// specified namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="ns">Element namespace.</param>
        /// <param name="name">Element name.</param>
        /// <param name="hint">Attribute name to be a hint.</param>
        ///
        /// <returns>Value or attribute.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValueOrAttribute(this XElement e, string ns, string name, string hint) =>
            e.GetElement(ns, name).GetValueOrAttribute(hint);

        /* ----------------------------------------------------------------- */
        ///
        /// GetValueOrAttribute
        ///
        /// <summary>
        /// Gets the value of the specified element, or attribute value
        /// corresponding to the specified hint in the default namespace.
        /// </summary>
        ///
        /// <param name="e">Source object.</param>
        /// <param name="hint">Attribute name to be a hint.</param>
        ///
        /// <returns>Value or attribute.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string GetValueOrAttribute(this XElement e, string hint)
        {
            if (e == null) return string.Empty;
            if (e.Value.HasValue()) return e.Value;
            if (!hint.HasValue()) return string.Empty;
            return (string)e.Attribute(hint) ?? string.Empty;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetNamespace
        ///
        /// <summary>
        /// Gets the namespace object corresponding to the specified value.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static XNamespace GetNamespace(this XElement e, string prefix) =>
            prefix.HasValue() ?
            e.GetNamespaceOfPrefix(prefix) :
            e.GetDefaultNamespace();

        #endregion
    }
}
