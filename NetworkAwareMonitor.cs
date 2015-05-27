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


namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.NetworkAwareMonitor
    ///
    /// <summary>
    /// ネットワーク状況の変化に反応するモニタリング用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class NetworkAwareMonitor : Cube.Scheduler
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
        public NetworkAwareMonitor() : this(DateTime.Now) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAwareMonitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public NetworkAwareMonitor(DateTime lastExecuted)
            : base(lastExecuted)
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
            if (State == SchedulerState.Stop) return;

            if (!e.IsAvailable && State == SchedulerState.Run) Suspend();
            else if (e.IsAvailable && State == SchedulerState.Suspend) Resume();

            if (NetworkChanged != null) NetworkChanged(this, e);
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
    }
}
