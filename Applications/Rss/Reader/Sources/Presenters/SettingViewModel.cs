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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Cube.Reflection.Extensions;
using Cube.Xui;
using Cube.Xui.Commands.Extensions;
using Cube.Xui.Drawing.Extensions;

/* ------------------------------------------------------------------------- */
///
/// SettingViewModel
///
/// <summary>
/// Represents the ViewModel for the SettingWindow.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class SettingViewModel : ViewModelBase<SettingFolder>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// SettingViewModel
    ///
    /// <summary>
    /// Initializes a new instance of the SettingViewModel class with the
    /// specified arguments.
    /// </summary>
    ///
    /// <param name="settings">User settings.</param>
    /// <param name="context">Synchronization context.</param>
    ///
    /* --------------------------------------------------------------------- */
    public SettingViewModel(SettingFolder settings, SynchronizationContext context) :
        base(settings, new Aggregator(), context)
    {
        Local  = new BindableValue<LocalSetting>(settings.Value, GetDispatcher(false));
        Shared = new BindableValue<SharedSetting>(settings.Shared, GetDispatcher(false));
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Local
    ///
    /// <summary>
    /// Gets the local settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public BindableValue<LocalSetting> Local { get; }

    /* --------------------------------------------------------------------- */
    ///
    /// Shared
    ///
    /// <summary>
    /// Gets the shared user settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public BindableValue<SharedSetting> Shared { get; }

    /* --------------------------------------------------------------------- */
    ///
    /// Logo
    ///
    /// <summary>
    /// Gets the logo image of the application.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public BitmapImage Logo => _logo ??= Properties.Resources.Logo.ToBitmapImage();

    /* --------------------------------------------------------------------- */
    ///
    /// Product
    ///
    /// <summary>
    /// Gets the product name.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Product => "CubeRSS Reader";

    /* --------------------------------------------------------------------- */
    ///
    /// Version
    ///
    /// <summary>
    /// Gets the version string.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Version => $"Version {Facade.Version.ToString(3, true)}";

    /* --------------------------------------------------------------------- */
    ///
    /// Windows
    ///
    /// <summary>
    /// Gets the Windows version string.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Windows => Environment.OSVersion.ToString();

    /* --------------------------------------------------------------------- */
    ///
    /// Framework
    ///
    /// <summary>
    /// Gets the framework version string.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Framework => $"Microsoft .NET Framework {Environment.Version}";

    /* --------------------------------------------------------------------- */
    ///
    /// Copyright
    ///
    /// <summary>
    /// Gets the copyright statement.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string Copyright { get; } = typeof(App).Assembly.GetCopyright();

    #endregion

    #region Commands

    /* --------------------------------------------------------------------- */
    ///
    /// SelectDataDirectory
    ///
    /// <summary>
    /// Gets the command to select the data directory.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand SelectDataDirectory => Get(() => new DelegateCommand(
        () => Send(Message.DataDirectory(Local.Value.DataDirectory))
    ));

    /* --------------------------------------------------------------------- */
    ///
    /// Apply
    ///
    /// <summary>
    /// Gets the apply command.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public ICommand Apply => Get(() => new DelegateCommand(() =>
    {
        Send(new ApplyMessage());
        Close.Execute();
    }));

    #endregion

    #region Fields
    private BitmapImage _logo;
    #endregion
}
