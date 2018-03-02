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
using Cube.Log;
using Cube.Xui;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows.Input;

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
    public abstract class CommonViewModel : ViewModelBase, IDisposable
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
        public CommonViewModel(IMessenger messenger) : base(messenger)
        {
            _dispose = new OnceAction<bool>(Dispose);
        }

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
        protected void Send(DialogMessage message) => Messenger.Send(message);

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        ///
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        ///
        /// <param name="err">例外オブジェクト</param>
        /// <param name="message">例外発生時のメッセージ</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(Exception err, string message)
        {
            this.LogError(message);
            this.LogError(err.ToString(), err);

            var ss = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(message)) ss.AppendLine(message);
            ss.Append($"{err.Message} ({err.GetType().Name})");
            Send(new DialogMessage(ss.ToString())
            {
                Button = System.Windows.MessageBoxButton.OK,
                Image  = System.Windows.MessageBoxImage.Error,
                Result = true,
            });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        ///
        /// <summary>
        /// 例外発生時にエラーメッセージを表示します。
        /// </summary>
        ///
        /// <param name="action">実行内容</param>
        /// <param name="message">例外発生時のメッセージ</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(Action action, string message)
        {
            try { action(); }
            catch (Exception err) { Send(err, message); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Send
        ///
        /// <summary>
        /// 例外発生時にエラーメッセージを表示します。
        /// </summary>
        ///
        /// <param name="action">実行内容</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Send(Action action) => Send(action, string.Empty);

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~CommonViewModel
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~CommonViewModel() { _dispose.Invoke(false); }

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
        protected virtual void Dispose(bool disposing) { }

        #endregion

        #endregion

        #region Fields
        private OnceAction<bool> _dispose;
        private RelayCommand _close;
        #endregion
    }
}
