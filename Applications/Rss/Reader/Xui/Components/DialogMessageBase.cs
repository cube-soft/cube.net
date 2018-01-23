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
using System.Windows;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// DialogMessageBase
    ///
    /// <summary>
    /// 各種ダイアログに表示する情報を保持するための基底クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class DialogMessageBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessageBase
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessageBase(Action<MessageBoxResult> callback)
        {
            Callback = callback;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Text
        /// 
        /// <summary>
        /// メッセージ内容を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Text { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Caption
        /// 
        /// <summary>
        /// タイトルキャプションを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Caption { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Callback
        /// 
        /// <summary>
        /// コールバック用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Action<MessageBoxResult> Callback { get; }

        #endregion
    }
}
