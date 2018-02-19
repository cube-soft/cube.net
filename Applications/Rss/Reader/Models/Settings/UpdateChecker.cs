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
using System.Diagnostics;
using Cube.Log;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// UpdateChecker
    ///
    /// <summary>
    /// RSS フィードに関連する処理の窓口となるクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class UpdateChecker
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// UpdateChecker
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="settings">設定用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public UpdateChecker(SettingsFolder settings)
        {
            var io  = settings.IO;
            var dir = io.Get(AssemblyReader.Default.Location).DirectoryName;

            FileName = io.Combine(dir, "UpdateChecker.exe");
            Settings = settings;

            var time     = settings.Value.LastCheckUpdate ?? DateTime.MinValue;
            var past     = DateTime.Now - time;
            var interval = TimeSpan.FromDays(1);

            _timer.Interval = interval;
            _timer.Subscribe(WhenTick);
            _timer.Start(past < interval ? interval - past : TimeSpan.FromMilliseconds(100));
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        ///
        /// <summary>
        /// 設定用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder Settings { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// アップデート確認用プログラムのパスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string FileName { get; }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenTick
        ///
        /// <summary>
        /// 一定時間毎に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenTick()
        {
            try { Process.Start(FileName, Settings.Product); }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }
            finally { Settings.Value.LastCheckUpdate = DateTime.Now; }
        }

        #endregion

        #region Fields
        private WakeableTimer _timer = new WakeableTimer();
        #endregion
    }
}
