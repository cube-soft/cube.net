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
using System.Runtime.Serialization;
using Cube.DataContract;
using Cube.FileSystem;
using Cube.Mixin.IO;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SharedSetting
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class SharedSetting : SerializableBase
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
        public static string FileName => "Settings.json";

        /* ----------------------------------------------------------------- */
        ///
        /// StartUri
        ///
        /// <summary>
        /// Gets or sets the URL of the RssEntry object to be displayed at
        /// startup.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember(Name = "Start")]
        public Uri StartUri
        {
            get => Get<Uri>();
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Capacity
        ///
        /// <summary>
        /// Gets or sets the maximum number of RSS feed items to be retained
        /// in memory.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public int Capacity
        {
            get => Get(() => 100);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LightMode
        ///
        /// <summary>
        /// Gets or sets a value indicating whether to apply the lightweight
        /// mode.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool LightMode
        {
            get => Get(() => false);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableNewWindow
        ///
        /// <summary>
        /// Gets or sets a value indicating whether Open in new window is
        /// enabled.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool EnableNewWindow
        {
            get => Get(() => false);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EnableMonitorMessage
        ///
        /// <summary>
        /// Gets or sets a value indicating whether the status of RSS feed
        /// acquisition is displayed in the status bar.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool EnableMonitorMessage
        {
            get => Get(() => true);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CheckUpdate
        ///
        /// <summary>
        /// Gets or sets a value indicating whether or not to check for
        /// software updates.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public bool CheckUpdate
        {
            get => Get(() => true);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// HighInterval
        ///
        /// <summary>
        /// Gets or sets the check interval of the high frequency monitor.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public TimeSpan HighInterval
        {
            get => Get(() => TimeSpan.FromHours(1));
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LowInterval
        ///
        /// <summary>
        /// Gets or sets the check interval of the low frequency monitor.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public TimeSpan LowInterval
        {
            get => Get(() => TimeSpan.FromHours(24));
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InitialDelay
        ///
        /// <summary>
        /// Gets or sets the initial delay time for monitoring.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan InitialDelay
        {
            get => Get(() => TimeSpan.FromSeconds(3));
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LastCheckUpdate
        ///
        /// <summary>
        /// Get or set the date and time of the last software update
        /// confirmation.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastCheckUpdate
        {
            get => Get<DateTime?>();
            set => Set(value);
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// Loads the settings from the specified arguments.
        /// </summary>
        ///
        /// <param name="directory">Dicretory path.</param>
        /// <param name="io">I/O handler.</param>
        ///
        /// <returns>SharedSetting object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static SharedSetting Load(string directory, IO io) => io.LoadOrDefault(
            io.Combine(directory, FileName),
            e => Format.Json.Deserialize<SharedSetting>(e),
            new SharedSetting()
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// Saves the current settings to the specified path.
        /// </summary>
        ///
        /// <param name="directory">Dicretory path.</param>
        /// <param name="io">I/O handler.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Save(string directory, IO io) => io.Save(
            io.Combine(directory, FileName),
            e => Format.Json.Serialize(e, this)
        );

        #endregion
    }
}
