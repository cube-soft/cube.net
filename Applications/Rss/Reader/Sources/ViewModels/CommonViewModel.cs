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
using Cube.Xui;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows.Input;

namespace Cube.Net.Rss.App.Reader
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
    public abstract class CommonViewModel : MessengerViewModel, IDisposable
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
        /// <param name="messenger">メッセージ伝搬用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        protected CommonViewModel(IMessenger messenger) : base(messenger)
        {
            _dispose = new OnceAction<bool>(Dispose);
        }

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
        public ICommand Close =>
            _close = _close ?? new RelayCommand(
                () => Send<CloseMessage>()
            );

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private ICommand _close;
        #endregion
    }
}
