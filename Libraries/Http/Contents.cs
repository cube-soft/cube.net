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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ValueContent(TValue)
    ///
    /// <summary>
    /// HttpContent を変換した結果を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal class ValueContent<TValue> : HttpContent
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ValueContent
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="src">変換前の HttpContent オブジェクト</param>
        /// <param name="value">変換後のオブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public ValueContent(HttpContent src, TValue value)
        {
            Source = src;
            Value = value;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Source
        ///
        /// <summary>
        /// 変換前のオブジェクトを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public HttpContent Source { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// 変換後のオブジェクトを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public TValue Value { get; }

        #endregion

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~ValueContent
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~ValueContent()
        {
            Dispose(false);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを解放します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) Source.Dispose();

            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SerializeToStreamAsync
        ///
        /// <summary>
        /// 非同期で HTTP コンテンツをシリアライズし Stream オブジェクトに
        /// コピーします。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            => Source.CopyToAsync(stream, context);

        /* ----------------------------------------------------------------- */
        ///
        /// TryComputeLength
        ///
        /// <summary>
        /// HTTP コンテンツのバイト数の取得を試みます。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected override bool TryComputeLength(out long length)
        {
            length = Source.Headers?.ContentLength ?? -1;
            return length != -1;
        }

        #region Fields
        private bool _disposed = false;
        #endregion

        #endregion
    }
}
