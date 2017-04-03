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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// IContentConverter
    ///
    /// <summary>
    /// HttpContent を変換するためのインターフェースです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public interface IContentConverter<TValue>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        Task<TValue> ConvertAsync(HttpContent src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GenericContentConverter(TValue)
    ///
    /// <summary>
    /// 関数オブジェクトを IContentConverter に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class GenericContentConverter<TValue> : IContentConverter<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// GenericContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public GenericContentConverter(Func<HttpContent, Task<TValue>> func)
        {
            _func = func;
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public Task<TValue> ConvertAsync(HttpContent src) => _func?.Invoke(src);

        #endregion

        #region Fields
        private Func<HttpContent, Task<TValue>> _func;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ContentConvertHandler(TValue)
    ///
    /// <summary>
    /// HttpContent を TValue オブジェクトに変換するための HTTP
    /// ハンドラです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ContentConvertHandler<TValue> : HttpClientHandler
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ContentConvertHandler
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ContentConvertHandler() : base()
        {
            Proxy    = null;
            UseProxy = false;

            if (SupportsAutomaticDecompression)
            {
                AutomaticDecompression = DecompressionMethods.Deflate |
                                         DecompressionMethods.GZip;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GenericContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public ContentConvertHandler(IContentConverter<TValue> converter) : this()
        {
            Converter = converter;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GenericContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public ContentConvertHandler(Func<HttpContent, Task<TValue>> func)
            : this(new GenericContentConverter<TValue>(func)) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Converter
        ///
        /// <summary>
        /// 変換用オブジェクトを取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public IContentConverter<TValue> Converter { get; set; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// SendAsync
        ///
        /// <summary>
        /// HTTP リクエストを非同期で送信します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response?.Content != null && Converter != null)
            {
                response.Content = new ValueContent<TValue>(
                    response.Content,
                    await Converter.ConvertAsync(response.Content)
                );
            }
            return response;
        }

        #endregion
    }

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
            length = Source?.Headers?.ContentLength ?? -1;
            return length != -1;
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
            base.Dispose(disposing);
            if (disposing) Source?.Dispose();
        }

        #endregion
    }
}
