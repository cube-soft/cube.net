/* ------------------------------------------------------------------------- */
///
/// NetworkAwareMonitor.cs
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
using Microsoft.Win32;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkAwareMonitor
    ///
    /// <summary>
    /// ネットワーク状況の変化に反応するモニタリング用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class NetworkAwareMonitor : Scheduler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAwareMonitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NetworkAwareMonitor()
            : base()
        {
            PowerMode = PowerModes.Resume;
            NetworkChange.NetworkAvailabilityChanged += (s, e) => OnNetworkChanged(e);
            SystemEvents.PowerModeChanged += (s, e) => OnPowerModeChanged(e);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// PowerMode
        /// 
        /// <summary>
        /// 現在の電源モードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PowerModes PowerMode { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// IsNetworkAvailable
        /// 
        /// <summary>
        /// ネットワークが使用可能な状態かどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsNetworkAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
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

        /* ----------------------------------------------------------------- */
        ///
        /// PowerModeChanged
        /// 
        /// <summary>
        /// 電源の状態が変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<PowerModeChangedEventArgs> PowerModeChanged;

        #endregion

        #region Virtual methods

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
            ChangeState();
            if (NetworkChanged != null) NetworkChanged(this, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PowerModeChanged
        /// 
        /// <summary>
        /// 電源の状態が変化した時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnPowerModeChanged(PowerModeChangedEventArgs e)
        {
            PowerMode = e.Mode;
            ChangeState();
            if (PowerModeChanged != null) PowerModeChanged(this, e);
        }

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnExecute
        /// 
        /// <summary>
        /// モニタリングのための操作を実行するタイミングになった時に
        /// 発生するイベントです。
        /// </summary>
        /// 
        /// <remarks>
        /// ネットワークが利用不可能な状態の場合 Suspend を実行します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnExecute(EventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) Suspend();
            else base.OnExecute(e);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ChangeState
        /// 
        /// <summary>
        /// ネットワークや電源の状態に応じて State を切り替えます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void ChangeState()
        {
            if (State == SchedulerState.Stop) return;

            var available = (PowerMode == PowerModes.Resume) && IsNetworkAvailable;
            if (available && State == SchedulerState.Suspend) Resume();
            else if (!available && State == SchedulerState.Run) Suspend();
        }

        #endregion
    }
}
