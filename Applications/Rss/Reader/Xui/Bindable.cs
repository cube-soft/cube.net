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
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cube.Xui
{
    /* --------------------------------------------------------------------- */
    ///
    /// Bindable(T)
    ///
    /// <summary>
    /// 値を Binding 可能にするためのクラスです。
    /// </summary>
    /// 
    /// <remarks>
    /// Value プロパティを通じて実際の値にアクセスします。
    /// PropertyChanged イベントは、コンストラクタで指定された同期
    /// コンテキストを用いて発生します。
    /// </remarks>
    /// 
    /* --------------------------------------------------------------------- */
    public class Bindable<T> : INotifyPropertyChanged
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Bindable
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable() : this(default(T)) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Bindable
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="value">初期値</param>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable(T value) : this(value, SynchronizationContext.Current) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Bindable
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="value">初期値</param>
        /// <param name="context">同期コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable(T value, SynchronizationContext context)
        {
            _value   = value;
            _context = context;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        /// 
        /// <summary>
        /// 実際の値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public T Value
        {
            get => _value;
            set
            {
                if (_value != null && _value.Equals(value)) return;
                if (_value is INotifyPropertyChanged p0) p0.PropertyChanged -= WhenMemberChanged;
                _value = value;
                if (_value is INotifyPropertyChanged p1) p1.PropertyChanged += WhenMemberChanged;
                RaisePropertyChanged();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Synchronously
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
        public bool Synchronously { get; set; } = false;

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// PropertyChanged
        /// 
        /// <summary>
        /// プロパティの内容変更時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event PropertyChangedEventHandler PropertyChanged;

        /* ----------------------------------------------------------------- */
        ///
        /// OnPropertyChanged
        /// 
        /// <summary>
        /// PropertyChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        /* ----------------------------------------------------------------- */
        ///
        /// RaisePropertyChanged
        /// 
        /// <summary>
        /// PropertyChanged イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            var e = new PropertyChangedEventArgs(name);
            if (_context != null)
            {
                if (Synchronously) _context.Send(_ => OnPropertyChanged(e), null);
                else _context.Post(_ => OnPropertyChanged(e), null);
            }
            else OnPropertyChanged(e);
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenMemberChanged
        /// 
        /// <summary>
        /// Value の PropertyChanged イベント発生時に実行される
        /// ハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenMemberChanged(object sender, PropertyChangedEventArgs e)
            => RaisePropertyChanged(nameof(Value));

        #endregion

        #region Fields
        private T _value;
        private SynchronizationContext _context;
        #endregion
    }
}
