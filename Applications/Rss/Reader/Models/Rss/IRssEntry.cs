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
using System.ComponentModel;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// IRssEntry
    ///
    /// <summary>
    /// RSS のエントリおよびカテゴリを表すインターフェースです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public interface IRssEntry : INotifyPropertyChanged
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Parent
        /// 
        /// <summary>
        /// 親要素を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        IRssEntry Parent { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        /// 
        /// <summary>
        /// タイトルを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        string Title { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        /// 
        /// <summary>
        /// 未読記事数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        int Count { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Selected
        /// 
        /// <summary>
        /// 選択状態かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        bool Selected { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Expanded
        /// 
        /// <summary>
        /// 子要素が表示状態かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        bool Expanded { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Editing
        /// 
        /// <summary>
        /// ユーザによる編集中かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        bool Editing { get; set; }
    }

}
