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
using System.Windows;
using Cube.Net.Rss;
using Cube.Xui.Converters;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// LastCheckedToString
    ///
    /// <summary>
    /// RssFeed.LastChecked を文字列に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class LastCheckedToString : OneWayValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// LastCheckedToString
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LastCheckedToString() : base(e =>
            e is RssFeed src && src.LastChecked != DateTime.MinValue ?
            string.Format("{0} {1}",
                Properties.Resources.MessageLastChecked,
                src.LastChecked.ToString("yyyy/MM/dd HH:mm:ss")
            ) :
            string.Empty
        ) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// LastCheckedToVisibility
    ///
    /// <summary>
    /// RssFeed.LastChecked の値を基に Visibility を決定するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class LastCheckedToVisibility : OneWayValueConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// LastCheckedToVisibility
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public LastCheckedToVisibility() : base(e =>
            e is RssFeed src && src.LastChecked != DateTime.MinValue ?
            Visibility.Visible :
            Visibility.Collapsed
        ) { }
    }
}
