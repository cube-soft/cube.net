/* ------------------------------------------------------------------------- */
///
/// NetworkAwareObserver.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Net.NetworkInformation;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.NetworkAwareObserver
    ///
    /// <summary>
    /// ネットワークの状況変化に反応する Observer の基底となるクラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// このクラスは直接オブジェクト化する事はできません。継承クラスを
    /// 利用して下さい。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public class NetworkAwareObserver
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAwareObserver
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected NetworkAwareObserver()
        {
            NetworkChange.NetworkAvailabilityChanged += (s, e) => OnNetworkChanged(e);
        }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkChanged
        /// 
        /// <summary>
        /// ネットワークの状況が変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<NetworkAvailabilityEventArgs> NetworkChanged;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        /// 
        /// <summary>
        /// 監視を開始します。
        /// </summary>
        /// 
        /// <remarks>
        /// 継承クラスでは ExecuteStart() をオーバーライドし、必要な実装を
        /// 行って下さい。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Start()
        {
            ExecuteStart();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ExecuteStop
        /// 
        /// <summary>
        /// 監視を中止します。
        /// </summary>
        /// 
        /// <remarks>
        /// 継承クラスでは ExecuteStop() をオーバーライドし、必要な実装を
        /// 行って下さい。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop()
        {
            ExecuteStop();
        }

        #endregion

        #region Virtual methods

        /* ----------------------------------------------------------------- */
        ///
        /// ExecuteStart
        /// 
        /// <summary>
        /// 監視を開始します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void ExecuteStart() { }

        /* ----------------------------------------------------------------- */
        ///
        /// ExecuteStop
        /// 
        /// <summary>
        /// 監視を中止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void ExecuteStop() { }

        /* ----------------------------------------------------------------- */
        ///
        /// OnNetworkChanged
        /// 
        /// <summary>
        /// ネットワークの状況が変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnNetworkChanged(NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable) Start();
            else Stop();

            if (NetworkChanged != null) NetworkChanged(this, e);
        }

        #endregion
    }
}
