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
using System;
using System.IO;
using Cube.Mixin.Logging;

namespace Cube.Net.Http
{
    #region IContentConverter<TValue>

    /* --------------------------------------------------------------------- */
    ///
    /// IContentConverter
    ///
    /// <summary>
    /// Represents the interface to convert the HTTP response.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public interface IContentConverter<TValue>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// Gets or sets a value indicating whether to ignore exceptions.
        /// </summary>
        ///
        /// <remarks>
        /// If used in a class that retries a default number of times
        /// when an exception occurs, such as HttpMonitor, the failure
        /// of the conversion may result in extra communication.
        /// In such a case, set IgnoreException to true to suppress it.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        bool IgnoreException { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// Invokes the conversion.
        /// </summary>
        ///
        /// <param name="src">Stream object.</param>
        ///
        /// <returns>Converted object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        TValue Convert(Stream src);
    }

    #endregion

    #region ContentConverterBase<TValue>

    /* --------------------------------------------------------------------- */
    ///
    /// ContentConverterBase
    ///
    /// <summary>
    /// Represetns the base class of the IContentConverter implementations.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class ContentConverterBase<TValue> : IContentConverter<TValue>
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// Gets or sets a value indicating whether to ignore exceptions.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// Invokes the conversion.
        /// </summary>
        ///
        /// <param name="src">Stream object.</param>
        ///
        /// <returns>Converted object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public TValue Convert(Stream src)
        {
            try { return OnConvert(src); }
            catch (Exception err)
            {
                if (IgnoreException) GetType().LogWarn(err);
                else throw;
            }
            return default;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnConvert
        ///
        /// <summary>
        /// Invokes the conversion.
        /// </summary>
        ///
        /// <param name="src">Stream object.</param>
        ///
        /// <returns>Converted object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected abstract TValue OnConvert(Stream src);

        #endregion
    }

    #endregion

    #region ContentConverter<TValue>

    /* --------------------------------------------------------------------- */
    ///
    /// ContentConverter(TValue)
    ///
    /// <summary>
    /// Provides functionality to invoke the provided function as
    /// IContentConverter interface.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ContentConverter<TValue> : ContentConverterBase<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ContentConverter
        ///
        /// <summary>
        /// Initializes a new instance of the ContentConverter class with
        /// the specified function.
        /// </summary>
        ///
        /// <param name="func">Function object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public ContentConverter(Func<Stream, TValue> func) { _func = func; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnConvert
        ///
        /// <summary>
        /// Invokes the conversion.
        /// </summary>
        ///
        /// <param name="src">Stream object.</param>
        ///
        /// <returns>Converted object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        protected override TValue OnConvert(Stream src) => _func(src);

        #endregion

        #region Fields
        private readonly Func<Stream, TValue> _func;
        #endregion
    }

    #endregion
}
