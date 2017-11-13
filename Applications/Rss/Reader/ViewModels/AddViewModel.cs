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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Cube.Net.Http;
using Cube.Net.Rss;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// AddViewModel
    ///
    /// <summary>
    /// フィードの追加画面とモデルを関連付けるための ViewModel です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class AddViewModel : ViewModelBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// AddViewModel
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public AddViewModel(Action<RssFeed> callback) : base(new Messenger())
        {
            _callback = callback;
            Url.PropertyChanged += (s, e) => Add.RaiseCanExecuteChanged();
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
        /// Add
        /// 
        /// <summary>
        /// 新しい RSS フィードを追加するコマンドを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RelayCommand Add
            => _add = _add ?? new RelayCommand(
                async () =>
                {
                    var http = HttpClientFactory.Create();
                    var rss  = await http.GetRssAsync(new Uri(Url.Value));
                    _callback(rss);
                    Messenger.Send(this);
                },
                () => !string.IsNullOrEmpty(Url.Value)
            );

        #endregion

        #region Fields
        private Action<RssFeed> _callback;
        private RelayCommand _add;
        #endregion
    }
}
