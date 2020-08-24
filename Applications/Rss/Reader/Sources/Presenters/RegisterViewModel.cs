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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Cube.Mixin.Observing;
using Cube.Mixin.String;
using Cube.Xui;

namespace Cube.Net.Rss.Reader
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
    public class RegisterViewModel : GenericViewModel<RegisterFacade>
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
        public RegisterViewModel(Func<string, Task> callback, SynchronizationContext context) : base(
            new RegisterFacade(new ContextInvoker(context, false)),
            new Aggregator(),
            context
        ) {
            Execute = Get(() => new DelegateCommand(
                async () => {
                    try
                    {
                        Facade.Busy = true;
                        await callback?.Invoke(Url.Value);
                        Close.Execute(null);
                    }
                    catch (Exception err) { Send(MessageFactory.Error(err, Properties.Resources.ErrorRegister)); }
                    finally { Facade.Busy = false; }
                },
                () => Facade.Url.HasValue() && !Facade.Busy
            ).Associate(Facade, nameof(Facade.Busy), nameof(Facade.Url)), nameof(Execute));
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
        public IElement<bool> Busy => Get(() => new BindableElement<bool>(
            () => string.Empty,
            () => Facade.Busy,
            GetInvoker(false)
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// Url
        ///
        /// <summary>
        /// 追加するフィードの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IElement<string> Url => Get(() => new BindableElement<string>(
           () => string.Empty,
           () => Facade.Url,
           e  => Facade.Url = e,
           GetInvoker(false)
        ));

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
        public ICommand Execute { get; }

        #endregion
    }
}
