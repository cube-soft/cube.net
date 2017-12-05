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
using System;
using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssEntryDropTarget
    ///
    /// <summary>
    /// RssEntry オブジェクトのドラッグ&amp;ドロップを処理するための
    /// クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RssEntryDropTarget : IDropTarget
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssEntryDropTarget
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
        public RssEntryDropTarget(Action<RssEntryBase, RssEntryBase, int> callback)
        {
            System.Diagnostics.Debug.Assert(callback != null);
            _callback = callback;
        }

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
            if (CanDrop(e))
            {
                var branch = e.TargetItem is RssCategory; 
                e.Effects = DragDropEffects.Move;
                e.DropTargetAdorner = branch ?
                    DropTargetAdorners.Highlight :
                    DropTargetAdorners.Insert;
            }
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
        public void Drop(IDropInfo e)
        {
            var cvt   = e.TargetItem as RssEntryBase;
            var src   = e.Data as RssEntryBase;
            var dest  = cvt is RssEntry ? cvt.Parent : cvt;
            var index = cvt is RssEntry ? e.InsertIndex : -1;

            _callback(src, dest, index);
        }

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
            var src  = e.Data as RssEntryBase;
            var dest = e.TargetItem as RssEntryBase;

            return src != dest && src.Parent != dest;
        }

        #region Fields
        private Action<RssEntryBase, RssEntryBase, int> _callback;
        #endregion

        #endregion
    }
}
