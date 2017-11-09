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

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// BrowserVersion
    /// 
    /// <summary>
    /// ブラウザのバージョンを定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum BrowserVersion
    {
        /// <summary>IE7</summary>
        IE7 = 7000,
        /// <summary>IE8 互換モード</summary>
        IE8Quirks = 8000,
        /// <summary>IE8</summary>
        IE8Standards = 8888,
        /// <summary>IE9 互換モード</summary>
        IE9Quirks = 9000,
        /// <summary>IE9</summary>
        IE9Standards = 9999,
        /// <summary>IE10 互換モード</summary>
        IE10Quirks = 10000,
        /// <summary>IE10</summary>
        IE10Standards = 10001,
        /// <summary>IE11 互換モード</summary>
        IE11Quirks = 11000,
        /// <summary>Edge モード</summary>
        Edge = 11001,
        /// <summary>適用可能な最新バージョン</summary>
        Latest = -1,
    }

    /* --------------------------------------------------------------------- */
    ///
    /// BrowserSettings
    /// 
    /// <summary>
    /// Web ブラウザ用コントロールに関する各種設定情報を取得または
    /// 更新するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class BrowserSettings
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        /// 
        /// <summary>
        /// エミュレートされている IE バージョンを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static BrowserVersion Version
        {
            get => GetEmulateVersion();
            set => SetEmulateVersion(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GpuRendering
        /// 
        /// <summary>
        /// GPU レンダリングモードが有効かどうかを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static bool GpuRendering
        {
            get => GetGpuRendering();
            set => SetGpuRendering(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MaxConnections
        /// 
        /// <summary>
        /// 最大同時接続数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static int MaxConnections
        {
            get => GetMaxConnections();
            set => SetMaxConnections(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// NavigationSounds
        /// 
        /// <summary>
        /// クリック音等が有効かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static bool NavigationSounds
        {
            get => GetNavigationSounds();
            set => SetNavigationSounds(value);
        }

        #endregion

        #region Implementations

        #region Browser emulation

        /* ----------------------------------------------------------------- */
        ///
        /// GetEmulateVersion
        /// 
        /// <summary>
        /// エミュレートされている IE バージョンを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static BrowserVersion GetEmulateVersion() => Log(() =>
        {
            using (var root = OpenFeatureControl())
            using (var subkey = root.OpenSubKey(_RegEmulation, false))
            {
                if (subkey == null) return BrowserVersion.IE7;

                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                var version = subkey.GetValue(filename);
                return version != null ? (BrowserVersion)version : BrowserVersion.IE7;
            }
        }, BrowserVersion.IE7);

        /* ----------------------------------------------------------------- */
        ///
        /// SetEmulateVersion
        /// 
        /// <summary>
        /// エミュレートする IE バージョンを設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void SetEmulateVersion(BrowserVersion version) => Log(() =>
        {
            using (var root = OpenFeatureControl(true))
            using (var subkey = root.CreateSubKey(_RegEmulation))
            {
                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                var value = (version == BrowserVersion.Latest) ? GetLatestVersion() : version;
                subkey.SetValue(filename, (int)value);
            }
        });

        #endregion

        #region GPU rendering

        /* ----------------------------------------------------------------- */
        ///
        /// GetGpuRendering
        /// 
        /// <summary>
        /// GPU レンダリングモードが有効かどうかを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static bool GetGpuRendering() => Log(() =>
        {
            using (var root = OpenFeatureControl())
            using (var subkey = root.OpenSubKey(_RegRendering, false))
            {
                if (subkey == null) return false;

                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                var value = subkey.GetValue(filename);
                if (value == null) return false;
                return (int)value == 1;
            }
        }, false);

        /* ----------------------------------------------------------------- */
        ///
        /// SetGpuRendering
        /// 
        /// <summary>
        /// GPU レンダリングモードを有効にするかどうかを設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void SetGpuRendering(bool enabled) => Log(() =>
        {
            using (var root = OpenFeatureControl(true))
            using (var subkey = root.CreateSubKey(_RegRendering))
            {
                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                var value = enabled ? 1 : 0;
                subkey.SetValue(filename, value);
            }
        });

        #endregion

        #region Max connections

        /* ----------------------------------------------------------------- */
        ///
        /// GetMaxConnections
        /// 
        /// <summary>
        /// 最大同時接続数を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static int GetMaxConnections() => Log(() =>
        {
            const int default_max_connection = 6;
            using (var root = OpenFeatureControl())
            using (var subkey = root.OpenSubKey(_RegMaxConnections, false))
            {
                if (subkey == null) return default_max_connection;

                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                var value = subkey.GetValue(filename);
                if (value == null) return default_max_connection;
                return (int)value;
            }
        }, 6 /* default_max_connection */);

        /* ----------------------------------------------------------------- */
        ///
        /// SetMaxConnections
        /// 
        /// <summary>
        /// 最大同時接続数を設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void SetMaxConnections(int number) => Log(() =>
        {
            if (number < 2 || number > 128) return;

            using (var root = OpenFeatureControl(true))
            using (var subkey = root.CreateSubKey(_RegMaxConnections))
            using (var subkey10 = root.CreateSubKey(_RegMaxConnections10))
            {
                var module = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                var filename = System.IO.Path.GetFileName(module.FileName);
                subkey.SetValue(filename, number);
                subkey10.SetValue(filename, number);
            }
        });

        #endregion

        #region Navigation sounds

        /* ----------------------------------------------------------------- */
        ///
        /// GetNavigationSounds
        /// 
        /// <summary>
        /// クリック音等が有効かどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static bool GetNavigationSounds()
            => UrlMon.NativeMethods.CoInternetIsFeatureEnabled(
            21,     // FEATURE_DISABLE_NAVIGATION_SOUNDS
            0x02    // SET_FEATURE_ON_PROCESS
        ) != 0;

        /* ----------------------------------------------------------------- */
        ///
        /// SetNavigationSounds
        /// 
        /// <summary>
        /// クリック音等を有効または無効に設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void SetNavigationSounds(bool enabled)
            => UrlMon.NativeMethods.CoInternetSetFeatureEnabled(
            21,     // FEATURE_DISABLE_NAVIGATION_SOUNDS
            0x02,   // SET_FEATURE_ON_PROCESS
            !enabled
        );

        #endregion

        /* ----------------------------------------------------------------- */
        ///
        /// OpenFeatureControl
        /// 
        /// <summary>
        /// FeatureControl 直下のサブキーを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static Microsoft.Win32.RegistryKey OpenFeatureControl(bool writable = false)
        {
            var name = System.IO.Path.Combine(_RegRoot, @"Main\FeatureControl");
            return writable ?
                   Microsoft.Win32.Registry.CurrentUser.CreateSubKey(name) :
                   Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetLatestVersion
        /// 
        /// <summary>
        /// PC にインストールされた最新の IE バージョンを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static BrowserVersion GetLatestVersion() => Log(() =>
        {
            using (var subkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_RegRoot))
            {
                var value = subkey.GetValue("svcVersion") as string;
                if (value == null) value = subkey.GetValue("Version") as string;
                if (value == null) return BrowserVersion.IE7;

                switch (int.Parse(value.Substring(0, value.IndexOf('.'))))
                {
                    case 7:  return BrowserVersion.IE7;
                    case 8:  return BrowserVersion.IE8Standards;
                    case 9:  return BrowserVersion.IE9Standards;
                    case 10: return BrowserVersion.IE10Standards;
                    case 11: return BrowserVersion.Edge;
                    default: return BrowserVersion.IE7;
                }
            }
        }, BrowserVersion.IE7);

        /* ----------------------------------------------------------------- */
        ///
        /// Log
        /// 
        /// <summary>
        /// 例外情報をログに出力します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static T Log<T>(Func<T> func, T alternate)
        {
            try { return func(); }
            catch (Exception e)
            {
                Cube.Log.Operations.Warn(typeof(BrowserSettings), e.ToString(), e);
                return alternate;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Log
        /// 
        /// <summary>
        /// 例外情報をログに出力します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void Log(Action action)
        {
            try { action(); }
            catch (Exception e)
            {
                Cube.Log.Operations.Warn(typeof(BrowserSettings), e.ToString(), e);
            }
        }

        #endregion

        #region Fields
        private static readonly string _RegRoot = @"Software\Microsoft\Internet Explorer";
        private static readonly string _RegEmulation = "FEATURE_BROWSER_EMULATION";
        private static readonly string _RegRendering = "FEATURE_GPU_RENDERING ";
        private static readonly string _RegMaxConnections = "FEATURE_MAXCONNECTIONSPERSERVER";
        private static readonly string _RegMaxConnections10 = "FEATURE_MAXCONNECTIONSPER1_0SERVER";
        #endregion
    }
}
