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
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
    /// ContentConverter(TValue)
    ///
    /// <summary>
    /// 関数オブジェクトを IContentConverter に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ContentConverter<TValue> : IContentConverter<TValue>
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
        public ContentConverter(Func<HttpContent, Task<TValue>> func)
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
    /// JsonContentConverter(TValue)
    ///
    /// <summary>
    /// JSON 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class JsonContentConverter<TValue> : IContentConverter<TValue>
        where TValue : class
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
        public async Task<TValue> ConvertAsync(HttpContent src)
            => new DataContractJsonSerializer(typeof(TValue))
            .ReadObject(await src.ReadAsStreamAsync()) as TValue;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// XmlContentConverter(TValue)
    ///
    /// <summary>
    /// XML 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class XmlContentConverter<TValue> : IContentConverter<TValue>
        where TValue : class
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
        public async Task<TValue> ConvertAsync(HttpContent src)
            => new XmlSerializer(typeof(TValue))
            .Deserialize(await src.ReadAsStreamAsync()) as TValue;
    }
}
