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
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Cube.Xui;
using Cube.Xui.Triggers;

namespace Cube.Net.App.Rss.Reader
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
    public abstract class CommonViewModel : ViewModelBase
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
        /* ----------------------------------------------------------------- */
        public CommonViewModel() : this(GalaSoft.MvvmLight.Messaging.Messenger.Default) { }

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
        public CommonViewModel(IMessenger messenger) : base(messenger) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Messenger
        /// 
        /// <summary>
        /// Messenger オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IMessenger Messenger => MessengerInstance;

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
                () => Messenger.Send(new CloseMessage())
            );

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        /// 
        /// <summary>
        /// メッセージを表示します。
        /// </summary>
        /// 
        /// <param name="message">メッセージ</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(MessageBox message) => Messenger.Send(message);

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        /// 
        /// <summary>
        /// メッセージを表示します。
        /// </summary>
        /// 
        /// <param name="message">メッセージ</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(string message) => Send(new MessageBox(message));

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        /// 
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        /// 
        /// <param name="err">例外オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(Exception err)
        {
            var user = err.GetType() == typeof(ArgumentException);
            var msg = user ? err.Message : $"{err.Message} ({err.GetType().Name})";
            var ss = new System.Text.StringBuilder();

            ss.AppendLine(Properties.Resources.ErrorUnexpected);
            ss.AppendLine();
            ss.AppendLine(msg);

            Send(ss.ToString());
        }

        #endregion

        #region Fields
        private RelayCommand _close;
        #endregion
    }
}
