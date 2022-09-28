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
namespace Cube.Net.Rss.Tests;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cube.FileSystem;
using Cube.Net.Rss.Reader;
using Cube.Reflection.Extensions;
using Cube.Tests;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// RssEntryTest
///
/// <summary>
/// Tests the RssEntry and its related classes.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class RssSubscriberTest : ResourceFixture
{
    #region Tests

    /* --------------------------------------------------------------------- */
    ///
    /// Load
    ///
    /// <summary>
    /// Tests the Load method.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Load()
    {
        using var src = Create();
        Assert.That(src.Capacity,       Is.EqualTo(100));
        Assert.That(src.CacheDirectory, Is.Null, nameof(src.CacheDirectory));
        Assert.That(src.UserAgent,      Is.Null, nameof(src.UserAgent));

        var count = 0;
        src.CollectionChanged += (s, e) => count++;
        src.Load();

        Assert.That(src.Categories.Count(), Is.EqualTo(2));
        Assert.That(count, Is.GreaterThan(3), nameof(src.CollectionChanged));

        var uri = new Uri("https://github.com/blog.atom");
        Assert.That(src.Entries.Count(),        Is.EqualTo(2));
        Assert.That(src.Entries.First().Title,  Is.EqualTo("The GitHub Blog"));
        Assert.That(src.Entries.First().Uri,    Is.EqualTo(uri));
        Assert.That(src.Entries.First().Parent, Is.Null);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Load_NotFound
    ///
    /// <summary>
    /// Tests the Load method with a nonexistent JSON file.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Load_NotFound()
    {
        using var src = new RssSubscriber(Dispatcher.Vanilla);
        src.FileName = GetSource("NotFound.json");
        src.Load();
        Assert.That(src.Count, Is.EqualTo(0));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Backup_Delete
    ///
    /// <summary>
    /// Tests to delete backup files that exceed the specified number.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Backup_Delete()
    {
        var dir  = Get(nameof(Backup_Delete), "Backup");
        var open = Io.Combine(dir, "20010101.json");

        for (var d = new DateTime(2001, 1, 1); d.Month < 2; d = d.AddDays(1))
        {
            var backup = Io.Combine(dir, $"{d:yyyyMMdd}.json");
            Io.Copy(GetSource("Sample.json"), backup, true);
        }

        using (Io.Open(open))
        using (var src = Create())
        {
            src.Load();
            Task.Delay(200).Wait();
        }

        Assert.That(Io.GetFiles(dir).Count(), Is.EqualTo(31));
        Assert.That(Io.Exists(open), Is.True); // delete failed
        Assert.That(Io.Exists(Io.Combine(dir, "20010102.json")), Is.False);
        Assert.That(Io.Exists(Io.Combine(dir, "20010103.json")), Is.True);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Find
    ///
    /// <summary>
    /// Tests the Find method.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Find()
    {
        using var src = Create();
        src.Load();

        Assert.That(src.Find(new Uri("https://github.com/blog.atom")), Is.Not.Null);
        Assert.That(src.Find(new Uri("http://www.example.com/")), Is.Null);
        Assert.That(src.Find(default), Is.Null);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Update_Frequency
    ///
    /// <summary>
    /// Tests the Update method with the specified RssCheckFrequency value.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestCase(RssCheckFrequency.Auto)]
    [TestCase(RssCheckFrequency.High)]
    [TestCase(RssCheckFrequency.Low)]
    [TestCase(RssCheckFrequency.None)]
    public void Update_Frequency(RssCheckFrequency value)
    {
        var uri = new Uri("https://clown.cube-soft.jp/feed");

        using var src = Create();
        src.Load();
        var dest = src.Find(uri);
        src.Received += (s, e) =>
        {
            Assert.That(e.Value.Uri, Is.EqualTo(uri));
            dest.Count = e.Value.UnreadItems.Count();
        };
        dest.Frequency = value;
        dest.Count = 0; // hack for tests.
        src.Reschedule(dest);

        var asm = typeof(RssSubscriberTest).Assembly;
        src.UserAgent = $"{asm.GetProduct()}/{asm.GetVersion()}";

        Assert.That(dest.Count, Is.EqualTo(0));
        src.Update(dest);
        Assert.That(Wait.For(() => dest.Count > 0), "Timeout");
        Assert.That(dest.Count, Is.AtLeast(1));
    }

    #endregion

    #region Others

    /* --------------------------------------------------------------------- */
    ///
    /// Create
    ///
    /// <summary>
    /// Creates a new instance of the RssSubscriber class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private RssSubscriber Create([CallerMemberName] string name = null)
    {
        Copy(name);
        return new RssSubscriber(Dispatcher.Vanilla) { FileName = FeedsPath(name) };
    }

    #endregion
}
