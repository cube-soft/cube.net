/* ------------------------------------------------------------------------- */
///
/// Monitor.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Cube.Log;

namespace Cube.Net.News
{
    /* --------------------------------------------------------------------- */
    ///
    /// Monitor
    ///
    /// <summary>
    /// Cube ニュースサーバと定期的に通信を行い、新着記事を監視するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Monitor : NetworkAwareMonitor
    {
        #region Properties

        /* --------------------------------------------------------------------- */
        ///
        /// EndPoint
        /// 
        /// <summary>
        /// 新着ニュース取得用の URL を取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static Uri EndPoint => Client.EndPoint;

        /* --------------------------------------------------------------------- */
        ///
        /// Version
        /// 
        /// <summary>
        /// 通知するバージョンを取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public Version Version
        {
            get { return _client.Version; }
            set { _client.Version = value; }
        }

        /* --------------------------------------------------------------------- */
        ///
        /// Timeout
        /// 
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
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
        /// Result
        /// 
        /// <summary>
        /// 新着記事の一覧を取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public IList<Article> Result
        {
            get { return _result; }
            protected set
            {
                if (_result == value) return;
                _result = value;
                OnResultChanged(new ValueEventArgs<IList<Article>>(value));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FailedCount
        /// 
        /// <summary>
        /// サーバとの通信に失敗した回数を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// この値は Reset() を実行した時に 0 になります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public int FailedCount { get; private set; }

        #endregion

        #region Events

        /* --------------------------------------------------------------------- */
        ///
        /// ResultChanged
        /// 
        /// <summary>
        /// 新着記事の一覧が更新された時に発生するイベントです。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public event EventHandler<ValueEventArgs<IList<Article>>> ResultChanged;

        #endregion

        #region Virtual methods

        /* --------------------------------------------------------------------- */
        ///
        /// OnResultChanged
        /// 
        /// <summary>
        /// ResultChanged イベントを発生させます。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        protected virtual void OnResultChanged(ValueEventArgs<IList<Article>> e)
            => ResultChanged?.Invoke(this, e);

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnReset
        /// 
        /// <summary>
        /// 現在の LastResult プロパティの値を破棄し、NTP サーバに
        /// 問い合わせます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async void OnReset(EventArgs e)
        {
            var current = State;
            base.OnReset(e);

            Result = null;
            FailedCount = 0;

            if (current == SchedulerState.Run)
            {
                await GetAsync();
                Start();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnExecute
        /// 
        /// <summary>
        /// 一定間隔で実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async void OnExecute(EventArgs e)
        {
            base.OnExecute(e);
            if (State != SchedulerState.Run) return;
            await GetAsync();
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        /// 
        /// <summary>
        /// 非同期で NTP サーバと通信を行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task GetAsync()
        {
            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    var result = await _client.GetAsync();
                    if (result.Count > 0) Result = result;
                    break;
                }
                catch (Exception err) { this.LogError(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }
        }

        #endregion

        #region Fields
        private Client _client = new Client();
        private IList<Article> _result = new List<Article>();
        #endregion
    }
}
