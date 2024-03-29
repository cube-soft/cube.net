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
namespace Cube.Net.Tests;

using System.Net.NetworkInformation;
using NUnit.Framework;

/* ------------------------------------------------------------------------- */
///
/// NetworkTest
///
/// <summary>
/// Tests the Network class.
/// </summary>
///
/* ------------------------------------------------------------------------- */
[TestFixture]
class NetworkTest
{
    /* --------------------------------------------------------------------- */
    ///
    /// Test
    ///
    /// <summary>
    /// Checks the properties of the Network class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Test]
    public void Test()
    {
        Assert.That(Network.Status,    Is.EqualTo(OperationalStatus.Up));
        Assert.That(Network.Available, Is.True);
    }
}
