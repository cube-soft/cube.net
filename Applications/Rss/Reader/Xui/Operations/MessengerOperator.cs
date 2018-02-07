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

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// MessengerOperator
    ///
    /// <summary>
    /// Messenger の拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class MessengerOperator
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Send
        ///
        /// <summary>
        /// 既定のメッセージを送信します。
        /// </summary>
        ///
        /// <param name="src">Messenger オブジェクト</param>
        ///
        /// <remarks>
        /// Messenger は型で判別しているため、不要な場合でもメッセージ
        /// オブジェクトを送信する必要がある。Send 拡張メソッドは、
        /// 型を指定すると既定のオブジェクトを送信する。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static void Send<T>(this IMessenger src) where T : new() => src.Send(new T());
    }
}
