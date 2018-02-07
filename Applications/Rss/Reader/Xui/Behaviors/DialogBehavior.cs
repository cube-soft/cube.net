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

namespace Cube.Xui.Behaviors
{
    /* --------------------------------------------------------------------- */
    ///
    /// DialogBehavior
    ///
    /// <summary>
    /// メッセージボックスを表示する Behavior です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class DialogBehavior : MessengerBehavior<DialogMessage>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Invoke
        ///
        /// <summary>
        /// 処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Invoke(DialogMessage e)
        {
            var dest = MessageBox.Show(e.Content, e.Title, e.Button, e.Image);
            e.Result = dest == MessageBoxResult.OK ||
                       dest == MessageBoxResult.Yes;
            e.Callback?.Invoke(e);
        }
    }
}
