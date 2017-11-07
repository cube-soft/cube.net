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
using Cube.Net.Http;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssMonitor
    ///
    /// <summary>
    /// 定期的に登録した URL からフィードを取得するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssMonitor : HttpMonitorBase<RssFeed>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RssMonitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor() : this(new RssContentConverter()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Monitor
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="converter">変換用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public RssMonitor(IContentConverter<RssFeed> converter)
            : base(new ContentHandler<RssFeed>(converter) { UseEntityTag = false }) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Uri
        ///
        /// <summary>
        /// リクエスト送信先 URL 一覧を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public IList<Uri> Uris { get; } = new List<Uri>();

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// GetRequestUris
        ///
        /// <summary>
        /// リクエスト送信先 URL 一覧を取得します。
        /// </summary>
        /// 
        /// <returns>URL 一覧</returns>
        /// 
        /* ----------------------------------------------------------------- */
        protected override IEnumerable<Uri> GetRequestUris() => Uris;

        #endregion
    }
}
