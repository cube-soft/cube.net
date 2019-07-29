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

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// MessageFactory
    ///
    /// <summary>
    /// 各種 Message オブジェクトを生成するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class MessageFactory
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Error
        ///
        /// <summary>
        /// Creates the DialogMessage object of the specified error
        /// message.
        /// </summary>
        ///
        /// <param name="err">Exception object.</param>
        /// <param name="message">Error message.</param>
        ///
        /// <returns>DialogMessage object.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DialogMessage Error(Exception err, string message) => new DialogMessage
        {
            Text    = $"{message} ({err.GetType().Name})",
            Title   = Properties.Resources.TitleError,
            Buttons = DialogButtons.Ok,
            Icon    = DialogIcon.Error,
            Value   = DialogStatus.Ok,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// RemoveWarning
        ///
        /// <summary>
        /// 削除時の警告メッセージを生成します。
        /// </summary>
        ///
        /// <param name="name">削除名</param>
        ///
        /// <returns>DialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DialogMessage RemoveWarning(string name) => new DialogMessage
        {
            Text    = string.Format(Properties.Resources.MessageRemove, name),
            Title   = Properties.Resources.TitleInformation,
            Buttons = DialogButtons.YesNo,
            Icon    = DialogIcon.Information,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// インポート用メッセージを生成します。
        /// </summary>
        ///
        /// <returns>OpenFileDialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static OpenFileMessage Import() => new OpenFileMessage
        {
            CheckPathExists = true,
            Multiselect     = false,
            Text            = Properties.Resources.MessageImport,
            Filter          = Properties.Resources.FilterOpml,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// Export
        ///
        /// <summary>
        /// エクスポート用メッセージを生成します。
        /// </summary>
        ///
        /// <returns>SaveFileDialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static SaveFileMessage Export() => new SaveFileMessage
        {
            CheckPathExists = false,
            OverwritePrompt = true,
            Text            = Properties.Resources.MessageExport,
            Filter          = Properties.Resources.FilterOpml,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// DataDirectory
        ///
        /// <summary>
        /// データディレクトリ選択用メッセージを生成します。
        /// </summary>
        ///
        /// <param name="src">選択パスの初期値</param>
        ///
        /// <returns>DirectoryDialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static OpenDirectoryMessage DataDirectory(string src) => new OpenDirectoryMessage
        {
            Value     = src,
            NewButton = true,
            Text      = Properties.Resources.MessageDataDirectory,
        };
    }
}
