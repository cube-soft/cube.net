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
    /// DialogMessage
    ///
    /// <summary>
    /// メッセージボックスに表示する情報を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class DialogMessage
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="text">メッセージ内容</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string text)
            : this(text, AssemblyReader.Default.Title) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="text">メッセージ内容</param>
        /// <param name="caption">タイトルキャプション</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string text, string caption)
            : this(text, caption, null) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="text">メッセージ内容</param>
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string text, Action<MessageBoxResult> callback)
            : this(text, AssemblyReader.Default.Title, callback) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="text">メッセージ内容</param>
        /// <param name="caption">タイトルキャプション</param>
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string text, string caption, Action<MessageBoxResult> callback)
        {
            Text     = text;
            Caption  = caption;
            Callback = callback;
        }

        #endregion

        #region Properties

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
        /// Button
        /// 
        /// <summary>
        /// 表示ボタンを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;

        /* ----------------------------------------------------------------- */
        ///
        /// Image
        /// 
        /// <summary>
        /// 表示イメージを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MessageBoxImage Image { get; set; } = MessageBoxImage.Error;

        #endregion
    }
}
