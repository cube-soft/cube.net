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
        /// <param name="content">メッセージ内容</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string content) :
            this(content, AssemblyReader.Default.Title) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="content">メッセージ内容</param>
        /// <param name="title">タイトル</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string content, string title) :
            this(content, title, null) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="content">メッセージ内容</param>
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string content, Action<DialogMessage> callback) :
            this(content, AssemblyReader.Default.Title, callback) { }

        /* ----------------------------------------------------------------- */
        ///
        /// DialogMessage
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="content">メッセージ内容</param>
        /// <param name="title">タイトル</param>
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public DialogMessage(string content, string title, Action<DialogMessage> callback)
        {
            Content  = content;
            Title    = title;
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
        public Action<DialogMessage> Callback { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        ///
        /// <summary>
        /// メッセージ内容を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Content { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        ///
        /// <summary>
        /// タイトルを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Title { get; set; }

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

        /* ----------------------------------------------------------------- */
        ///
        /// Result
        ///
        /// <summary>
        /// メッセージボックス表示後のユーザの行動を示す値を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Result { get; set; } = true;

        #endregion
    }
}
