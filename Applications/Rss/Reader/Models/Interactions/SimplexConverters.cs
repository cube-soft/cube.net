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
    /// TitleConverter
    ///
    /// <summary>
    /// メイン画面のタイトルに変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class TitleConverter : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TitleConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public TitleConverter() : base(e =>
            e is RssItem src ?
            $"{src.Title} - {AssemblyReader.Default.Title}" :
            $"{AssemblyReader.Default.Title}"
        ) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ArticleConverter
    ///
    /// <summary>
    /// RSS の内容を HTML 形式に変換するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ArticleConverter : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ArticleConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ArticleConverter() : base(
            e => e is RssItem src ?
            string.Format(
                Properties.Resources.Skeleton,
                Properties.Resources.SkeletonStyle,
                src.Link,
                src.Title,
                src.PublishTime,
                !string.IsNullOrEmpty(src.Content) ? src.Content : src.Summary
            ) :
            string.Empty
        )
        { }
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
    public class TextWrappingConverter : BooleanToGeneric<TextWrapping>
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
                src.LastChecked?.ToString("yyyy/MM/dd HH:mm:ss")
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
            base(e => e is RssFeed src && src.LastChecked != DateTime.MinValue) { }
    }
}
