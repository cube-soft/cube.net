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
using Cube.Xui.Converters;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// DivisionConverter
    ///
    /// <summary>
    /// 特定の値で除算するための変換用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class DivisionConverter : DuplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// DivisionConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="divisor">除数</param>
        ///
        /* ----------------------------------------------------------------- */
        public DivisionConverter(int divisor) : base(
            e => (int)e / divisor,
            e => (int)e * divisor
        ) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// MinuteConverter
    ///
    /// <summary>
    /// 秒単位の値を分単位に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class MinuteConverter : DivisionConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// MinuteConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public MinuteConverter() : base(60) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// HourConverter
    ///
    /// <summary>
    /// 秒単位の値を時間単位に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class HourConverter : DivisionConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// HourConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public HourConverter() : base(60 * 60) { }
    }
}
