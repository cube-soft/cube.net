/* ------------------------------------------------------------------------- */
///
/// EntityTag.cs
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
using System.Net.Http;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// EntityTag
    ///
    /// <summary>
    /// EntityTag を表すクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class EntityTag
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// EntityTag
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public EntityTag() { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// EntityTag を表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Value { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// CreateHandler
        ///
        /// <summary>
        /// EntityTag 監視用の HttpClientHandler オブジェクトを生成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public HttpClientHandler CreateHandler()
        {
            var dest = new EntityTagHandler(Value);
            dest.Received += Handler_Received;
            return dest;
        }

        #endregion

        #region Event handlers

        /* ----------------------------------------------------------------- */
        ///
        /// Handler_Received
        ///
        /// <summary>
        /// EntityTag を受信時に実行されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Handler_Received(object sender, ValueEventArgs<string> e)
        {
            Value = e.Value;

            var handler = sender as EntityTagHandler;
            if (handler == null) return;

            handler.Received -= Handler_Received;
        }

        #endregion
    }
}
