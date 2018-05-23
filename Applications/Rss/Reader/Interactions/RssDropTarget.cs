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
using GongSolutions.Wpf.DragDrop;
using System;
using System.Windows;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssDropTarget
    ///
    /// <summary>
    /// RssEntry オブジェクトのドラッグ&amp;ドロップを処理するための
    /// クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssDropTarget : IDropTarget
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssDropTarget
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="callback">
        /// ドロップ時に実行されるコールバック関数
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        public RssDropTarget(Action<IRssEntry, IRssEntry, int> callback)
        {
            System.Diagnostics.Debug.Assert(callback != null);
            _callback = callback;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IsReadOnly
        ///
        /// <summary>
        /// 読み取り専用モードかどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsReadOnly { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// DragOver
        ///
        /// <summary>
        /// ドラッグ状態でマウスオーバ時に実行されます。
        /// </summary>
        ///
        /// <param name="e">ドロップ情報</param>
        ///
        /* ----------------------------------------------------------------- */
        public void DragOver(IDropInfo e)
        {
            if (!CanDrop(e)) return;

            e.Effects           = DragDropEffects.Move;
            e.DropTargetAdorner = e.Data is RssEntry && e.TargetItem is RssCategory ?
                                  DropTargetAdorners.Highlight :
                                  DropTargetAdorners.Insert;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Drop
        ///
        /// <summary>
        /// ドロップ時に実行されます。
        /// </summary>
        ///
        /// <param name="e">ドロップ情報</param>
        ///
        /* ----------------------------------------------------------------- */
        public void Drop(IDropInfo e) =>
            _callback(e.Data as IRssEntry, e.TargetItem as IRssEntry, e.InsertIndex);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// CanDrop
        ///
        /// <summary>
        /// ドロップ可能かどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private bool CanDrop(IDropInfo e)
        {
            if (IsReadOnly || e.Data == e.TargetItem) return false;
            if (e.Data is RssEntry src &&
                e.TargetItem is RssCategory dest &&
                src.Parent == dest) return false;
            return true;
        }

        #endregion

        #region Fields
        private readonly Action<IRssEntry, IRssEntry, int> _callback;
        #endregion
    }
}
