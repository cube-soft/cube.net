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
using Cube.Xui.Converters;
using System.Windows;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// ExpandConverter
    ///
    /// <summary>
    /// カテゴリの拡張アイコンに変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ExpandConverter : BooleanToValue<string>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// FrequencyConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ExpandConverter() : base(
            System.Convert.ToChar(59650).ToString(),
            System.Convert.ToChar(59649).ToString()
        ) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// FrequencyConverter
    ///
    /// <summary>
    /// Frequency オブジェクトを文字列に変換するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class FrequencyConverter : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// FrequencyConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public FrequencyConverter() :
            base(e => e is RssCheckFrequency f ? f.ToMessage() : string.Empty) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// TextWrappingConverter
    ///
    /// <summary>
    /// LightMode の値を TextWrapping に変換するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class TextWrappingConverter : BooleanToValue<TextWrapping>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TextWrappingConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TextWrappingConverter() :
            base(TextWrapping.NoWrap, TextWrapping.Wrap) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ItemStatusToString
    ///
    /// <summary>
    /// RssItem.Status を文字列に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ItemStatusToString : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ItemStatusToString
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ItemStatusToString() : base(e =>
            e is RssItemStatus status && status != RssItemStatus.Read ?
            $"{System.Convert.ToChar(11044)} " :
            string.Empty
        ) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// LastCheckedToString
    ///
    /// <summary>
    /// RssFeed.LastChecked を文字列に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class LastCheckedToString : SimplexConverter
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
            e is RssFeed src && src.LastChecked.HasValue ?
            string.Format("{0} {1}",
                Properties.Resources.MessageLastChecked,
                src.LastChecked.Value.ToString("yyyy/MM/dd HH:mm:ss")
            ) :
            string.Empty
        )
        { }
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
    public class LastCheckedToVisibility : BooleanToVisibility
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
        public LastCheckedToVisibility() :
            base(e => e is RssFeed src && src.LastChecked.HasValue) { }
    }
}
