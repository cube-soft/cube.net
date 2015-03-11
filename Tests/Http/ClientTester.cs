/* ------------------------------------------------------------------------- */
///
/// SettingsTester.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// This is distributed under the Microsoft Public License (Ms-PL).
/// See http://www.opensource.org/licenses/ms-pl.html
///
/* ------------------------------------------------------------------------- */
using System;
using NUnit.Framework;

namespace Cube.Net.Http.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.Http.Tests.ClientTester
    /// 
    /// <summary>
    /// Http.Client クラスをテストするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class ClientTester
    {
        /* ----------------------------------------------------------------- */
        ///
        /// TestOpenRead
        /// 
        /// <summary>
        /// 通信した結果をストリーム経由で読み込むテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestOpenRead()
        {
            Assert.DoesNotThrow(() => {
                var uri = new Uri("http://www.cube-soft.jp/");
                var client = new Cube.Net.Http.Client();
                client.UserAgent  = "Cube.Net.Http.ClientTester";
                client.EnableGzip = true;
                client.EnableETag = true;

                using (var stream = client.OpenRead(uri)) { }
            });
        }
    }
}
