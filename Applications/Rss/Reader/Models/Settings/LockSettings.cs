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
using Cube.FileSystem;
using Cube.FileSystem.Files;
using Cube.Settings;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Cube.Net.App.Rss.Reader
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
        public static LockSettings Load(string directory, Operator io)
        {
            var src = io.Combine(directory, FileName);
            if (io.Exists(src)) return io.Load(src, e => SettingsType.Json.Load<LockSettings>(e));

            var dest = new LockSettings();
            io.Save(src, e => SettingsType.Json.Save(e, dest));
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
        public void Release(string directory, Operator io)
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
        /// <remarks>
        /// 64bit Windows 上で 32bit エミュレーションで実行すると SID の
        /// 取得に失敗する。 System.DirectoryServices.AccountManagement を
        /// 利用した場合でも、サービスが起動しておらず例外が送出される事が
        /// あり、取得に成功する事を保証する方法が今のところ存在しない。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private string GetCurrentUserSid()
        {
            var name = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData";
            using (var root = Registry.LocalMachine.OpenSubKey(name, false))
            {
                if (root == null) return string.Empty;

                foreach (var subname in root.GetSubKeyNames())
                using (var src = root.OpenSubKey(subname, false))
                {
                    Debug.Assert(src != null);

                    var tmp = src.GetValue("LoggedOnUser") as string ??
                              src.GetValue("LoggedOnSAMUser") as string;
                    if (string.IsNullOrEmpty(tmp)) continue;

                    var index = tmp.IndexOf('\\');
                    var user = index >= 0 ? tmp.Substring(index + 1) : tmp;
                    if (user.Equals(Environment.UserName)) return src.GetValue("LoggedOnUserSID") as string;
                }
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
