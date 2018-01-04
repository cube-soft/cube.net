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
using Cube.Xui.Triggers;

namespace Cube.Net.App.Rss.Reader
{
    #region RegisterWindow

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterTrigger
    ///
    /// <summary>
    /// RegisterViewModel を介した MesengerTrigger です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RegisterTrigger : MessengerTrigger<RegisterViewModel> { }

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterWindowAction
    ///
    /// <summary>
    /// RegisterWindow を表示するための Action クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RegisterWindowAction : ShowDialogAction<RegisterWindow> { }

    #endregion

    #region PropertyWindow

    /* --------------------------------------------------------------------- */
    ///
    /// PropertyTrigger
    ///
    /// <summary>
    /// PropertyViewModel を介した MesengerTrigger です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class PropertyTrigger : MessengerTrigger<PropertyViewModel> { }

    /* --------------------------------------------------------------------- */
    ///
    /// PropertyWindowAction
    ///
    /// <summary>
    /// PropertyWindow を表示するための Action クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class PropertyWindowAction : ShowDialogAction<PropertyWindow> { }

    #endregion
}
