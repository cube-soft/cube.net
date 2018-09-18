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
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cube.Net.Rss.App.Reader
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
    public class RegisterViewModel : CommonViewModel
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
        public RegisterViewModel(Func<string, Task> callback) : base(new Messenger())
        {
            _callback = callback;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Busy
        ///
        /// <summary>
        /// 処理中かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<bool> Busy { get; } = new Bindable<bool>(false);

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

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Execute
        ///
        /// <summary>
        /// 新しい RSS フィードを登録するコマンドを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Execute =>
            _execute = _execute ?? new BindableCommand(
                async () =>
                {
                    try
                    {
                        Busy.Value = true;
                        await _callback?.Invoke(Url.Value);
                        Close.Execute(null);
                    }
                    catch (Exception err) { Send(MessageFactory.Error(err, Properties.Resources.ErrorRegister)); }
                    finally { Busy.Value = false; }
                },
                () => !string.IsNullOrEmpty(Url.Value) && !Busy.Value,
                Busy, Url
            );

        #endregion

        #region Fields
        private readonly Func<string, Task> _callback;
        private ICommand _execute;
        #endregion
    }
}
