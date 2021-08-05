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
    /// Specifies the type of RSS and Atom feeds.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum RssVersion
    {
        /// <summary>RSS 0.91.</summary>
        Rss091,
        /// <summary>RSS 0.92.</summary>
        Rss092,
        /// <summary>RSS 1.0.</summary>
        Rss100,
        /// <summary>RSS 2.0.</summary>
        Rss200,
        /// <summary>Atom.</summary>
        Atom,
        /// <summary>Unknown.</summary>
        Unknown,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssVersionConverter
    ///
    /// <summary>
    /// Provides extended methods of the RssVersion enum.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssVersionConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetRssVersion
        ///
        /// <summary>
        /// Creates a new instance of the RssVersion class with the specified
        /// XML element.
        /// </summary>
        ///
        /// <param name="src">Source XML element.</param>
        ///
        /// <returns>RssVersion object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssVersion GetRssVersion(this XElement src)
        {
            var name = src.Name.LocalName.ToLowerInvariant();
            if (name == "rss")
            {
                switch ((string)src.Attribute("version") ?? string.Empty)
                {
                    case "0.91": return RssVersion.Rss091;
                    case "0.92": return RssVersion.Rss092;
                    case "2.0":  return RssVersion.Rss200;
                    default: break;
                }
            }
            else
            {
                var ns = src.GetDefaultNamespace().NamespaceName.ToLowerInvariant();
                if (name == "feed" && ns.Contains("atom")) return RssVersion.Atom;
                if (name == "rdf"  && ns.Contains("rss"))  return RssVersion.Rss100;
            }
            return RssVersion.Unknown;
        }
    }
}
