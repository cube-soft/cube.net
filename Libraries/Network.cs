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
using Cube.Log;
using Cube.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Network
    ///
    /// <summary>
    /// ネットワーク状況を検証するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class Network
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Network
        ///
        /// <summary>
        /// 静的オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        static Network()
        {
            NetworkChange.NetworkAvailabilityChanged += WhenChanged;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Status
        ///
        /// <summary>
        /// ネットワーク状況を示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static OperationalStatus Status
        {
            get
            {
                var ns = GetNetworkInterfaces();
                var dest = ns.FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up) ??
                           ns.FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Testing);
                return dest?.OperationalStatus ?? OperationalStatus.Down;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Available
        ///
        /// <summary>
        /// ネットワークが利用可能かどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static bool Available
        {
            get
            {
                var ns   = GetNetworkInterfaces();
                var dest = ns.FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);
                return dest != null;
            }
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// AvailabilityChanged
        ///
        /// <summary>
        /// ネットワークの利用可能状況が変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static event NetworkAvailabilityChangedEventHandler AvailabilityChanged;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// DisableOptions
        ///
        /// <summary>
        /// ネットワークに関連するオプションを無効化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static void DisableOptions()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            WebRequest.DefaultWebProxy = null;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetNetworkInterfaces
        ///
        /// <summary>
        /// ネットワークインターフェースの一覧を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static IEnumerable<NetworkInterface> GetNetworkInterfaces() =>
            NetworkInterface.GetAllNetworkInterfaces().Where(n =>
                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
               !n.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase)
            );

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChanged
        ///
        /// <summary>
        /// ネットワークの利用可能状況が変化した時に実行されるハンドラです。
        /// </summary>
        ///
        /// <remarks>
        /// IsAvailable が true に変化した直後は、実際にはまだ通信可能な
        /// 状態になっていない事があります。Network クラスでは、
        /// IsAvailable が true になってからいずれかのネットワーク
        /// インターフェースの状態が Up になるまで、最大 2 分の待機時間を
        /// 設けています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private static void WhenChanged(object s, NetworkAvailabilityEventArgs e)
        {
            if (!e.IsAvailable) AvailabilityChanged?.Invoke(s, e);
            else Task.Run(async () =>
            {
                var type = typeof(Network);
                for (var i = 0; i < 24; ++i) // 5 sec * 24 = 2 min
                {
                    if (Status == OperationalStatus.Up)
                    {
                        Logger.Debug(type, ($"Status:Up ({i * 5} sec)"));
                        AvailabilityChanged?.Invoke(s, e);
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
                Logger.Debug(type, $"Status:{Status} (Timeout)");
                AvailabilityChanged?.Invoke(s, e);
            }).Forget();
        }

        #endregion
    }
}