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
using System.Collections.Generic;
using Cube.Net.Rss;
using NUnit.Framework;

namespace Cube.Net.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssCacheDictionaryTest
    ///
    /// <summary>
    /// RssCacheDictionary のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssCacheDictionaryTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Properties_Default
        ///
        /// <summary>
        /// 各種プロパティの初期値を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Properties_Default()
        {
            var src = new RssCacheDictionary();

            Assert.That(src.Count,        Is.EqualTo(0));
            Assert.That(src.IsReadOnly,   Is.False);
            Assert.That(src.Keys.Count,   Is.EqualTo(0));
            Assert.That(src.Values.Count, Is.EqualTo(0));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CopyTo_Throws
        ///
        /// <summary>
        /// サポートされていないメソッドを実行した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void CopyTo_Throws()
        {
            var src  = new RssCacheDictionary();
            var dest = new KeyValuePair<Uri, RssFeed>[1];

            Assert.That(
                () => src.CopyTo(dest, 0),
                Throws.TypeOf<NotSupportedException>()
            );
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Capacity
        ///
        /// <summary>
        /// Capacity のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Capacity()
        {
            var src = new RssCacheDictionary();
            Assert.That(src.Capacity, Is.EqualTo(100));

            src.Capacity = 10;
            Assert.That(src.Capacity, Is.EqualTo(10));

            src.Capacity = 1;
            Assert.That(src.Capacity, Is.EqualTo(1));

            src.Capacity = 0;
            Assert.That(src.Capacity, Is.EqualTo(1));
        }
    }
}
