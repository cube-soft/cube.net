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
    /// SimplexConverter
    ///
    /// <summary>
    /// 単方向の変換のみをサポートしている Converter クラスです。
    /// ConvertBack は常に NotSupportedException が送出されます。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class SimplexConverter : MarkupExtension, IValueConverter
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SimplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="func">変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public SimplexConverter(Func<object, object> func) :
            this((x, _, __, ___) => func(x)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SimplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="func">変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public SimplexConverter(Func<object, object, object> func) :
            this((x, _, y, __) => func(x, y)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SimplexConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="func">変換関数</param>
        ///
        /* ----------------------------------------------------------------- */
        public SimplexConverter(Func<object, Type, object, CultureInfo, object> func)
        {
            _func = func;
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
            _func(value, target, parameter, culture);

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        ///
        /// <summary>
        /// このメソッドはサポートされません。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

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
        private Func<object, Type, object, CultureInfo, object> _func;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ValueToString
    ///
    /// <summary>
    /// 指定されたオブジェクトを文字列に変換するためのクラスです。
    /// 変換には object.ToString() メソッドを利用します。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ValueToString : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ValueToString
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ValueToString() : base (e => e?.ToString() ?? string.Empty) { }
    }
}
