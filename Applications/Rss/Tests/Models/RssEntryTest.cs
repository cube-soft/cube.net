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
using System.Linq;
using Cube.Net.App.Rss.Reader;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests.Models
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
            var dest = new RssEntry();
            Assert.That(dest.Expanded, Is.False);
            dest.Expanded = true;
            Assert.That(dest.Expanded, Is.False);
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
            Assert.That(new RssCategory().Count, Is.EqualTo(0));
    }
}
