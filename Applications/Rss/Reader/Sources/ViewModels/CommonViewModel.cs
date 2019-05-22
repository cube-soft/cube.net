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
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// CommonViewModel
    ///
    /// <summary>
    /// ViewModel の既定となるクラスです。一般的なコマンドを実装します。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class CommonViewModel : PresentableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MainViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="aggregator">Message aggregator.</param>
        ///
        /* ----------------------------------------------------------------- */
        protected CommonViewModel(Aggregator aggregator) : base(aggregator) { }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Close
        ///
        /// <summary>
        /// 画面を閉じるコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Close => _close ?? (_close = new RelayCommand(() => Send<CloseMessage>()));

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// Releases the unmanaged resources used by the object and
        /// optionally releases the managed resources.
        /// </summary>
        ///
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing) { }

        /* ----------------------------------------------------------------- */
        ///
        /// TrackSync
        ///
        /// <summary>
        /// 指定された処理を実行し、例外が発生した場合には DialogMessage を
        /// 送信します。
        /// </summary>
        ///
        /// <param name="action">実行内容</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void TrackSync(Action action) => Track(action, DialogMessage.Create, true);

        #endregion

        #region Fields
        private ICommand _close;
        #endregion
    }
}
