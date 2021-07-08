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
using System.Security;
using Cube.FileSystem;
using Cube.FileSystem.DataContract;
using Microsoft.Win32;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// LockSetting
    ///
    /// <summary>
    /// Represents the locking information.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class LockSetting : SerializableBase
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
        public static string FileName => ".lockfile";

        /* ----------------------------------------------------------------- */
        ///
        /// Sid
        ///
        /// <summary>
        /// Gets or sets the SID of the user being logged on.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Sid
        {
            get => Get(() => GetCurrentUserSid());
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UserName
        ///
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string UserName
        {
            get => Get(() => Environment.UserName);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MachineName
        ///
        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string MachineName
        {
            get => Get(() => Environment.MachineName);
            set => Set(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// Gets a value indicating whether the device is in read-only.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly => Get(() => !Sid.Equals(GetCurrentUserSid()));

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
        ///
        /// <returns>LockSetting object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static LockSetting Load(string directory)
        {
            var src = Io.Combine(directory, FileName);
            if (Io.Exists(src)) return Format.Json.Deserialize<LockSetting>(src);

            var dest = new LockSetting();
            Format.Json.Serialize(src, dest);
            Io.SetAttributes(src, System.IO.FileAttributes.ReadOnly | System.IO.FileAttributes.Hidden);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Release
        ///
        /// <summary>
        /// Releses the lock.
        /// </summary>
        ///
        /// <param name="directory">Dicretory path.</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Release(string directory)
        {
            if (IsReadOnly) return;
            Io.Delete(Io.Combine(directory, FileName));
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetCurrentUserSid
        ///
        /// <summary>
        /// Gets the SID of the user being logged on.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetCurrentUserSid()
        {
            var keyword = "Volatile Environment";
            var cmp     = Environment.UserName;
            var option  = StringComparison.CurrentCultureIgnoreCase;

            foreach (var dest in Registry.Users.GetSubKeyNames())
            {
                try
                {
                    using var key = Registry.Users.OpenSubKey($@"{dest}\{keyword}", false);
                    if (key == null) continue;
                    var user = key.GetValue("UserName", string.Empty) as string;
                    if (user.Equals(cmp, option)) return dest;
                }
                catch (SecurityException) { /* Other's profile */ }
            }
            return string.Empty;
        }

        #endregion
    }
}
