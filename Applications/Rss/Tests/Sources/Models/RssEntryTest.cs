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
using NUnit.Framework;
using System;
using System.Threading;

namespace Cube.Net.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntryTest
    ///
    /// <summary>
    /// RssEntry および RssCategory のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssEntryTest
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Entry_Expanded
        ///
        /// <summary>
        /// RssEntry.Expanded プロパティの挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Entry_Expanded()
        {
            var dest = new RssEntry(new SynchronizationContext());
            Assert.That(dest.Context,  Is.Not.Null);
            Assert.That(dest.Expanded, Is.False);
            dest.Expanded = true;
            Assert.That(dest.Expanded, Is.False);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Entry_Update_Error
        ///
        /// <summary>
        /// Error プロパティを持つ RssFeed オブジェクトで Update を
        /// 実行した時の挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Entry_Update_Error()
        {
            var dest = new RssEntry(new SynchronizationContext())
            {
                Uri       = new Uri("https://blog.cube-soft.jp/?feed=rss2"),
                Link      = new Uri("https://blog.cube-soft.jp/"),
                Count     = 0,
                Frequency = RssCheckFrequency.High,
                Title     = "CubeSoft Blog",
            };

            var src = new RssFeed
            {
                Uri         = new Uri("https://blog.cube-soft.jp/?feed=rss2"),
                Error       = new ArgumentException("Test"),
                LastChecked = DateTime.Now,
            };

            dest.Update(src);

            Assert.That(dest.LastChecked.HasValue, Is.True);
            Assert.That(src.Title, Is.EqualTo(dest.Title));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Entry_Expanded
        ///
        /// <summary>
        /// RssEntry.Expanded プロパティの挙動を確認します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void Category_Count() =>
            Assert.That(new RssCategory(new SynchronizationContext()).Count, Is.EqualTo(0));
    }
}
