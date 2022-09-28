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

namespace Cube.Net.Rss.Reader;

/* ------------------------------------------------------------------------- */
///
/// Message
///
/// <summary>
/// Provides functionality to create message objects.
/// </summary>
///
/* ------------------------------------------------------------------------- */
public static class Message
{
    /* --------------------------------------------------------------------- */
    ///
    /// Error
    ///
    /// <summary>
    /// Creates the DialogMessage object of the specified error
    /// message.
    /// </summary>
    ///
    /// <param name="err">Exception object.</param>
    /// <param name="message">Error message.</param>
    ///
    /// <returns>DialogMessage object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static DialogMessage Error(Exception err, string message) => new($"{message} ({err.GetType().Name})")
    {
        Title   = Properties.Resources.TitleError,
        Buttons = DialogButtons.Ok,
        Icon    = DialogIcon.Error,
        Value   = DialogStatus.Ok,
    };

    /* --------------------------------------------------------------------- */
    ///
    /// RemoveWarning
    ///
    /// <summary>
    /// Gets a warning message when removing.
    /// </summary>
    ///
    /// <param name="name">Name of the removing entry.</param>
    ///
    /// <returns>DialogMessage object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static DialogMessage RemoveWarning(string name) => new(string.Format(Properties.Resources.MessageRemove, name))
    {
        Title   = Properties.Resources.TitleInformation,
        Buttons = DialogButtons.YesNo,
        Icon    = DialogIcon.Information,
    };

    /* --------------------------------------------------------------------- */
    ///
    /// Import
    ///
    /// <summary>
    /// Gets a message for import.
    /// </summary>
    ///
    /// <returns>OpenFileDialogMessage object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static OpenFileMessage Import() => new(Properties.Resources.MessageImport)
    {
        CheckPathExists = true,
        Multiselect     = false,
        Filters         = new FileDialogFilter[] {
            new(Properties.Resources.FilterOpml, ".opml", ".xml"),
            new(Properties.Resources.FilterAll,  ".*"),
        },
    };

    /* --------------------------------------------------------------------- */
    ///
    /// Export
    ///
    /// <summary>
    /// Gets a message for export.
    /// </summary>
    ///
    /// <returns>SaveFileDialogMessage object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static SaveFileMessage Export() => new(Properties.Resources.MessageExport)
    {
        CheckPathExists = false,
        OverwritePrompt = true,
        Filters         = new FileDialogFilter[] {
            new(Properties.Resources.FilterOpml, ".opml", ".xml"),
            new(Properties.Resources.FilterAll,  ".*"),
        },
    };

    /* --------------------------------------------------------------------- */
    ///
    /// DataDirectory
    ///
    /// <summary>
    /// Gets a message for data directory selection.
    /// </summary>
    ///
    /// <param name="src">Initial value of selection path.</param>
    ///
    /// <returns>DirectoryDialogMessage object.</returns>
    ///
    /* --------------------------------------------------------------------- */
    public static OpenDirectoryMessage DataDirectory(string src) => new(Properties.Resources.MessageDataDirectory)
    {
        Value     = src,
        NewButton = true,
    };
}
