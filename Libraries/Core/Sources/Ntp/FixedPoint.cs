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
namespace Cube.Net.Ntp
{
    /* --------------------------------------------------------------------- */
    ///
    /// FixedPoint
    ///
    /// <summary>
    /// Provides functionality to convert a signed 32-bit fixed-point number
    /// to double.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class FixedPoint
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ToDouble
        ///
        /// <summary>
        /// Converts a signed 32-bit fixed-point number to a double.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static double ToDouble(int value)
        {
            var number = (short)(value >> 16);
            var fraction = (ushort)(value & short.MaxValue);
            return number + fraction / _CompensatingRate16;
        }

        #endregion

        #region Fields
        private static readonly double _CompensatingRate16 = 0x10000d;
        #endregion
    }
}
