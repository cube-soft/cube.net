/* ------------------------------------------------------------------------- */
///
/// Stratum.cs
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
    /// Stratum
    /// 
    /// <summary>
    /// 階層の状態を定義した列挙型です。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public enum Stratum
    {
        Unspecified,            // 0 - unspecified or unavailable
        PrimaryReference,       // 1 - primary reference (e.g. radio-clock)
        SecondaryReference,     // 2-15 - secondary reference (via NTP or SNTP)
        Reserved                // 16-255 - reserved
    }
}
