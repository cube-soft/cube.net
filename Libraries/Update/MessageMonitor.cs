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
using System.Collections.Generic;
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
        /* ----------------------------------------------------------------- */
        public MessageMonitor() : base()
        {
            Interval = TimeSpan.FromDays(1);
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

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// アプリケーションのバージョンを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Version Version { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// VersionDigit
        ///
        /// <summary>
        /// バージョン番号の有効桁数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int VersionDigit { get; set; } = 3;

        /* ----------------------------------------------------------------- */
        ///
        /// VersionPostfix
        ///
        /// <summary>
        /// バージョンの末尾に付与する文字列（α、β、…）を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string VersionPostfix { get; set; } = string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// OneTimeActivation
        ///
        /// <summary>
        /// 最初の通信でアクティブ化を行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool OneTimeActivation { get; set; }

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

            var http = new System.Net.Http.HttpClient(new ClientHandler
            {
                UseProxy = false,
                Proxy = null
            });
            http.Timeout = Timeout;
            http.DefaultRequestHeaders.ConnectionClose = true;

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    return await http.GetUpdateMessageAsync(
                        EndPoint,
                        CreateVersion(),
                        CreateQuery()
                    );
                }
                catch (Exception err) { this.LogError(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }

            return null;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateVersion
        ///
        /// <summary>
        /// バージョンを表す文字列を生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string CreateVersion()
        {
            var dest = Version.ToString(VersionDigit);
            return string.IsNullOrEmpty(VersionPostfix) ?
                   dest :
                   dest + VersionPostfix;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateQuery
        ///
        /// <summary>
        /// クエリーを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private IDictionary<string, string> CreateQuery()
        {
            var dest = new Dictionary<string, string>();
            if (OneTimeActivation)
            {
                dest.Add("flag", "install");
                OneTimeActivation = false;
            }
            return dest;
        }

        #endregion
    }
}
