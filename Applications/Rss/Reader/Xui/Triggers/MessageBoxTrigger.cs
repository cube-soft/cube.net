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

namespace Cube.Xui.Triggers
{
    /* --------------------------------------------------------------------- */
    ///
    /// MessageBoxTrigger
    ///
    /// <summary>
    /// メッセージボックスを表示するための Trigger クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class MessageBoxTrigger : MessengerTrigger<MessageBox> { }

    /* --------------------------------------------------------------------- */
    ///
    /// MessageBoxAction
    ///
    /// <summary>
    /// メッセージボックスを表示するための Action クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class MessageBoxAction : TriggerAction<DependencyObject>
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
            if (parameter is MessageBox msg)
            {
                var result = System.Windows.MessageBox.Show(
                    msg.Text,
                    msg.Caption,
                    msg.Button,
                    msg.Image
                );
                msg.Callback?.Invoke(result);
            }
        }
    }
}
