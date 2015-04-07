/* ------------------------------------------------------------------------- */
///
/// Observer.cs
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

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Ntp.Observer
    ///
    /// <summary>
    /// NTP サーバと通信を行い、時刻のずれを監視するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Observer
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(Client client)
        {
            _client = client;
            Interval = TimeSpan.FromHours(1);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(string hostNameOrAddress)
            : this(new Client(hostNameOrAddress)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Observer
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Observer(string hostNameOrAddress, int port)
            : this(new Client(hostNameOrAddress, port)) { } 

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// NTP サーバとの通信のタイムアウト時間を設定または取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Interval
        /// 
        /// <summary>
        /// NTP サーバに問い合わせる時間を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Interval { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// LastResult
        /// 
        /// <summary>
        /// NTP サーバと最後に通信した結果を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Packet LastResult { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// IsValid
        /// 
        /// <summary>
        /// 現在の結果が有効なものであるかどうかを表す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsValid
        {
            get
            {
                var packet = LastResult;
                return (packet != null && packet.IsValid);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LocalClockOffset
        /// 
        /// <summary>
        /// ローカル時刻とのずれを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan LocalClockOffset
        {
            get { return IsValid ? LastResult.LocalClockOffset : TimeSpan.Zero; }
        }

        #endregion

        #region Fields
        private Client _client = null;
        #endregion
    }
}
