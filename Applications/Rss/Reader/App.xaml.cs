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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Cube.Forms.Controls;
using Cube.Forms.Extensions;
using Cube.Reflection.Extensions;
using Cube.Xui.Logging.Extensions;

/* ------------------------------------------------------------------------- */
///
/// App
///
/// <summary>
/// Represents the main program.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public partial class App : Application, IDisposable
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// App
    ///
    /// <summary>
    /// Initializes a new instance of the App class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public App() => _dispose = new(Dispose);

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// OnStartup
    ///
    /// <summary>
    /// Occurs when the process is launched.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!Activate()) return;

        Logger.Configure(new Logging.NLog.LoggerSource());
        Logger.Info(typeof(App).Assembly);
        Logger.ObserveTaskException();
        this.ObserveUiException();

        try
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
            WebControl.EmulateVersion = WebControlVersion.Latest;
            WebControl.MaxConnections = 10;
            WebControl.NavigationSounds = false;
        }
        catch (Exception err) { Logger.Warn(err); }

        base.OnStartup(e);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OnExit
    ///
    /// <summary>
    /// Occurs when the process is terminated.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        base.OnExit(e);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ~App
    ///
    /// <summary>
    /// Finalizes the App.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    ~App() { _dispose.Invoke(false); }

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases all resources used by the object.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
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
    /// Releases the unmanaged resources used by the WakeableTimer
    /// and optionally releases the managed resources.
    /// </summary>
    ///
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources;
    /// false to release only unmanaged resources.
    /// </param>
    ///
    /* ----------------------------------------------------------------- */
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) _mutex?.Dispose();
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// Activate
    ///
    /// <summary>
    /// Activates the current process.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private bool Activate()
    {
        var asm  = typeof(App).Assembly;
        var name = $"{Environment.UserName}@{asm.GetProduct()}";
        _mutex = new Mutex(true, name, out bool dest);
        if (!dest)
        {
            _mutex = null;
            var id = Process.GetCurrentProcess().Id;
            Process.GetProcessesByName(name).FirstOrDefault(p => p.Id != id).Activate();
            Current.Shutdown();
        }
        return dest;
    }

    #endregion

    #region Fields
    private readonly OnceAction<bool> _dispose;
    private Mutex _mutex;
    #endregion
}
