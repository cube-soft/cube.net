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
using System.Collections.Generic;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// BindableOperator
    ///
    /// <summary>
    /// Bindable(T) および BindableCollection(T) の拡張用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class BindableOperator
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ToBindable
        /// 
        /// <summary>
        /// コレクションを BindingCollection(T) オブジェクトに変換します。
        /// </summary>
        /// 
        /// <param name="src">コレクション</param>
        /// 
        /// <returns>BindableCollection(T) オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static BindableCollection<T> ToBindable<T>(this IEnumerable<T> src) =>
            new BindableCollection<T>(src);
    }
}
