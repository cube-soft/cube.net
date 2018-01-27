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
using Cube.FileSystem;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// ViewModelHelper
    /// 
    /// <summary>
    /// ViewModel をテストするための補助クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    class ViewModelHelper : FileHelper
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ViewModelHelper
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        protected ViewModelHelper() : this("Sample.json") { }

        /* ----------------------------------------------------------------- */
        ///
        /// ViewModelHelper
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="json">RSS フィード用ファイル</param>
        ///
        /* ----------------------------------------------------------------- */
        protected ViewModelHelper(string json) : this(json, new Operator()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// FileHelper
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="json">RSS フィード用ファイル</param>
        /// <param name="io">ファイル操作用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        protected ViewModelHelper(string json, Operator io) : base(io)
        {
            IO.Copy(Example(json), Result("Feeds.json"), true);
        }

        #endregion
    }
}
