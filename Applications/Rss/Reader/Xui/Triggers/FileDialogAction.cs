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
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Win32;

namespace Cube.Xui.Triggers
{
    #region OpenFileDialog

    /* --------------------------------------------------------------------- */
    ///
    /// OpenFileDialogTrigger
    ///
    /// <summary>
    /// OpenFileDialog を表示するための Trigger クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class OpenFileDialogTrigger : MessengerTrigger<OpenFileDialogMessage> { }

    /* --------------------------------------------------------------------- */
    ///
    /// OpenFileDialogAction
    ///
    /// <summary>
    /// OpenFileDialog を表示するための TriggerAction です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class OpenFileDialogAction : TriggerAction<DependencyObject>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        /// 
        /// <summary>
        /// メッセージボックスを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
        {
            if (parameter is OpenFileDialogMessage msg)
            {
                var dialog = new OpenFileDialog
                {
                    CheckPathExists = msg.CheckPathExists,
                    Multiselect     = msg.Multiselect,
                };

                if (!string.IsNullOrEmpty(msg.Title)) dialog.Title = msg.Title;
                if (!string.IsNullOrEmpty(msg.FileName)) dialog.FileName = msg.FileName;
                if (!string.IsNullOrEmpty(msg.Filter)) dialog.Filter = msg.Filter;
                if (!string.IsNullOrEmpty(msg.InitialDirectory)) dialog.InitialDirectory = msg.InitialDirectory;

                var result = dialog.ShowDialog() ?? false;
                msg.Cancel = !result;
                if (!msg.Cancel)
                {
                    msg.FileName  = dialog.FileName;
                    msg.FileNames = dialog.FileNames;
                }
                msg.Callback?.Invoke(msg);
            }
        }
    }

    #endregion

    #region SaveFileDialog

    /* --------------------------------------------------------------------- */
    ///
    /// SaveFileDialogTrigger
    ///
    /// <summary>
    /// SaveFileDialog を表示するための Trigger クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SaveFileDialogTrigger : MessengerTrigger<SaveFileDialogMessage> { }

    /* --------------------------------------------------------------------- */
    ///
    /// SaveFileDialogAction
    ///
    /// <summary>
    /// SaveFileDialog を表示するための TriggerAction です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class SaveFileDialogAction : TriggerAction<DependencyObject>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        /// 
        /// <summary>
        /// メッセージボックスを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(object parameter)
        {
            if (parameter is SaveFileDialogMessage msg)
            {
                var dialog = new SaveFileDialog
                {
                    CheckPathExists = msg.CheckPathExists,
                    OverwritePrompt = msg.OverwritePrompt,
                };

                if (!string.IsNullOrEmpty(msg.Title)) dialog.Title = msg.Title;
                if (!string.IsNullOrEmpty(msg.FileName)) dialog.FileName = msg.FileName;
                if (!string.IsNullOrEmpty(msg.Filter)) dialog.Filter = msg.Filter;
                if (!string.IsNullOrEmpty(msg.InitialDirectory)) dialog.InitialDirectory = msg.InitialDirectory;

                var result = dialog.ShowDialog() ?? false;
                msg.Cancel = !result;
                if (!msg.Cancel) msg.FileName = dialog.FileName;
                msg.Callback?.Invoke(msg);
            }
        }
    }

    #endregion
}
