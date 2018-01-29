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
using System.Collections.Generic;
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
        /// CheckPathExists
        /// 
        /// <summary>
        /// 指定されたファイルの存在チェックを実行するかどうかを示す値を
        /// 取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool CheckPathExists { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// InitialDirectory
        /// 
        /// <summary>
        /// ディレクトリの初期設定を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string InitialDirectory { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        /// 
        /// <summary>
        /// 選択されたファイルのパスを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string FileName { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Filter
        /// 
        /// <summary>
        /// フィルタを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Filter { get; set; } = "All Files (*.*)|*.*";

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
            base(callback)
        {
            CheckPathExists = true;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Multiselect
        /// 
        /// <summary>
        /// 複数選択可能かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Multiselect { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// FileNames
        /// 
        /// <summary>
        /// 選択されたファイルのパス一覧を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<string> FileNames { get; set; }

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
            base(callback)
        {
            CheckPathExists = false;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// OverwritePrompt
        /// 
        /// <summary>
        /// 上書き確認ダイアログを表示するかどうかを示す値を取得または
        /// 設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool OverwritePrompt { get; set; } = true;

        #endregion
    }
}
