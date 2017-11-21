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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Collections.Specialized;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// BindableCollection(T)
    ///
    /// <summary>
    /// コレクションを Binding 可能にするためのクラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// ObservableCollection(T) で発生する PropertyChanged および
    /// CollectionChanged イベントをコンストラクタで指定された同期
    /// コンテキストを用いて伝搬させます。
    /// </remarks>
    /// 
    /* --------------------------------------------------------------------- */
    public class BindableCollection<T> : ObservableCollection<T>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// BindableCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection() : base() { }

        /* ----------------------------------------------------------------- */
        ///
        /// BindableCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="collection">コピー元となるコレクション</param>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection(IEnumerable<T> collection)
            : this(collection, SynchronizationContext.Current) { }

        /* ----------------------------------------------------------------- */
        ///
        /// BindableCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="collection">コピー元となるコレクション</param>
        /// <param name="context">同期コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection(IEnumerable<T> collection, SynchronizationContext context)
            : base(collection)
        {
            _context = context;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// OnPropertyChanged
        /// 
        /// <summary>
        /// PropertyChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_context != null) _context.Post(_ => base.OnPropertyChanged(e), null);
            else base.OnPropertyChanged(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnCollectionChanged
        /// 
        /// <summary>
        /// CollectionChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_context != null) _context.Post(_ => base.OnCollectionChanged(e), null);
            else base.OnCollectionChanged(e);
        }

        #region Fields
        private SynchronizationContext _context = SynchronizationContext.Current;
        #endregion

        #endregion
    }
}
