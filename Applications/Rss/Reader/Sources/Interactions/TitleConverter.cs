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
namespace Cube.Net.Rss.Reader;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Cube.Reflection.Extensions;

/* ------------------------------------------------------------------------- */
///
/// TitleConverter
///
/// <summary>
/// Provides funtionality to convert to a main window title.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class TitleConverter : MarkupExtension, IMultiValueConverter
{
    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Convert
    ///
    /// <summary>
    /// Invokes the conversion.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var asm = typeof(TitleConverter).Assembly;
        var ss  = new System.Text.StringBuilder();
        if (values[0] is RssItem src) _ = ss.Append($"{src.Title} - ");
        _ = ss.Append(asm.GetTitle());
        if (values[1] is LockSetting x && x.IsReadOnly) _ = ss.Append($" ({Properties.Resources.MessageReadOnly})");
        return ss.ToString();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ConvertBack
    ///
    /// <summary>
    /// Invokes the reverse conversion. The method is not currently supported.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public object[] ConvertBack(object s, Type[] t, object p, CultureInfo c) =>
        throw new NotSupportedException();

    /* --------------------------------------------------------------------- */
    ///
    /// ProvideValue
    ///
    /// <summary>
    /// Returns the self object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    #endregion
}
