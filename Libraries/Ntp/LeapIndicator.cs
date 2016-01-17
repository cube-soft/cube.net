/* ------------------------------------------------------------------------- */
///
/// LeapIndicator.cs
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
    /// LeapIndicator
    /// 
    /// <summary>
    /// 閏秒指示子 (LI: Leap Indicator) の状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum LeapIndicator : uint
    {
        NoWarning    = 0,   // 0 - No warning
        LastMinute61 = 1,   // 1 - Last minute has 61 seconds
        LastMinute59 = 2,   // 2 - Last minute has 59 seconds
        Alarm        = 3    // 3 - Alarm condition (clock not synchronized)
    }
}
