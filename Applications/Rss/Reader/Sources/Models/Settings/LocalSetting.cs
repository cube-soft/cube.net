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
using System.Runtime.Serialization;
using Cube.DataContract;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// LocalSetting
    ///
    /// <summary>
    /// Represents the user settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class LocalSetting : SerializableBase
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// Get the name of the file that will hold the configuration.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string FileName => "LocalSettings.json";

        /* ----------------------------------------------------------------- */
        ///
        /// FeedFileName
        ///
        /// <summary>
        /// Gets the name of the file used to hold the feed list.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string FeedFileName => "Feeds.json";

        /* ----------------------------------------------------------------- */
        ///
        /// CacheDirectoryName
        ///
        /// <summary>
        /// Get the name of the directory where the cache files are stored.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string CacheDirectoryName => "Cache";

        /* ----------------------------------------------------------------- */
        ///
        /// Width
        ///
        /// <summary>
        /// Gets or sets the width of the main window.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Width
        {
            get => Get(() => 1100);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Height
        ///
        /// <summary>
        /// Gets or sets the height of the main window.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Height
        {
            get => Get(() => 650);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EntryColumn
        ///
        /// <summary>
        /// Gets or sets the width of the RSS entry display area.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int EntryColumn
        {
            get => Get(() => 230);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ArticleColumn
        ///
        /// <summary>
        /// Gets or sets the width of the new articles display area.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int ArticleColumn
        {
            get => Get(() => 270);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DataDirectory
        ///
        /// <summary>
        /// Gets or sets the path of the directory for data storage.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string DataDirectory
        {
            get => Get<string>();
            set => Set(value);
        }

        #endregion
    }
}
