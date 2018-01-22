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
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// BindableCommand(T)
    ///
    /// <summary>
    /// 特定のプロパティを関連付けられるコマンドです。
    /// 関連付けられたオブジェクトの PropertyChanged イベント発生時に
    /// CanExecuteChanged イベントを発生させます。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class BindableCommand<T> : RelayCommand<T>, IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// BindableCommand
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="execute">実行内容</param>
        /// <param name="canExecute">実行可能かどうか</param>
        /// <param name="obj">関連付けるオブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public BindableCommand(Action<T> execute, Func<T, bool> canExecute,
            INotifyPropertyChanged obj) : base(execute, canExecute)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            _dispose = new OnceAction<bool>(Dispose);
            AssociatedObject = obj;
            AssociatedObject.PropertyChanged += WhenChanged;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// AssociatedObject
        /// 
        /// <summary>
        /// 関連付けられたオブジェクトを取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public INotifyPropertyChanged AssociatedObject { get; }

        #endregion

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~BindableCommand
        /// 
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        ~BindableCommand() { _dispose.Invoke(false); }

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
            if (disposing)
            {
                AssociatedObject.PropertyChanged -= WhenChanged;
            }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChanged
        /// 
        /// <summary>
        /// 関連付けられたオブジェクトのプロパティが変更差た時に実行
        /// されるハンドラです。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void WhenChanged(object sender, PropertyChangedEventArgs e) =>
            RaiseCanExecuteChanged();

        #endregion

        #region Fields
        private OnceAction<bool> _dispose;
        #endregion
    }
}
