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
using System.Runtime.InteropServices;

namespace Cube.Net.UrlMon
{
    /* --------------------------------------------------------------------- */
    ///
    /// UrlMon.NativeMethods
    ///
    /// <summary>
    /// urlmon.dll に定義された関数を宣言するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class NativeMethods
    {
        /* ----------------------------------------------------------------- */
        ///
        /// CoInternetIsFeatureEnabled
        ///
        /// <summary>
        /// https://msdn.microsoft.com/ja-jp/library/ms537164.aspx
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DllImport(LibName)]
        public static extern int CoInternetIsFeatureEnabled(int featureEntry, int dwFlags);

        /* ----------------------------------------------------------------- */
        ///
        /// CoInternetSetFeatureEnabled
        ///
        /// <summary>
        /// https://msdn.microsoft.com/ja-jp/library/ms537168.aspx
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DllImport(LibName)]
        public static extern int CoInternetSetFeatureEnabled(int FeatureEntry, int dwFlags, bool fEnable);

        #region Fields
        const string LibName = "urlmon.dll";
        #endregion
    }
}
