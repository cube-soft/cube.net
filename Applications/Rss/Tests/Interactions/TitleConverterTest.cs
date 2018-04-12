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
using Cube.Net.App.Rss.Reader;
using Cube.Net.Rss;
using NUnit.Framework;
using System.Globalization;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// TitleConverterTest
    ///
    /// <summary>
    /// タイトル変換のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class TitleConverterTest
    {
        #region TitleConverter

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Title
        ///
        /// <summary>
        /// タイトルの変換テストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Title() => Assert.That(
            Convert(new RssItem { Title = "TitleTest" }, new LockSettings()),
            Is.EqualTo("TitleTest - Cube.Core")
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Convert_Title_Null
        ///
        /// <summary>
        /// タイトルの変換時に null が指定された時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Convert_Title_Null() => Assert.That(
            Convert(null, new LockSettings()),
            Is.EqualTo("Cube.Core")
        );

        #endregion

        #region Helper methods

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        ///
        /// <summary>
        /// Convert メソッドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string Convert(RssItem src, LockSettings settings) =>
            new TitleConverter().Convert(
                new object[] { src, settings },
                typeof(string),
                null,
                CultureInfo.CurrentCulture
            ) as string;

        #endregion
    }
}
