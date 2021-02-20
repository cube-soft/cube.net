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
using System.Reflection;
using Cube.Mixin.Assembly;
using Cube.Mixin.Logging;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// UpdateChecker
    ///
    /// <summary>
    /// アップデートの確認を行うためのクラスです。
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
        /// <param name="src">設定用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public UpdateChecker(SettingFolder src)
        {
            var io  = src.IO;
            var dir = Assembly.GetExecutingAssembly().GetDirectoryName();

            FileName = io.Combine(dir, "CubeChecker.exe");
            Setting  = src;

            _timer.Interval = TimeSpan.FromDays(1);
            _timer.Subscribe(Synchronous.AsTask(WhenTick));

            if (Setting.Shared.CheckUpdate) Start();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Setting
        ///
        /// <summary>
        /// 設定用オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingFolder Setting { get; }

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

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// 定期実行を開始します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Start()
        {
            var time  = Setting.Shared.LastCheckUpdate ?? DateTime.MinValue;
            var past  = DateTime.Now - time;
            var delta = past < _timer.Interval ?
                        _timer.Interval - past :
                        TimeSpan.FromMilliseconds(100);
            _timer.Start(delta);
            this.LogDebug($"Start\tInterval:{_timer.Interval}\tInitialDelay:{delta}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// 定期実行を停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Stop() => _timer.Stop();

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
            try { Process.Start(FileName, "CubeRssReader"); }
            catch (Exception err) { this.LogWarn($"{FileName} ({err.Message})"); }
            finally { Setting.Shared.LastCheckUpdate = DateTime.Now; }
        }

        #endregion

        #region Fields
        private readonly WakeableTimer _timer = new WakeableTimer();
        #endregion
    }
}
