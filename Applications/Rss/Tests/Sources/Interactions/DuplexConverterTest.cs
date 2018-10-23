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
    /// DuplexConverterTest
    ///
    /// <summary>
    /// 双方向変換のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class DuplexConverterTest
    {
        #region Tests

        #region MinuteConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Minute
        ///
        /// <summary>
        /// TimeSpan オブジェクトを分単位に変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Minute() => Assert.That(
            Convert<int>(new MinuteConverter(), TimeSpan.FromSeconds(1234)),
            Is.EqualTo(20)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Minute_Null
        ///
        /// <summary>
        /// MinuteConverter に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Minute_Null() => Assert.That(
            Convert<int>(new MinuteConverter(), null),
            Is.EqualTo(0)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack_Minute
        ///
        /// <summary>
        /// TimeSpan オブジェクトに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ConvertBack_Minute() => Assert.That(
            ConvertBack<TimeSpan?>(new MinuteConverter(), "34"),
            Is.EqualTo(TimeSpan.FromMinutes(34))
        );

        #endregion

        #region HourConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Hour
        ///
        /// <summary>
        /// TimeSpan オブジェクトを時間単位に変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Hour() => Assert.That(
            Convert<int>(new HourConverter(), TimeSpan.FromSeconds(5432)),
            Is.EqualTo(1)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Hour_Null
        ///
        /// <summary>
        /// HourConverter に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Hour_Null() => Assert.That(
            Convert<int>(new HourConverter(), null),
            Is.EqualTo(0)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack_Hour
        ///
        /// <summary>
        /// TimeSpan オブジェクトに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ConvertBack_Hour() => Assert.That(
            ConvertBack<TimeSpan?>(new HourConverter(), "3"),
            Is.EqualTo(TimeSpan.FromHours(3))
        );

        #endregion

        #region ColumnConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Column
        ///
        /// <summary>
        /// 整数値を GridLength オブジェクトに変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Column() => Assert.That(
            (int)Convert<GridLength>(new ColumnConverter(), 300).Value,
            Is.EqualTo(300)
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Column_Null
        ///
        /// <summary>
        /// ColumnConverter に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Column_Null() => Assert.That(
            () => Convert<GridLength>(new ColumnConverter(), null),
            Throws.TypeOf<NotSupportedException>()
        );

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack_Column
        ///
        /// <summary>
        /// GridLength オブジェクトを整数値に変換するテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void ConvertBack_Column() => Assert.That(
            ConvertBack<int>(new ColumnConverter(), new GridLength(200)),
            Is.EqualTo(200)
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
        private T Convert<T>(DuplexConverter src, object value) =>
            (T)src.Convert(value, typeof(T), null, CultureInfo.CurrentCulture);

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertBack
        ///
        /// <summary>
        /// ConvertBack メソッドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private T ConvertBack<T>(DuplexConverter src, object value) =>
            (T)src.ConvertBack(value, typeof(T), null, CultureInfo.CurrentCulture);

        #endregion
    }
}
