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
using System.IO;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpMonitor
    ///
    /// <summary>
    /// 定期的に HTTP 通信を実行するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HttpMonitor<TValue> : HttpMonitorBase<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="func">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(Func<Stream, TValue> func) :
            this(new ContentConverter<TValue>(func)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(IContentConverter<TValue> converter) :
            this(new ContentHandler<TValue>(converter)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMonitor(ContentHandler<TValue> handler) : base(handler)
        {
            Timer.SubscribeAsync(WhenTick);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// Uris の最初の項目を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequestUri
        ///
        /// <summary>
        /// リクエスト送信先 URL を取得します。
        /// </summary>
        ///
        /// <returns>URL</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual Uri GetRequestUri() => Uri;

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenTick
        ///
        /// <summary>
        /// 一定間隔で実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private async Task WhenTick()
        {
            if (Subscriptions.Count <= 0) return;

            var uri = GetRequestUri();

            for (var i = 0; i <= RetryCount; ++i)
            {
                try
                {
                    if (State != TimerState.Run) return;
                    await PublishAsync(uri);
                    break;
                }
                catch (Exception err)
                {
                    this.LogWarn(err.ToString(), err);
                    await Task.Delay(RetryInterval).ConfigureAwait(false);
                    this.LogDebug($"Retry\tCount:{i + 1}\tUrl:{uri}");
                }
            }
        }

        #endregion
    }
}
