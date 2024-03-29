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
namespace Cube.Net.Tests.Http;

using System;
using System.Net;
using Cube.Net.Http;
using Cube.Reflection.Extensions;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// HttpHandlerTest
///
/// <summary>
/// Tests the HttpHandler and inherited classes.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class HttpHandlerTest
{
    #region Tests

    /* --------------------------------------------------------------------- */
    ///
    /// GetProperties
    ///
    /// <summary>
    /// Confirms the default values of properties.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void GetProperties()
    {
        var src = new HttpHandler();
        Assert.That(src.UserAgent,       Is.Null, nameof(src.UserAgent));
        Assert.That(src.ConnectionClose, Is.True, nameof(src.ConnectionClose));
        Assert.That(src.UseEntityTag,    Is.True, nameof(src.UseEntityTag));
        Assert.That(src.EntityTag,       Is.Null, nameof(src.EntityTag));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GetAsync_ConnectionClose
    ///
    /// <summary>
    /// Test the ConnectionClose property.
    /// </summary>
    ///
    /// <remarks>
    /// NuGet に公開されている System.Net.Http for .NET 3.5 には
    /// ConnectionClose に関するバグが存在する模様。
    /// 現在は、AddRequestHeaders で Connection プロパティに値を
    /// 設定しないように修正したものを利用している。
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void GetAsync_ConnectionClose()
    {
        var uri = new Uri("http://www.cube-soft.jp/");
        var h   = new HttpHandler
        {
            UserAgent       = GetUserAgent(),
            ConnectionClose = true
        };

        using var http = HttpClientFactory.Create(h);
        using var response = http.GetAsync(uri).Result;

        Assert.That(response.Headers.Connection.Contains("Close"));
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GetAsync_EntityTag
    ///
    /// <summary>
    /// Tests the EntityTag (ETag) property.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void GetAsync_EntityTag()
    {
        var uri = new Uri("https://www.cube-soft.jp/favicon.ico");
        var h   = new HttpHandler { UserAgent = GetUserAgent() };

        using var http = HttpClientFactory.Create(h);
        using (var r = http.GetAsync(uri).Result)
        {
            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        var tag = h.EntityTag;
        using (var r = http.GetAsync(uri).Result)
        {
            Assert.That(h.EntityTag,  Is.EqualTo(tag));
            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
        }
    }

    #endregion

    #region Others

    /* --------------------------------------------------------------------- */
    ///
    /// GetUserAgent
    ///
    /// <summary>
    /// Gets the user agent from the current environment.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private string GetUserAgent()
    {
        var asm = typeof(HttpHandlerTest).Assembly;
        var app = $"{asm.GetProduct()}/{asm.GetVersion()}";
        var win = Environment.OSVersion.VersionString;
        var net = $".NET {Environment.Version}";
        return $"{app} ({win}; {net})";
    }

    #endregion
}
