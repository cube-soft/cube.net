﻿/* ------------------------------------------------------------------------- */
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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Cube.Net.Rss;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RegisterViewModel
    ///
    /// <summary>
    /// 新規 URL の登録画面とモデルを関連付けるための ViewModel です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RegisterViewModel : ViewModelBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterViewModel
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RegisterViewModel(Action<RssFeed> callback) : base(new Messenger())
        {
            _callback = callback;
            Url.PropertyChanged += (s, e) => Register.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Url
        /// 
        /// <summary>
        /// 追加するフィードの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Url { get; } = new Bindable<string>();

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
        /// Register
        /// 
        /// <summary>
        /// 新しい RSS フィードを登録するコマンドを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RelayCommand Register
            => _register = _register ?? new RelayCommand(
                async () =>
                {
                    try
                    {
                        var http = new RssClient();
                        var rss = await http.GetAsync(new Uri(Url.Value));
                        if (rss == null) Error(Properties.Resources.ErrorFeedNotFound);
                        else
                        {
                            _callback?.Invoke(rss);
                            Messenger.Send(this);
                        }
                    }
                    catch (Exception err) { Error(err); }
                },
                () => !string.IsNullOrEmpty(Url.Value)
            );

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        /// 
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Error(string message)
            => Messenger.Send(new MessageBox(message));

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        /// 
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Error(Exception err)
        {
            var ss = new System.Text.StringBuilder();
            ss.AppendLine(Properties.Resources.ErrorUnexpected);
            ss.AppendLine();
            ss.AppendLine($"{err.Message} ({err.GetType().Name})");
            Error(ss.ToString());
        }

        #region Fields
        private Action<RssFeed> _callback;
        private RelayCommand _register;
        #endregion

        #endregion
    }
}