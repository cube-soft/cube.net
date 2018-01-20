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
using System.Collections.Generic;
using Cube.Net.Rss;
using Cube.Xui;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssBindableData
    ///
    /// <summary>
    /// メイン画面にバインドされるデータ群を定義したクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssBindableData
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
        ///
        /* ----------------------------------------------------------------- */
        public RssBindableData(IEnumerable<RssEntryBase> root, Settings settings)
        {
            Root = root;
            User = new Bindable<Settings>(settings);
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
        public IEnumerable<RssEntryBase> Root { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Settings
        /// 
        /// <summary>
        /// ユーザ設定を保持するオブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<Settings> User { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Entry
        /// 
        /// <summary>
        /// 選択中の Web サイトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssEntryBase> Entry { get; } = new Bindable<RssEntryBase>();

        /* ----------------------------------------------------------------- */
        ///
        /// Feed
        /// 
        /// <summary>
        /// 選択中の Web サイトの RSS フィードを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssFeed> Feed { get; } = new Bindable<RssFeed>();

        /* ----------------------------------------------------------------- */
        ///
        /// Article
        /// 
        /// <summary>
        /// 選択中の記事を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<RssItem> Article { get; } = new Bindable<RssItem>();

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        /// 
        /// <summary>
        /// Web ブラウザに表示する URL を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// このプロパティは通常時には使用されません。RSS フィードとは
        /// 関係のない Web ページを表示する際に設定されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<Uri> Uri { get; } = new Bindable<Uri>();

        /* ----------------------------------------------------------------- */
        ///
        /// Message
        /// 
        /// <summary>
        /// メッセージを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Message { get; } = new Bindable<string>();

        #endregion
    }
}
