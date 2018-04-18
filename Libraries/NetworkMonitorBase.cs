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
using System;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// NetworkMonitorBase
    ///
    /// <summary>
    /// 定期的にネットワーク通信を実行するための基底クラスです。
    /// </summary>
    ///
    /// <remarks>
    /// このクラスは NetworkAwareTimer の簡易ラッパークラスとなります。
    /// 実際に何らかの処理を実装する場合はこのクラスを継承し、
    /// Timer.Subscribe() に必要な処理を登録して下さい。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class NetworkMonitorBase : IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// NetworkMonitorBase
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected NetworkMonitorBase()
        {
            _dispose = new OnceAction<bool>(Dispose);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// State
        ///
        /// <summary>
        /// オブジェクトの状態を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimerState State => Timer.State;

        /* ----------------------------------------------------------------- */
        ///
        /// LastPublished
        ///
        /// <summary>
        /// 最後に Publish が実行された日時を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public DateTime? LastPublished => Timer.LastPublished;

        /* ----------------------------------------------------------------- */
        ///
        /// Interval
        ///
        /// <summary>
        /// 実行間隔を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Interval
        {
            get => Timer.Interval;
            set => Timer.Interval = value;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// RetryCount
        ///
        /// <summary>
        /// 通信失敗時に再試行する最大回数を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public int RetryCount { get; set; } = 5;

        /* --------------------------------------------------------------------- */
        ///
        /// RetryInterval
        ///
        /// <summary>
        /// 通信失敗時に再試行する間隔を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);

        /* ----------------------------------------------------------------- */
        ///
        /// Timer
        ///
        /// <summary>
        /// 内部用タイマーを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected NetworkAwareTimer Timer { get; } = new NetworkAwareTimer();

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 監視を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Start() => Start(TimeSpan.Zero);

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 監視を実行します。
        /// </summary>
        ///
        /// <param name="delay">初期遅延時間</param>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Start(TimeSpan delay)
        {
            var state = Timer.State;
            Timer.Start(delay);
            if (state != TimerState.Stop) return;
            this.LogDebug($"Start\tInterval:{Interval}\tInitialDelay:{delay}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// 監視を停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Stop()
        {
            var state = Timer.State;
            Timer.Stop();
            if (state == TimerState.Stop) return;
            this.LogDebug($"Stop\tLastPublished:{Timer.LastPublished}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Suspend
        ///
        /// <summary>
        /// 監視を一時停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Suspend() => Timer.Suspend();

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        ///
        /// <summary>
        /// リセットします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Reset()
        {
            var state = State;
            Timer.Reset();
            if (state == TimerState.Run)
            {
                Stop();
                Start();
            }
        }

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~NetworkMonitorBase
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~NetworkMonitorBase() { _dispose.Invoke(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            _dispose.Invoke(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージリソースを解放するかどうかを示す値
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Timer.Dispose();
        }

        #endregion

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        #endregion
    }
}
