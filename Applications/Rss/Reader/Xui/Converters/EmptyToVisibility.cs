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
using System.Windows;
using System.Windows.Data;

namespace Cube.Xui.Converters
{
    /* --------------------------------------------------------------------- */
    ///
    /// EmptyToVisibility
    ///
    /// <summary>
    /// 文字列から Visibility へ変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class EmptyToVisibility : IValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// 文字列から Visibility へ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public object Convert(object value, Type target, object parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value as string) ?
            Visibility.Visible :
            Visibility.Collapsed;

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
    }
}
