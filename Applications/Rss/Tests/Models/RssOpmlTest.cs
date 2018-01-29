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

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssOpmlTest
    ///
    /// <summary>
    /// RssOpml のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssOpmlTest : FileHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// LoadOpml
        /// 
        /// <summary>
        /// OPML ファイルをロードするテストを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void LoadOpml()
        {
            var dest = RssOpml.Load(Example("Sample.opml"), IO).ToList();
            Assert.That(dest.Count, Is.EqualTo(1));

            var root = dest[0] as RssCategory;
            Assert.That(root.Title, Is.EqualTo("Subscriptions"));
            Assert.That(root.Children.Count, Is.EqualTo(3));

            var s0 = root.Children[0] as RssEntry;
            Assert.That(s0.Parent, Is.EqualTo(root), s0.Title);
            Assert.That(s0.Title,  Is.EqualTo("The GitHub Blog"));
            Assert.That(s0.Uri,    Is.EqualTo(new Uri("https://github.com/blog.atom")));
            Assert.That(s0.Link,   Is.EqualTo(new Uri("https://github.com/blog")));

            var s1 = root.Children[1] as RssCategory;
            Assert.That(s1.Parent, Is.EqualTo(root), s1.Title);
            Assert.That(s1.Title,  Is.EqualTo("キューブ・ソフト"));
            Assert.That(s1.Children.Count, Is.EqualTo(1));

            var s10 = s1.Children[0] as RssEntry;
            Assert.That(s10.Parent, Is.EqualTo(s1), s10.Title);
            Assert.That(s10.Title,  Is.EqualTo("CubeSoft Blog"));
            Assert.That(s10.Uri,    Is.EqualTo(new Uri("https://blog.cube-soft.jp/?feed=rss2")));
            Assert.That(s10.Link,   Is.EqualTo(new Uri("https://blog.cube-soft.jp/")));

            var s2 = root.Children[2] as RssCategory;
            Assert.That(s2.Parent, Is.EqualTo(root), s2.Title);
            Assert.That(s2.Title,  Is.EqualTo("Microsoft"));
            Assert.That(s2.Children.Count, Is.EqualTo(3));

            var s20 = s2.Children[0] as RssEntry;
            Assert.That(s20.Parent, Is.EqualTo(s2), s20.Title);
            Assert.That(s20.Title,  Is.EqualTo("The Official Microsoft Blog"));
            Assert.That(s20.Uri,    Is.EqualTo(new Uri("https://blogs.microsoft.com/feed/")));
            Assert.That(s20.Link,   Is.EqualTo(new Uri("https://blogs.microsoft.com/")));

            var s21 = s2.Children[1] as RssEntry;
            Assert.That(s21.Parent, Is.EqualTo(s2), s21.Title);
            Assert.That(s21.Title,  Is.EqualTo("Windows Blog"));
            Assert.That(s21.Uri,    Is.EqualTo(new Uri("https://blogs.windows.com/feed/")));
            Assert.That(s21.Link,   Is.EqualTo(new Uri("https://blogs.windows.com/")));

            var s22 = s2.Children[2] as RssEntry;
            Assert.That(s22.Parent, Is.EqualTo(s2), s22.Title);
            Assert.That(s22.Title,  Is.EqualTo(".NET Blog"));
            Assert.That(s22.Uri,    Is.EqualTo(new Uri("https://blogs.msdn.microsoft.com/dotnet/feed/")));
            Assert.That(s22.Link,   Is.EqualTo(new Uri("https://blogs.msdn.microsoft.com/dotnet/")));
        }
    }
}
