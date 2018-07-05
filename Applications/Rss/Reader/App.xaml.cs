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
using Cube.Forms.Processes;
using Cube.Log;
using Cube.Xui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// App
    ///
    /// <summary>
    /// メインプログラムを表すクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public partial class App : Application, IDisposable
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// App
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public App()
        {
            _dispose = new OnceAction<bool>(Dispose);
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// OnStartup
        ///
        /// <summary>
        /// 起動時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Activate()) return;

            Logger.Configure();
            Logger.Info(GetType(), Assembly.GetExecutingAssembly());

            _resources.Add(Logger.ObserveTaskException());
            _resources.Add(this.ObserveUiException());

            try
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 10;
                BrowserSettings.Version = BrowserVersion.Latest;
                BrowserSettings.MaxConnections = 10;
                BrowserSettings.NavigationSounds = false;
            }
            catch (Exception err) { this.LogWarn(err.ToString(), err); }

            base.OnStartup(e);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OnExit
        ///
        /// <summary>
        /// 終了時に実行されます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~App
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~App() { _dispose.Invoke(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを開放します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
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
        /// リソースを開放します。
        /// </summary>
        ///
        /// <param name="disposing">
        /// マネージオブジェクトを開放するかどうか
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var obj in _resources) obj.Dispose();
                _mutex?.Dispose();
            }
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Activate
        ///
        /// <summary>
        /// アクティブ化を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private bool Activate()
        {
            var asm  = Assembly.GetExecutingAssembly().GetReader();
            var name = $"{Environment.UserName}@{asm.Product}";
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
        private readonly IList<IDisposable> _resources = new List<IDisposable>();
        private Mutex _mutex;
        #endregion
    }
}
