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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Cube.Net.App.Rss.Reader
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
    public partial class App : Application
    {
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

            LogOperator.Configure();
            LogOperator.ObserveTaskException();
            LogOperator.Info(GetType(), Assembly.GetExecutingAssembly());

            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
            BrowserSettings.Version = BrowserVersion.Latest;
            BrowserSettings.MaxConnections = 10;
            BrowserSettings.NavigationSounds = false;

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
            if (_mutex != null) _mutex.ReleaseMutex();
            base.OnExit(e);
        }

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
            var name = AssemblyReader.Default.Product;
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
        private Mutex _mutex;
        #endregion
    }
}
