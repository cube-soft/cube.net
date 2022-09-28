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
using System.Reflection;
using Cube.FileSystem;
using Cube.Reflection.Extensions;
using Cube.Synchronous;

/* ------------------------------------------------------------------------- */
///
/// UpdateChecker
///
/// <summary>
/// Provides functionality to check for updates.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class UpdateChecker
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// UpdateChecker
    ///
    /// <summary>
    /// Initializes a new instance of the UpdateChecker class with the
    /// specified user settings.
    /// </summary>
    ///
    /// <param name="src">User settings.</param>
    ///
    /* --------------------------------------------------------------------- */
    public UpdateChecker(SettingFolder src)
    {
        var dir = Assembly.GetExecutingAssembly().GetDirectoryName();

        FileName = Io.Combine(dir, "CubeChecker.exe");
        Setting  = src;

        _timer.Interval = TimeSpan.FromDays(1);
        _ = _timer.SubscribeSync(WhenTick);

        if (Setting.Shared.CheckUpdate) Start();
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Setting
    ///
    /// <summary>
    /// Gets the user settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public SettingFolder Setting { get; }

    /* --------------------------------------------------------------------- */
    ///
    /// FileName
    ///
    /// <summary>
    /// Gets the path to the update confirmation program.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string FileName { get; }

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Start
    ///
    /// <summary>
    /// Starts periodic execution.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Start()
    {
        var time  = Setting.Shared.LastCheckUpdate ?? DateTime.MinValue;
        var past  = DateTime.Now - time;
        var delta = past < _timer.Interval ?
                    _timer.Interval - past :
                    TimeSpan.FromMilliseconds(100);
        _timer.Start(delta);
        Logger.Debug($"{nameof(Start)} Interval:{_timer.Interval} InitialDelay:{delta}");
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Stop
    ///
    /// <summary>
    /// Stops periodic execution.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public void Stop() => _timer.Stop();

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// WhenTick
    ///
    /// <summary>
    /// Executed at regular intervals.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void WhenTick()
    {
        try { _ = Process.Start(FileName, "CubeRssReader"); }
        catch (Exception err) { Logger.Warn($"{FileName} ({err.Message})"); }
        finally { Setting.Shared.LastCheckUpdate = DateTime.Now; }
    }

    #endregion

    #region Fields
    private readonly WakeableTimer _timer = new();
    #endregion
}
