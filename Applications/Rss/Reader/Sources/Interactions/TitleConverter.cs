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
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// TitleConverter
    ///
    /// <summary>
    /// メイン画面のタイトルに変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class TitleConverter : MarkupExtension, IMultiValueConverter
    {
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
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var asm = Assembly.GetExecutingAssembly().GetReader();
            var ss  = new System.Text.StringBuilder();
            if (values[0] is RssItem src) ss.Append($"{src.Title} - ");
            ss.Append(asm.Title);
            if (values[1] is LockSettings x && x.IsReadOnly) ss.Append($" ({Properties.Resources.MessageReadOnly})");
            return ss.ToString();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        ///
        /// <summary>
        /// 逆変換を実行します。
        /// </summary>
        ///
        /// <remarks>
        /// このメソッドはサポートされていません。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public object[] ConvertBack(object s, Type[] t, object p, CultureInfo c) =>
            throw new NotSupportedException();

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
    }
}
