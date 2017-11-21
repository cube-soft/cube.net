﻿/* ------------------------------------------------------------------------- */
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
using System.Xml.Linq;
using Cube.Net.Rss;
using NUnit.Framework;

namespace Cube.Net.Tests.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssMonitorTest
    ///
    /// <summary>
    /// RssMonitor のテスト用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class RssVersionTest : FileHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetRssVersion
        ///
        /// <summary>
        /// RssVersion オブジェクトを取得するテストを実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        [TestCase("SampleRss091.xml",   ExpectedResult = RssVersion.Rss091)]
        [TestCase("SampleRss092.xml",   ExpectedResult = RssVersion.Rss092)]
        [TestCase("SampleRss10-01.xml", ExpectedResult = RssVersion.Rss10)]
        [TestCase("SampleRss20-01.xml", ExpectedResult = RssVersion.Rss20)]
        [TestCase("SampleAtom-01.xml",  ExpectedResult = RssVersion.Atom)]
        public RssVersion GetRssVersion(string filename)
        {
            var doc = XDocument.Load(Example(filename));
            return doc.Root.GetRssVersion();
        }
    }
}