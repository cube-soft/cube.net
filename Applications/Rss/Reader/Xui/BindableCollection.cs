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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Collections.Specialized;
using Cube.Generics;
using Cube.Log;

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
    public class BindableCollection<T> : ObservableCollection<T>, IDisposable
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
        public BindableCollection() : this(new T[0]) { }

        /* ----------------------------------------------------------------- */
        ///
        /// BindableCollection
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="context">同期コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCollection(SynchronizationContext context) :
            this(new T[0], context) { }

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
        public BindableCollection(IEnumerable<T> collection) :
            this(collection, SynchronizationContext.Current) { }

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
        public BindableCollection(IEnumerable<T> collection, SynchronizationContext context) :
            base(collection ?? new T[0])
        {
            _dispose = new OnceAction<bool>(Dispose);
            _context = context;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IsSynchronous
        ///
        /// <summary>
        /// UI スレッドに対して同期的にイベントを発生させるかどうかを
        /// 示す値を取得または設定します。
        /// </summary>
        ///
        /// <remarks>
        /// true の場合は Send メソッド、false の場合は Post メソッドを
        /// 用いてイベントを伝搬します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsSynchronous { get; set; } = false;

        /* ----------------------------------------------------------------- */
        ///
        /// IsRedirected
        ///
        /// <summary>
        /// 要素のイベントをリダイレクトするかどうかを示す値を取得または
        /// 設定します。
        /// </summary>
        ///
        /// <remarks>
        /// true に設定した場合、要素の PropertyChanged イベントが発生した
        /// 時に該当要素に対して NotifyCollectionChangedAction.Replace を
        /// 設定した状態で CollectionChanged イベントを発生させます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public bool IsRedirected { get; set; } = true;

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~BindableCollection
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~BindableCollection() { _dispose.Invoke(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            _dispose.Invoke(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージオブジェクトを開放するかどうか
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) UnsetHandler(this);
        }

        #endregion

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
            if (_context != null)
            {
                if (IsSynchronous) _context.Send(_ => base.OnPropertyChanged(e), null);
                else _context.Post(_ => base.OnPropertyChanged(e), null);
            }
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
            if (_context != null)
            {
                if (IsSynchronous) _context.Send(_ => OnCollectionChangedCore(e), null);
                else _context.Post(_ => OnCollectionChangedCore(e), null);
            }
            else OnCollectionChangedCore(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnCollectionChangedCore
        ///
        /// <summary>
        /// CollectionChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void OnCollectionChangedCore(NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        SetHandler(e.NewItems);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        UnsetHandler(e.OldItems);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        UnsetHandler(e.OldItems);
                        SetHandler(e.NewItems);
                        break;
                }
                base.OnCollectionChanged(e);
            }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetHandler
        ///
        /// <summary>
        /// 各項目に対してハンドラを設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetHandler(IList items)
        {
            if (!IsRedirected) return;

            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged e)
                {
                    e.PropertyChanged -= WhenMemberChanged;
                    e.PropertyChanged += WhenMemberChanged;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UnsetHandler
        ///
        /// <summary>
        /// 各項目からハンドラの設定を解除します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void UnsetHandler(IList items)
        {
            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged e) e.PropertyChanged -= WhenMemberChanged;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMemberChanged
        ///
        /// <summary>
        /// 要素のプロパティが変更された時に実行されるハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMemberChanged(object sender, PropertyChangedEventArgs e)
        {
            var index = IndexOf(sender.TryCast<T>());
            if (index < 0) return;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                sender,
                sender,
                index
            ));
        }

        #endregion

        #region Fields
        private SynchronizationContext _context;
        private OnceAction<bool> _dispose;
        #endregion
    }
}
