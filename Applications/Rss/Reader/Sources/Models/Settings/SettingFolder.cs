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
using Cube.DataContract;
using Cube.FileSystem;
using Cube.Forms.Controls;
using Cube.Reflection.Extensions;
using Cube.Text.Extensions;

/* ------------------------------------------------------------------------- */
///
/// SettingFolder
///
/// <summary>
/// Represents the application and/or user settings.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public class SettingFolder : SettingFolder<LocalSetting>
{
    #region Constructors

    /* --------------------------------------------------------------------- */
    ///
    /// SettingFolder
    ///
    /// <summary>
    /// Initializes static resources of the SettingFolder class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    static SettingFolder() => Network.Setup();

    /* --------------------------------------------------------------------- */
    ///
    /// SettingFolder
    ///
    /// <summary>
    /// Initializes a new instance of the specified Assembly object.
    /// </summary>
    ///
    /// <param name="asm">Assembly object.</param>
    ///
    /* --------------------------------------------------------------------- */
    public SettingFolder(Assembly asm) : this(Io.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        asm.GetCompany(),
        asm.GetProduct()
    ), asm.GetSoftwareVersion()) { }

    /* --------------------------------------------------------------------- */
    ///
    /// SettingFolder
    ///
    /// <summary>
    /// Initializes a new instance of the SettingFolder class with the
    /// specified arguments.
    /// </summary>
    ///
    /// <param name="root">Root path of the setting directory.</param>
    /// <param name="version">Software version.</param>
    ///
    /* --------------------------------------------------------------------- */
    public SettingFolder(string root, SoftwareVersion version) :
        base(Format.Json, Io.Combine(root, LocalSetting.FileName), version)
    {
        AutoSave = true;
        RootDirectory = root;
        DataDirectory = root;
        Version.Suffix = "β";
        Reset(root);
    }

    #endregion

    #region Properties

    /* --------------------------------------------------------------------- */
    ///
    /// Lock
    ///
    /// <summary>
    /// Gets the settings for locking.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public LockSetting Lock { get; private set; }

    /* --------------------------------------------------------------------- */
    ///
    /// Shared
    ///
    /// <summary>
    /// Gets the shared user settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public SharedSetting Shared { get; private set; }

    /* --------------------------------------------------------------------- */
    ///
    /// RootDirectory
    ///
    /// <summary>
    /// Gets the path that is the root of the directory that holds the
    /// local settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string RootDirectory { get; }

    /* --------------------------------------------------------------------- */
    ///
    /// DataDirectory
    ///
    /// <summary>
    /// Gets the path to the directory that holds user settings.
    /// </summary>
    ///
    /// <remarks>
    /// DataDirectory property may be changed by GUI user operations, and
    /// should be retrieved from this property after initializing the
    /// SettingFolder or after loading the local settings file.
    /// </remarks>
    ///
    /* --------------------------------------------------------------------- */
    public string DataDirectory { get; private set; }

    /* --------------------------------------------------------------------- */
    ///
    /// UserAgent
    ///
    /// <summary>
    /// Gets the User-Agent string.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public string UserAgent => _userAgent ??= GetUserAgent();

    #endregion

    #region Methods

    /* --------------------------------------------------------------------- */
    ///
    /// Dispose
    ///
    /// <summary>
    /// Releases the unmanaged resources used by the object and optionally
    /// releases the managed resources.
    /// </summary>
    ///
    /// <param name="disposing">
    /// true to release both managed and unmanaged resources;
    /// false to release only unmanaged resources.
    /// </param>
    ///
    /* --------------------------------------------------------------------- */
    protected override void Dispose(bool disposing)
    {
        Lock.Release(DataDirectory);
        base.Dispose(disposing);
    }

    #endregion

    #region Implementations

    /* --------------------------------------------------------------------- */
    ///
    /// OnLoad
    ///
    /// <summary>
    /// Occurs when the settings are loaded.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnLoad()
    {
        Logger.Try(base.OnLoad);

        var min = 100;
        var dest = Value;
        if (!dest.DataDirectory.HasValue()) dest.DataDirectory = RootDirectory;
        dest.EntryColumn = Math.Min(dest.EntryColumn, dest.Width - min * 2);
        dest.ArticleColumn = Math.Min(dest.ArticleColumn, dest.Width - dest.EntryColumn - min);
        DataDirectory = dest.DataDirectory;
        Reset(DataDirectory);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// OnSave
    ///
    /// <summary>
    /// Occurs when the settings are saved.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    protected override void OnSave()
    {
        if (!Lock.IsReadOnly) Shared.Save(DataDirectory);
        base.OnSave();
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GetLastCheckUpdate
    ///
    /// <summary>
    /// Gets the LastCheckUpdate value.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private DateTime? GetLastCheckUpdate()
    {
        try
        {
            var name = $@"Software\CubeSoft\CubeRssReader";
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, false);
            if (key == null) return default;

            var s = key.GetValue("LastCheckUpdate") as string;
            return s.HasValue()? DateTime.Parse(s).ToLocalTime() : default;
        }
        catch (Exception err) { Logger.Warn(err); }
        return default;
    }

    /* --------------------------------------------------------------------- */
    ///
    /// GetUserAgent
    ///
    /// <summary>
    /// Gets a string representing the User-Agent.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private string GetUserAgent()
    {
        var app  = $"CubeRssReader/{Version.Number}";
        var win  = Environment.OSVersion.VersionString;
        var net  = $".NET {Environment.Version}";
        var view = WebControl.EmulateVersion;
        return $"{app} ({win}; {net}; {view})";
    }

    /* --------------------------------------------------------------------- */
    ///
    /// Reset
    ///
    /// <summary>
    /// Reloads the additional settings.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    private void Reset(string directory)
    {
        Lock = LockSetting.Load(directory);
        Debug.Assert(Lock != null);
        Lock.PropertyChanged += (s, ev) => OnPropertyChanged(ev);

        Shared = SharedSetting.Load(directory);
        Debug.Assert(Shared != null);
        Shared.LastCheckUpdate = GetLastCheckUpdate();
        Shared.PropertyChanged += (s, ev) => OnPropertyChanged(ev);
    }

    #endregion

    #region Fields
    private string _userAgent;
    #endregion
}
