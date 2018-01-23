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
    /// UpdateSources
    ///
    /// <summary>
    /// UpdateSources を実行する事を表すメッセージです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class UpdateSourcesMessage { }

    /* --------------------------------------------------------------------- */
    ///
    /// UpdateSourcesTrigger
    ///
    /// <summary>
    /// Messenger オブジェクト経由で UpdateSources を実行するための
    /// Trigger クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class UpdateSourcesTrigger : MessengerTrigger<UpdateSourcesMessage> { }

    /* --------------------------------------------------------------------- */
    ///
    /// UpdateSourcesAction
    ///
    /// <summary>
    /// UpdateSources を実行する TriggerAction です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class UpdateSourcesAction : TriggerAction<DependencyObject>
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
        protected override void Invoke(object notused)
        {
            if (AssociatedObject is Window w) w.BindingGroup.UpdateSources();
        }
    }
}
