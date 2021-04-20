﻿/* ------------------------------------------------------------------------- */
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
using System.Collections.Generic;
using Cube.Xui;

namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// MainBindableData
    ///
    /// <summary>
    /// メイン画面にバインドされるデータ群を定義したクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class MainBindableData
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssBindableData
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="root">ルートオブジェクト</param>
        /// <param name="settings">設定用オブジェクト</param>
        /// <param name="dispatcher">同期用コンテキスト</param>
        ///
        /* ----------------------------------------------------------------- */
        public MainBindableData(IEnumerable<IRssEntry> root, SettingFolder settings, Dispatcher dispatcher)
        {
            Root       = root;
            Lock       = new BindableValue<LockSetting>(settings.Lock, dispatcher);
            Local      = new BindableValue<LocalSetting>(settings.Value, dispatcher);
            Shared     = new BindableValue<SharedSetting>(settings.Shared, dispatcher);
            Current    = new BindableValue<IRssEntry>(dispatcher);
            LastEntry  = new BindableValue<RssEntry>(dispatcher);
            Content    = new BindableValue<object>(dispatcher);
            Message    = new BindableValue<string>(dispatcher);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Root
        ///
        /// <summary>
        /// 登録されている RssEntry のルートにあたるオブジェクトを
        /// 取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerable<IRssEntry> Root { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Lock
        ///
        /// <summary>
        /// ロック情報を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<LockSetting> Lock { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Local
        ///
        /// <summary>
        /// ローカル設定を保持するオブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<LocalSetting> Local { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Shared
        ///
        /// <summary>
        /// ユーザ設定を保持するオブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<SharedSetting> Shared { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Current
        ///
        /// <summary>
        /// 選択中のカテゴリまたは RSS エントリを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<IRssEntry> Current { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// LastEntry
        ///
        /// <summary>
        /// 最後に選択した RSS エントリを取得します。
        /// </summary>
        ///
        /// <remarks>
        /// RSS エントリを選択中の場合、Current と LastEntry は同じ値に
        /// なります。カテゴリを選択中の場合、Current は該当カテゴリの
        /// 値を LastEntry は直前まで選択されていた RssEntry の値を保持
        /// します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<RssEntry> LastEntry { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Content
        ///
        /// <summary>
        /// Web ブラウザに表示する内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<object> Content { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Message
        ///
        /// <summary>
        /// メッセージを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public BindableValue<string> Message { get; }

        #endregion
    }
}
