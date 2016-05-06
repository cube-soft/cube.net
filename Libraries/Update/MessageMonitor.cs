/* ------------------------------------------------------------------------- */
///
/// MessageMonitor.cs
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
using System.Threading.Tasks;
using Cube.Net.Http;
using Cube.Log;

namespace Cube.Net.Update
{
    /* --------------------------------------------------------------------- */
    ///
    /// MessageMonitor
    /// 
    /// <summary>
    /// アップデートメッセージを監視するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class MessageMonitor : NetworkAwareMonitor
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MessageMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <remarks>
        /// SoftwareActivator オブジェクトを指定した場合、初回の通信で
        /// アクティブ化を行います。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public MessageMonitor(SoftwareActivator activator = null) : base()
        {
            Interval = TimeSpan.FromDays(1);
            _activator = activator;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// EndPoint
        ///
        /// <summary>
        /// メッセージ確認用の URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri EndPoint { get; set; }

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Received
        ///
        /// <summary>
        /// アップデートメッセージの受信時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<ValueEventArgs<Message>> Received;

        #endregion

        #region Virtual methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnReceived
        ///
        /// <summary>
        /// Received イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnReceived(ValueEventArgs<Message> e)
            => Received?.Invoke(this, e);

        #endregion

        #region Override methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnExecute
        ///
        /// <summary>
        /// 定期的に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async void OnExecute(EventArgs e)
        {
            base.OnExecute(e);

            if (State != SchedulerState.Run ||
                EndPoint == null || Version == null) return;

            var message = await GetAsync();
            if (message == null) return;
            OnReceived(ValueEventArgs.Create(message));
        }

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync
        ///
        /// <summary>
        /// アップデートメッセージを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<Message> GetAsync()
        {
            if (EndPoint == null || Version == null) return null;
            if (_activator != null　&& _activator.Required) return await ActivateAsync();

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    return await ClientFactory.Create(Timeout)
                        .GetUpdateMessageAsync(EndPoint, Version);
                }
                catch (Exception err) { this.LogError(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }

            return null;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ActivateAsync
        ///
        /// <summary>
        /// アクティブ化を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task<Message> ActivateAsync()
        {
            try
            {
                if (_activator == null) return null;
                _activator.Version = Version;
                _activator.Secondary = EndPoint;
                await _activator.RunAsync();

                return null;
            }
            finally { _activator = null; }
        }

        #endregion

        #region Fields
        private SoftwareActivator _activator = null;
        #endregion
    }
}
