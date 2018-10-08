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
using System.Net.NetworkInformation;

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
            Network.AvailabilityChanged -= WhenNetworkChanged;
            Network.AvailabilityChanged += WhenNetworkChanged;
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
        /// OnNetworkChanged
        ///
        /// <summary>
        /// NetworkChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnNetworkChanged(NetworkAvailabilityEventArgs e)
        {
            if (Network.Available && State == TimerState.Suspend) Resume(TimeSpan.FromMilliseconds(100));
            else if (!Network.Available && State == TimerState.Run) Suspend();
            NetworkChanged?.Invoke(this, e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenNetworkChanged
        ///
        /// <summary>
        /// NetworkChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenNetworkChanged(object s, NetworkAvailabilityEventArgs e) =>
            OnNetworkChanged(e);

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// オブジェクトを開放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージオブジェクトを開放するかどうか
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            Network.AvailabilityChanged -= WhenNetworkChanged;
            base.Dispose(disposing);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnPowerModeChanged
        ///
        /// <summary>
        /// 電源の状態が変更された時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnPowerModeChanged(EventArgs e)
        {
            if (!Network.Available && State == TimerState.Run) Suspend();
            else base.OnPowerModeChanged(e);
        }

        #endregion
    }
}
