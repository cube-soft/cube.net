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
namespace Cube.Net.Rss.Reader;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Cube.Observable.Extensions;
using Cube.Text.Extensions;
using Cube.Xui;

/* ------------------------------------------------------------------------- */
///
/// RegisterViewModel
///
/// <summary>
/// Represents the ViewModel for the RegisterWindow.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class RegisterViewModel : ViewModelBase<RegisterFacade>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// RegisterViewModel
    ///
    /// <summary>
    /// Initializes a new instance of the RegisterViewModel class with the
    /// specified arguments.
    /// </summary>
    ///
    /// <param name="callback">Callback action.</param>
    /// <param name="context">Synchronization context.</param>
    ///
    /* --------------------------------------------------------------------- */
    public RegisterViewModel(Func<string, Task> callback, SynchronizationContext context) : base(
        new RegisterFacade(new ContextDispatcher(context, false)),
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
                catch (Exception err) { Send(Message.Error(err, Properties.Resources.ErrorRegister)); }
                finally { Facade.Busy = false; }
            },
            () => Facade.Url.HasValue() && !Facade.Busy
        ).Hook(Facade, nameof(Facade.Busy), nameof(Facade.Url)), nameof(Execute));
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Busy
    ///
    /// <summary>
    /// Gets or sets a value indicating whether processing is in progress.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IElement<bool> Busy => GetElement(() => new BindableElement<bool>(
        () => string.Empty,
        () => Facade.Busy,
        GetDispatcher(false)
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// Url
    ///
    /// <summary>
    /// Gets or sets the URL of the feed to be registered.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public IElement<string> Url => GetElement(() => new BindableElement<string>(
       () => string.Empty,
       () => Facade.Url,
       e  => Facade.Url = e,
       GetDispatcher(false)
    ));

    #endregion

    #region Commands

    /* --------------------------------------------------------------------- */
    ///
    /// Execute
    ///
    /// <summary>
    /// Gets the command to add a new RSS feed.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Execute { get; }

    #endregion
}
