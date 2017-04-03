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
using System.Threading.Tasks;
using System.Net.Http;
using Cube.Conversions;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Monitor(TValue)
    ///
    /// <summary>
    /// 定期的に HTTP 通信を実行するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Monitor<TValue> : NetworkAwareTimer
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="handler">HTTP 通信用ハンドラ</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(ContentConvertHandler<TValue> handler) : base()
        {
            _handler = handler;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(IContentConverter<TValue> converter)
            : this (new ContentConvertHandler<TValue>(converter)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Monitor(Func<HttpContent, Task<TValue>> func)
            : this(new ContentConvertHandler<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// EndPoint
        ///
        /// <summary>
        /// HTTP 通信を行う URL を取得または設定します。
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
        /// 新しい結果を受信した時に発生するイベントです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public event ValueEventHandler<TValue> Received;

        /* ----------------------------------------------------------------- */
        ///
        /// OnReceived
        ///
        /// <summary>
        /// Received イベントを発生させます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected virtual void OnReceived(ValueEventArgs<TValue> e)
            => Received?.Invoke(this, e);

        #endregion

        #region Implementations

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
            if (State != TimerState.Run || EndPoint == null) return;

            for (var i = 0; i < RetryCount; ++i)
            {
                try
                {
                    var http     = ClientFactory.Create(_handler, Timeout);
                    var response = await http.GetAsync(EndPoint.With(Version));

                    if (response?.Content is ValueContent<TValue> content)
                    {
                        OnReceived(ValueEventArgs.Create(content.Value));
                        return;
                    }
                }
                catch (Exception err) { this.LogWarn(err.Message, err); }
                ++FailedCount;
                await Task.Delay(RetryInterval);
            }

            this.LogWarn($"Failed\tCount:{FailedCount}");
        }

        #region Fields
        private ContentConvertHandler<TValue> _handler;
        #endregion

        #endregion
    }
}
