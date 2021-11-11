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
using System.Linq;
using Cube.Collections;
using Cube.FileSystem;
using Cube.Net.Rss;
using Cube.Tests;
using NUnit.Framework;

namespace Cube.Net.Tests.Rss
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
    class RssCacheDictionaryTest : FileFixture
    {
        #region Tests

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
            src.Clear();

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

        /* ----------------------------------------------------------------- */
        ///
        /// Contains
        ///
        /// <summary>
        /// Contains および ContainsKey の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Contains()
        {
            var feeds = Create();
            var cache = Get(nameof(Contains), "Cache");

            using var src = new RssCacheDictionary { Directory = cache };
            foreach (var e in feeds) src.Add(e);

            var q = feeds[0];
            Assert.That(src.ContainsKey(q.Uri), Is.True);
            Assert.That(src.Contains(new KeyValuePair<Uri, RssFeed>(q.Uri, q)), Is.True);
            Assert.That(src.Contains(new KeyValuePair<Uri, RssFeed>(q.Uri, null)), Is.False);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stash
        ///
        /// <summary>
        /// Stash のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Stash()
        {
            var feeds = Create();
            var cache = Get(nameof(Stash), "Cache");

            using (var src = new RssCacheDictionary { Directory = cache })
            {
                src.Capacity = 3;
                foreach (var feed in feeds) src.Add(feed);
                Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(3));
            }

            Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(6));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Remove
        ///
        /// <summary>
        /// Remove のテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Remove()
        {
            var feeds = Create();
            var cache = Get(nameof(Remove), "Cache");

            using var src = new RssCacheDictionary { Directory = cache };
            src.Capacity = 3;
            foreach (var e in feeds) src.Add(e);
            Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(3));

            Assert.That(src.Remove(feeds[0].Uri), Is.True);
            Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(3));

            Assert.That(src.Remove(feeds[1].Uri, true), Is.True);
            Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(2));

            Assert.That(src.Remove(new KeyValuePair<Uri, RssFeed>(feeds[2].Uri, feeds[2])));
            Assert.That(Io.GetFiles(cache).Count(), Is.EqualTo(2));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Delete_Null
        ///
        /// <summary>
        /// Delete の引数に null を指定した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Delete_Null() => Assert.DoesNotThrow(() =>
        {
            var cache = Get(nameof(Delete_Null), "Cache");
            using var src = new RssCacheDictionary { Directory = cache };
            src.Delete(null);
        });

        #endregion

        #region Others

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// テスト用データを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private List<RssFeed> Create() => new()
        {
            new() { Uri = new("http://www.example.com/"),  Title = "Ex1", LastChecked = DateTime.Now },
            new() { Uri = new("http://www.example.jp"),    Title = "Ex2", LastChecked = DateTime.Now },
            new() { Uri = new("http://www.example.net"),   Title = "Ex3", LastChecked = DateTime.Now },
            new() { Uri = new("http://www.example.org/"),  Title = "Ex4", LastChecked = DateTime.Now },
            new() { Uri = new("http://www.example.co.jp"), Title = "Ex5", LastChecked = DateTime.Now },
            new() { Uri = new("http://www.example.ne.jp"), Title = "Ex6", LastChecked = DateTime.Now },
        };

        #endregion
    }
}
