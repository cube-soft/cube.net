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
using Cube.Mixin.Assembly;
using Cube.Mixin.Commands;
using Cube.Mixin.Drawing;
using Cube.Xui;
using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingViewModel
    ///
    /// <summary>
    /// 設定画面とモデルを関連付けるためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SettingViewModel : CommonViewModel
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SettingViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingViewModel(SettingFolder Setting) : base(new Aggregator())
        {
            Model  = Setting;
            Local  = new BindableValue<LocalSetting>(Setting.Value, Setting.Dispatcher);
            Shared = new BindableValue<SharedSetting>(Setting.Shared, Setting.Dispatcher);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Local
        ///
        /// <summary>
        /// ローカル設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<LocalSetting> Local { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Data
        ///
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<SharedSetting> Shared { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Logo
        ///
        /// <summary>
        /// アプリケーションのロゴ画像を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BitmapImage Logo => _logo ?? (
            _logo = Properties.Resources.Logo.ToBitmapImage()
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Product
        ///
        /// <summary>
        /// アプリケーション名を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Product => Model.Assembly.GetProduct();

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// バージョンを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Version => $"Version {Model.Version.ToString(true)}";

        /* ----------------------------------------------------------------- */
        ///
        /// Windows
        ///
        /// <summary>
        /// Windows のバージョンを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Windows => Environment.OSVersion.ToString();

        /* ----------------------------------------------------------------- */
        ///
        /// Framework
        ///
        /// <summary>
        /// .NET Framework のバージョンを表す文字列を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Framework => $"Microsoft .NET Framework {Environment.Version}";

        /* ----------------------------------------------------------------- */
        ///
        /// Copyright
        ///
        /// <summary>
        /// コピーライト表記を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Copyright { get; } = Assembly.GetExecutingAssembly().GetCopyright();

        /* ----------------------------------------------------------------- */
        ///
        /// Model
        ///
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private SettingFolder Model { get; }

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// SelectDataDirectory
        ///
        /// <summary>
        /// データディレクトリを選択するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand SelectDataDirectory => Get(() => new DelegateCommand(
            () => Send(MessageFactory.DataDirectory(Local.Value.DataDirectory))
        ));

        /* ----------------------------------------------------------------- */
        ///
        /// Apply
        ///
        /// <summary>
        /// 内容を適用するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Apply => Get(() => new DelegateCommand(() =>
        {
            Send<UpdateSourcesMessage>();
            Close.Execute();
        }));

        #endregion

        #region Fields
        private BitmapImage _logo;
        #endregion
    }
}
