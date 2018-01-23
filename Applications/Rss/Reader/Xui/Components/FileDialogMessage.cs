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
    /// FileDialogMessage
    ///
    /// <summary>
    /// ファイルダイアログに表示する情報を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class FileDialogMessage : DialogMessageBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// FileDialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public FileDialogMessage(Action<MessageBoxResult> callback) :
            base(callback) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Filter
        /// 
        /// <summary>
        /// フィルタを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Filter { get; set; } = "すべてのファイル (*.*)|*.*";

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OpenFileDialogMessage
    ///
    /// <summary>
    /// OpenFileDialog に表示する情報を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class OpenFileDialogMessage : FileDialogMessage
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// OpenFileDialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public OpenFileDialogMessage(Action<MessageBoxResult> callback) :
            base(callback) { }

        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// SaveFileDialogMessage
    ///
    /// <summary>
    /// SaveFileDialog に表示する情報を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SaveFileDialogMessage : FileDialogMessage
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SaveFileDialogMessage
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="callback">コールバック用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public SaveFileDialogMessage(Action<MessageBoxResult> callback) :
            base(callback) { }

        #endregion
    }
}
