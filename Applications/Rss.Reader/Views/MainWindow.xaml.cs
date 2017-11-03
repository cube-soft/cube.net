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
using System.Windows;
using System.Windows.Controls;

namespace Cube.Net.Applications.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainWindow
    ///
    /// <summary>
    /// メイン画面を表すクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public partial class MainWindow : Window
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// MainWindow
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// WhenToolBarLoaded
        /// 
        /// <summary>
        /// ToolBar ロード時に実行されるハンドラです。
        /// </summary>
        /// 
        /// <remarks>
        /// OverflowGrid を非表示にします。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenToolBarLoaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToolBar;
            foreach (FrameworkElement item in tb.Items) ToolBar.SetOverflowMode(item, OverflowMode.Never);
            if (tb.Template.FindName("OverflowGrid", tb) is FrameworkElement grid)
            {
                grid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
