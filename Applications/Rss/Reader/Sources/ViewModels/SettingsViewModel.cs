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
using Cube.Xui;
using Cube.Xui.Converters;
using Cube.Xui.Mixin;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingsViewModel
    ///
    /// <summary>
    /// 設定画面とモデルを関連付けるためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SettingsViewModel : CommonViewModel
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsViewModel
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsViewModel(SettingsFolder settings) : base(new Messenger())
        {
            Model  = settings;
            Local  = settings.Value.ToBindable();
            Shared = settings.Shared.ToBindable();
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
        public Bindable<LocalSettings> Local { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Data
        ///
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<SharedSettings> Shared { get; }

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
        public string Product => Model.Assembly.Product;

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
        public string Copyright { get; } = Assembly.GetExecutingAssembly().GetReader().Copyright;

        /* ----------------------------------------------------------------- */
        ///
        /// Model
        ///
        /// <summary>
        /// ユーザ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private SettingsFolder Model { get; }

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
        public ICommand SelectDataDirectory => _selectDataDirectory ?? (
            _selectDataDirectory = new RelayCommand(
                () => Send(MessageFactory.DataDirectory(Local.Value.DataDirectory))
            )
        );

        /* ----------------------------------------------------------------- */
        ///
        /// Apply
        ///
        /// <summary>
        /// 内容を適用するコマンドです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Apply => _apply ?? (
            _apply = new RelayCommand(() =>
            {
                Send<UpdateSourcesMessage>();
                Close.Execute(null);
            })
        );

        #endregion

        #region Fields
        private BitmapImage _logo;
        private ICommand _apply;
        private ICommand _selectDataDirectory;
        #endregion
    }
}
