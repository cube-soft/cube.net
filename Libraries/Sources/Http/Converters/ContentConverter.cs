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
    public interface IContentConverter<out TValue>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /// <remarks>
        /// HttpMonitor のように、例外発生時に既定回数再試行するような
        /// クラスで使用された場合、変換の失敗が原因で余分な通信を発生
        /// させる可能性があります。そのような場合には IgnoreException を
        /// true にして抑制します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        bool IgnoreException { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// 変換処理を実行します。
        /// </summary>
        ///
        /// <param name="src">Stream オブジェクト</param>
        ///
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        TValue Convert(Stream src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ContentConverterBase
    ///
    /// <summary>
    /// HttpContent を変換するための基底クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class ContentConverterBase<TValue> : IContentConverter<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ContentConverterBase
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected ContentConverterBase() { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        ///
        /// <summary>
        /// 変換処理を実行します。
        /// </summary>
        ///
        /// <param name="src">Stream オブジェクト</param>
        ///
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public TValue Convert(Stream src)
        {
            try { return OnConvert(src); }
            catch (Exception err)
            {
                if (IgnoreException) this.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(TValue);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnConvert
        ///
        /// <summary>
        /// 変換処理を実行します。
        /// </summary>
        ///
        /// <param name="src">Stream オブジェクト</param>
        ///
        /// <returns>変換後のオブジェクト</returns>
        ///
        /// <remarks>
        /// 継承したクラスで実装する必要があります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual TValue OnConvert(Stream src) => throw new NotImplementedException();

        #endregion
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
    public class ContentConverter<TValue> : ContentConverterBase<TValue>
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
        public ContentConverter(Func<Stream, TValue> func)
        {
            _func = func;
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnConvert
        ///
        /// <summary>
        /// 変換処理を実行します。
        /// </summary>
        ///
        /// <param name="src">Stream オブジェクト</param>
        ///
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected override TValue OnConvert(Stream src) => _func(src);

        #endregion

        #region Fields
        private readonly Func<Stream, TValue> _func;
        #endregion
    }
}
