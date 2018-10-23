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
using Cube.Net.Rss.App.Reader;
using Cube.Xui.Converters;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Windows;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// SimplexConverterTest
    ///
    /// <summary>
    /// 単方向変換のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class SimplexConverterTest
    {
        #region Tests

        #region ExpandConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Converter_ExpandSymbol
        ///
        /// <summary>
        /// TreeView に表示するシンボルに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true,  ExpectedResult = 59650)]
        [TestCase(false, ExpectedResult = 59649)]
        public int Converter_ExpandSymbol(bool src) =>
            Convert<string>(new ExpandConverter(), src)[0];

        #endregion

        #region FrequencyConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_FrequencyMessage
        ///
        /// <summary>
        /// RssCheckFrequency に対応するメッセージに変換するテストを
        /// 実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(RssCheckFrequency.Auto, ExpectedResult = "自動")]
        [TestCase(RssCheckFrequency.High, ExpectedResult = "高頻度")]
        [TestCase(RssCheckFrequency.Low,  ExpectedResult = "低頻度")]
        [TestCase(RssCheckFrequency.None, ExpectedResult = "チェックしない")]
        public string Convert_FrequencyMessage(RssCheckFrequency src) =>
            Convert<string>(new FrequencyConverter(), src);

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_InvalidFrequency
        ///
        /// <summary>
        /// FrequencyConverter に無効な値を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_InvalidFrequency() => Assert.That(
            Convert<string>(new FrequencyConverter(), 256),
            Is.Empty
        );

        #endregion

        #region TextWrappingConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_TextWrapping
        ///
        /// <summary>
        /// TextWrapping に変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(true,  ExpectedResult = TextWrapping.NoWrap)]
        [TestCase(false, ExpectedResult = TextWrapping.Wrap)]
        public TextWrapping Convert_TextWrapping(bool src) =>
            Convert<TextWrapping>(new TextWrappingConverter(), src);

        #endregion

        #region ItemStatusToString

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_ItemStatus
        ///
        /// <summary>
        /// 未読を表すシンボルに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(RssItemStatus.Read,          ExpectedResult = 0)]
        [TestCase(RssItemStatus.Uninitialized, ExpectedResult = 2)]
        [TestCase(RssItemStatus.Unread,        ExpectedResult = 2)]
        public int Convert_ItemStatus(RssItemStatus src) =>
            Convert<string>(new ItemStatusToString(), src).Length;

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_InvalidItemStatus
        ///
        /// <summary>
        /// ItemStatusToString に無効な値を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_InvalidItemStatus() => Assert.That(
            Convert<string>(new ItemStatusToString(), 128),
            Is.Empty
        );

        #endregion

        #region LastCheckedToString

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_DateTime
        ///
        /// <summary>
        /// 日時を文字列表記に変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_DateTime() => Assert.That(
            Convert<string>(new LastCheckedToString(), new RssFeed
            {
                LastChecked = new DateTime(2018, 2, 10, 19, 25, 3),
            }),
            Is.EqualTo("最終チェック 2018/02/10 19:25:03")
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_DateTime_Default
        ///
        /// <summary>
        /// LastCheckedToString にデフォルトの RssFeed オブジェクトを
        /// 指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_DateTime_Default() => Assert.That(
            Convert<string>(new LastCheckedToString(), new RssFeed()),
            Is.Empty
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_DateTime_Null
        ///
        /// <summary>
        /// LastCheckedToVisibility に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_DateTime_Null() => Assert.That(
            Convert<string>(new LastCheckedToString(), null),
            Is.Empty
        );

        #endregion

        #region LastCheckedToVisibility

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Visibility
        ///
        /// <summary>
        /// 表示モードに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Visibility() => Assert.That(
            Convert<Visibility>(new LastCheckedToVisibility(), new RssFeed
            {
                LastChecked = new DateTime(2018, 2, 10, 19, 25, 3),
            }),
            Is.EqualTo(Visibility.Visible)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Visibility_Default
        ///
        /// <summary>
        /// LastCheckedToVisibility にデフォルトの RssFeed オブジェクトを
        /// 指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Visibility_Default() => Assert.That(
            Convert<Visibility>(new LastCheckedToVisibility(), new RssFeed()),
            Is.EqualTo(Visibility.Collapsed)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Visibility_Null
        ///
        /// <summary>
        /// LastCheckedToVisibility に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Visibility_Null() => Assert.That(
            Convert<Visibility>(new LastCheckedToVisibility(), null),
            Is.EqualTo(Visibility.Collapsed)
        );

        #endregion

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// Convert メソッドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private T Convert<T>(SimplexConverter src, object value) =>
            (T)src.Convert(value, typeof(T), null, CultureInfo.CurrentCulture);

        #endregion
    }
}
