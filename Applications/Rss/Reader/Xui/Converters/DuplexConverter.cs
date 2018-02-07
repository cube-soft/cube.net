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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cube.Xui.Converters
{
    /* --------------------------------------------------------------------- */
    ///
    /// DuplexConverter
    ///
    /// <summary>
    /// 双方向の変換をサポートしている Converter クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class DuplexConverter : MarkupExtension, IValueConverter
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// DuplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="forward">変換関数</param>
        /// <param name="back">逆変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public DuplexConverter(Func<object, object> forward, Func<object, object> back) :
            this((x, _, __, ___) => forward(x), (x, _, __, ___) => back(x)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DuplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="forward">変換関数</param>
        /// <param name="back">逆変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public DuplexConverter(
            Func<object, object, object> forward,
            Func<object, object, object> back) :
            this(
                (x, _, y, __) => forward(x, y),
                (x, _, y, __) => back(x, y)
            ) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DuplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="forward">変換関数</param>
        /// <param name="back">逆変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public DuplexConverter(
            Func<object, Type, object, CultureInfo, object> forward,
            Func<object, Type, object, CultureInfo, object> back)
        {
            _forward = forward;
            _back    = back;
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// 変換処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object value, Type target, object parameter, CultureInfo culture) =>
            _forward(value, target, parameter, culture);

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        ///
        /// <summary>
        /// このメソッドはサポートされません。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture) =>
            _back(value, target, parameter, culture);

        /* ----------------------------------------------------------------- */
        ///
        /// ProvideValue
        ///
        /// <summary>
        /// 自身のオブジェクトを返します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        #endregion

        #region Fields
        private Func<object, Type, object, CultureInfo, object> _forward;
        private Func<object, Type, object, CultureInfo, object> _back;
        #endregion
    }
}
