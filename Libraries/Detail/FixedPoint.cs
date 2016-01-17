/* ------------------------------------------------------------------------- */
///
/// FixedPoint.cs
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
using System;

namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// FixedPoint
    /// 
    /// <summary>
    /// 符号付き 32bit 固定小数点数から double への変換機能を提供するための
    /// クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal abstract class FixedPoint
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ToDouble
        /// 
        /// <summary>
        /// 符号付き 32bit 固定小数点数から double へ変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static double ToDouble(Int32 value)
        {
            var number = (Int16)(value >> 16);
            var fraction = (UInt16)(value & Int16.MaxValue);
            return number + fraction / _CompensatingRate16;
        }

        #region Constant variables
        private static readonly double _CompensatingRate16 = 0x10000d;
        #endregion
    }
}
