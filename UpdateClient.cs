/* ------------------------------------------------------------------------- */
///
/// UpdateClient.cs
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

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.UpdateClient
    /// 
    /// <summary>
    /// アップデートを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class UpdateClient
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// アップデート用 URL を取得または設定します。。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri Uri { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// MessageUri
        ///
        /// <summary>
        /// アップデートメッセージ用 URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Uri MessageUri { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// HttpHandler
        ///
        /// <summary>
        /// HTTP 通信時に追加的な処理を行うためのオブジェクトを取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMessageHandler HttpHandler { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsync
        ///
        /// <summary>
        /// アップデートを非同期で実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task RunAsync()
        {
            await Task.Factory.StartNew(() => { });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetMessageAsync
        ///
        /// <summary>
        /// アップデートメッセージを非同期で取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<string> GetMessageAsync()
        {
            await Task.Factory.StartNew(() => { });
            return string.Empty;
        }

        #endregion
    }
}
