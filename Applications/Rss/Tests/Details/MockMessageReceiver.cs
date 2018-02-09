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
using GalaSoft.MvvmLight.Messaging;
using Cube.Xui;
using NUnit.Framework;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// MockMessagReceiver
    ///
    /// <summary>
    /// ViewModel から送信されるメッセージを処理するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    class MockMessagReceiver
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MockMessagReceiver
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MockMessagReceiver(IMessenger src)
        {
            src.Register<DialogMessage>(this, DoDialogMessage);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// DoDialogMessage
        ///
        /// <summary>
        /// DialogMessage オブジェクトを受信して処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void DoDialogMessage(DialogMessage e)
        {
            Assert.That(e.Content, Is.Not.Null.And.Not.Empty);
            Assert.That(e.Title,   Is.Not.Null.And.Not.Empty);

            e.Result = true;
            e.Callback(e);
        }

        #endregion
    }
}
