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
using Cube.Xui;
using System;
using System.Windows;

namespace Cube.Net.Rss.App.Reader
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
        public static DialogMessage Error(Exception err, string message) =>
            new DialogMessage(
                $"{message} ({err.GetType().Name})",
                Properties.Resources.TitleError
            ) {
                Button = MessageBoxButton.OK,
                Image  = MessageBoxImage.Error,
                Result = MessageBoxResult.OK,
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
        /// <param name="e">コールバック関数</param>
        ///
        /// <returns>DialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static DialogMessage RemoveWarning(string name, DialogCallback e) =>
            new DialogMessage(
                string.Format(Properties.Resources.MessageRemove, name),
                Properties.Resources.TitleInformation,
                e
            ) {
                Button = MessageBoxButton.YesNo,
                Image  = MessageBoxImage.Information,
            };

        /* ----------------------------------------------------------------- */
        ///
        /// Import
        ///
        /// <summary>
        /// インポート用メッセージを生成します。
        /// </summary>
        ///
        /// <param name="e">コールバック関数</param>
        ///
        /// <returns>OpenFileDialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static OpenFileMessage Import(OpenFileCallback e) =>
            new OpenFileMessage(e)
            {
                CheckPathExists = true,
                Multiselect     = false,
                Title           = Properties.Resources.MessageImport,
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
        /// <param name="e">コールバック関数</param>
        ///
        /// <returns>SaveFileDialogMessage オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static SaveFileMessage Export(SaveFileCallback e) =>
            new SaveFileMessage(e)
            {
                CheckPathExists = false,
                OverwritePrompt = true,
                Title           = Properties.Resources.MessageExport,
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
        public static OpenDirectoryMessage DataDirectory(string src) =>
            new OpenDirectoryMessage(null)
            {
                FileName  = src,
                NewButton = true,
                Title     = Properties.Resources.MessageDataDirectory,
            };
    }
}
