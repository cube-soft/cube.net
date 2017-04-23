/* ------------------------------------------------------------------------- */
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
using Cube.Log;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkAwareTimer
    ///
    /// <summary>
    /// ネットワーク状況の変化に反応するタイマーです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class NetworkAwareTimer : WakeableTimer
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
        public NetworkAwareTimer() : this(TimeSpan.FromHours(1)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAwareMonitor
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="interval">実行周期</param>
        ///
        /* ----------------------------------------------------------------- */
        public NetworkAwareTimer(TimeSpan interval) : base(interval)
        {
            Network.AvailabilityChanged += (s, e) => OnNetworkChanged(e);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkAvailable
        /// 
        /// <summary>
        /// ネットワークが使用可能な状態かどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool NetworkAvailable => Network.Available;

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
        /// OnNetworkChanged
        /// 
        /// <summary>
        /// NetworkChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnNetworkChanged(NetworkAvailabilityEventArgs e)
        {
            var prev = State;
            if (NetworkAvailable)
            {
                if (State == TimerState.Suspend) Resume();
            }
            else if (State == TimerState.Run) Suspend();
            this.LogDebug($"NetworkAvailable:{NetworkAvailable}\tState:{prev}->{State}");

            NetworkChanged?.Invoke(this, e);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Publish
        /// 
        /// <summary>
        /// モニタリングのための操作を実行するタイミングになった時に
        /// 実行されます。
        /// </summary>
        /// 
        /// <remarks>
        /// ネットワークが利用不可能な状態の場合 Suspend を実行します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Publish()
        {
            if (!NetworkAvailable) Suspend();
            else base.Publish();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnPowerModeChanged
        /// 
        /// <summary>
        /// 電源の状態が変更された時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnPowerModeChanged(PowerModeChangedEventArgs e)
        {
            if (!NetworkAvailable)
            {
                if (State == TimerState.Run) Suspend();
                return;
            }
            base.OnPowerModeChanged(e);
        }

        #endregion
    }
}
