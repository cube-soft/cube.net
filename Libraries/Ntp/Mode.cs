/* ------------------------------------------------------------------------- */
///
/// Mode.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// Mode
    /// 
    /// <summary>
    /// 動作モードの状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum Mode : uint
    {
        Unknown          = 0,   // 0, 6, 7 - Reserved
        SymmetricActive  = 1,   // 1 - Symmetric active
        SymmetricPassive = 2,   // 2 - Symmetric pasive
        Client           = 3,   // 3 - Client
        Server           = 4,   // 4 - Server
        Broadcast        = 5,   // 5 - Broadcast
    }
}
