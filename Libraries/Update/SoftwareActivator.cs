/* ------------------------------------------------------------------------- */
///
/// SoftwareActivator.cs
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
using System.Net.Http;
using Cube.Net.Http;
using Cube.Conversions;
using Cube.Log;

namespace Cube.Net.Update
{
    /* --------------------------------------------------------------------- */
    ///
    /// SoftwareActivator
    /// 
    /// <summary>
    /// ソフトウェアのアクティブ化を行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SoftwareActivator
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SoftwareActivator
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SoftwareActivator() { }

        /* ----------------------------------------------------------------- */
        ///
        /// SoftwareActivator
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SoftwareActivator(string[] args) : this()
        {
            Required = args?.Length > 0 && args[0] == "install";
            Parse(args);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// CommonUri
        ///
        /// <summary>
        /// アクティブ化を行うための共通 URL を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static Uri EndPoint { get; }
            = new Uri("http://link.cube-soft.jp/install.php");

        /* ----------------------------------------------------------------- */
        ///
        /// Required
        ///
        /// <summary>
        /// アクティブ化を要求されているかどうかを示す値を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Required { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// ソフトウェアのバージョンを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SoftwareVersion Version { get; set; } = new SoftwareVersion();

        /* ----------------------------------------------------------------- */
        ///
        /// Timeout
        ///
        /// <summary>
        /// タイムアウト時間を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

        /* ----------------------------------------------------------------- */
        ///
        /// Utm
        ///
        /// <summary>
        /// UTM クエリパラメータを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public UtmQuery Utm { get; } = new UtmQuery();

        /* ----------------------------------------------------------------- */
        ///
        /// Secondary
        ///
        /// <summary>
        /// EndPoint の他に通信する URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Secondary { get; set; } = null;

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Received
        ///
        /// <summary>
        /// サーバからの応答を受信した時に発生するイベントです。
        /// </summary>
        /// 
        /// <remarks>
        /// EndoPoint または Secondary からの応答を受信した時に発生します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public event EventHandler<ValueEventArgs<HttpResponseMessage>> Received;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsync
        ///
        /// <summary>
        /// アクティブ化を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task RunAsync()
        {
            try
            {
                await RunAsyncPrimary();
                await RunAsyncSecondary();
            }
            catch (Exception err) { this.LogError(err.Message, err); }
        }

        #endregion

        #region Virutal methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnReceived
        ///
        /// <summary>
        /// Received メッセージを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnReceived(ValueEventArgs<HttpResponseMessage> e)
            => Received?.Invoke(this, e);

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Parse
        ///
        /// <summary>
        /// 引数を解析します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Parse(string[] args)
        {
            if (args == null || args.Length <= 1) return;

            for (var i = 0; i < args.Length - 1; ++i)
            {
                this.LogException(() =>
                {
                    if (args[i].ToLower() == "/source") Utm.Source = Normalize(args[i + 1]);
                    if (args[i].ToLower() == "/medium") Utm.Medium = Normalize(args[i + 1]);
                    if (args[i].ToLower() == "/campaign") Utm.Campaign = Normalize(args[i + 1]);
                    if (args[i].ToLower() == "/term") Utm.Term = Normalize(args[i + 1]);
                    if (args[i].ToLower() == "/content") Utm.Content = Normalize(args[i + 1]);
                });
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Normalize
        ///
        /// <summary>
        /// 文字列を正規化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Normalize(string src)
            => src.Replace("\"", "");

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsyncPrimary
        ///
        /// <summary>
        /// 1 段階目のアクティブ化を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task RunAsyncPrimary()
        {
            if (!Required) return;

            var uri = EndPoint.With(Utm);
            var client = ClientFactory.Create(Timeout);
            var response = await client.GetAsync(EndPoint.With(Utm));
            if (response == null) return;

            OnReceived(ValueEventArgs.Create(response));
            this.LogDebug($"Primary\tStatusCode:{response.StatusCode}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsyncSecondary
        ///
        /// <summary>
        /// 2 段階目のアクティブ化を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task RunAsyncSecondary()
        {
            if (!Required || Secondary == null) return;

            var uri = Secondary.With("ver", Version).With("flag", "install");
            var client = ClientFactory.Create(Timeout);
            var response = await client.GetAsync(uri);
            if (response == null) return;

            OnReceived(ValueEventArgs.Create(response));
            this.LogDebug($"Secondary\tStatusCode:{response.StatusCode}");
        }

        #endregion
    }
}
