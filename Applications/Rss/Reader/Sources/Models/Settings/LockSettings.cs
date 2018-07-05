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
using Cube.DataContract;
using Cube.FileSystem;
using Cube.FileSystem.Mixin;
using Microsoft.Win32;
using System;
using System.Runtime.Serialization;
using System.Security;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// LockSettings
    ///
    /// <summary>
    /// ロック情報を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class LockSettings : ObservableProperty
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// LockSettings
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LockSettings()
        {
            Reset();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// 設定を保持するファイルの名前を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static string FileName => ".lockfile";

        /* ----------------------------------------------------------------- */
        ///
        /// Sid
        ///
        /// <summary>
        /// 識別子を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string Sid
        {
            get => _sid;
            set => SetProperty(ref _sid, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UserName
        ///
        /// <summary>
        /// ユーザ名を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MachineName
        ///
        /// <summary>
        /// マシン名を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember]
        public string MachineName
        {
            get => _machineName;
            set => SetProperty(ref _machineName, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// 読み取り専用モードかどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly => _isReadOnly ?? (
            _isReadOnly = !Sid.Equals(GetCurrentUserSid())
        ).Value;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// 設定情報を読み込みます。
        /// </summary>
        ///
        /// <param name="directory">ディレクトリ</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /// <returns>LockSettings オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static LockSettings Load(string directory, IO io)
        {
            var src = io.Combine(directory, FileName);
            if (io.Exists(src)) return io.Load(src, e => Format.Json.Deserialize<LockSettings>(e));

            var dest = new LockSettings();
            io.Save(src, e => Format.Json.Serialize(e, dest));
            io.SetAttributes(src, System.IO.FileAttributes.ReadOnly | System.IO.FileAttributes.Hidden);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Release
        ///
        /// <summary>
        /// ロックを解除します。
        /// </summary>
        ///
        /// <param name="directory">ディレクトリ</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Release(string directory, IO io)
        {
            if (IsReadOnly) return;
            io.Delete(io.Combine(directory, FileName));
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnDeserializing
        ///
        /// <summary>
        /// デシリアライズ直前に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => Reset();

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// 値をリセットします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Reset()
        {
            _sid         = GetCurrentUserSid();
            _userName    = Environment.UserName;
            _machineName = Environment.MachineName;
            _isReadOnly  = default(bool?);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetCurrentUserSid
        ///
        /// <summary>
        /// ログオン中のユーザの SID を初期化します。
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
                    using (var key = Registry.Users.OpenSubKey($@"{dest}\{keyword}", false))
                    {
                        if (key == null) continue;
                        var user = key.GetValue("UserName", string.Empty) as string;
                        if (user.Equals(cmp, option)) return dest;
                    }
                }
                catch (SecurityException) { /* Other's profile */ }
            }
            return string.Empty;
        }

        #endregion

        #region Fields
        private string _sid;
        private string _userName;
        private string _machineName;
        private bool? _isReadOnly;
        #endregion
    }
}
