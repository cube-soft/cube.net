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
using System.Linq;
using Cube.Net.Rss;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssOperator
    ///
    /// <summary>
    /// RSS エントリに関する拡張用クラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class RssOperator
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Expand
        /// 
        /// <summary>
        /// RSS カテゴリの子要素が表示された状態に設定します。
        /// </summary>
        /// 
        /// <param name="src">カテゴリ</param>
        /// 
        /* ----------------------------------------------------------------- */
        public static void Expand(this IRssEntry src)
        {
            while (src is RssCategory category)
            {
                category.Expanded = true;
                src = category.Parent;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        /// 
        /// <summary>
        /// RssMonitor を再設定します。
        /// </summary>
        /// 
        /// <param name="mon">モニタ</param>
        /// <param name="feeds">対象 RSS フィード一覧</param>
        /// <param name="pred">叙述関数</param>
        /// 
        /// <remarks>
        /// RssEntry に変換できないオブジェクトは無視されます。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        public static void Reschedule(this RssMonitor mon,
            IEnumerable<RssFeed> feeds, Func<RssEntry, bool> pred) =>
            Reschedule(mon, feeds.OfType<RssEntry>(), pred);

        /* ----------------------------------------------------------------- */
        ///
        /// Reschedule
        /// 
        /// <summary>
        /// RssMonitor を再設定します。
        /// </summary>
        /// 
        /// <param name="mon">モニタ</param>
        /// <param name="entries">対象 RSS エントリ一覧</param>
        /// <param name="pred">叙述関数</param>
        /// 
        /* ----------------------------------------------------------------- */
        public static void Reschedule(this RssMonitor mon,
            IEnumerable<RssEntry> entries, Func<RssEntry, bool> pred)
        {
            try
            {
                mon.Suspend();
                mon.Clear();

                var none  = RssCheckFrequency.None;
                var items = entries.Where(e => e.Frequency != none && pred(e));

                foreach (var e in items) mon.Register(e.Uri, e.LastChecked);
            }
            finally { mon.Start(); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// IsHighFrequency
        /// 
        /// <summary>
        /// チェック間隔が高頻度に該当するかどうかを判別します。
        /// </summary>
        /// 
        /// <param name="src">RSS エントリ</param>
        /// <param name="now">基準時刻</param>
        /// 
        /// <returns>高頻度かどうか</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public static bool IsHighFrequency(this RssEntry src, DateTime now) =>
            src.Frequency == RssCheckFrequency.High ||
            src.Frequency == RssCheckFrequency.Auto &&
            now - src.LastPublished <= _border;

        /* ----------------------------------------------------------------- */
        ///
        /// IsLowFrequency
        /// 
        /// <summary>
        /// チェック間隔が低頻度に該当するかどうかを判別します。
        /// </summary>
        /// 
        /// <param name="src">RSS エントリ</param>
        /// <param name="now">基準時刻</param>
        /// 
        /// <returns>低頻度かどうか</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public static bool IsLowFrequency(this RssEntry src, DateTime now) =>
            src.Frequency == RssCheckFrequency.Low ||
            src.Frequency == RssCheckFrequency.Auto &&
            now - src.LastPublished <= _border;

        /* ----------------------------------------------------------------- */
        ///
        /// ToMessage
        /// 
        /// <summary>
        /// Frequency オブジェクトを表すメッセージを取得します。
        /// </summary>
        /// 
        /// <param name="src">Frequency オブジェクト</param>
        /// 
        /// <returns>メッセージ</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string ToMessage(this RssCheckFrequency src)
        {
            switch (src)
            {
                case RssCheckFrequency.Auto: return Properties.Resources.MessageAutoFrequency;
                case RssCheckFrequency.High: return Properties.Resources.MessageHighFrequency;
                case RssCheckFrequency.Low:  return Properties.Resources.MessageLowFrequency;
                case RssCheckFrequency.None: return Properties.Resources.MessageNoneFrequency;
            }
            return string.Empty;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ToException
        /// 
        /// <summary>
        /// 例外オブジェクトに変換します。
        /// </summary>
        /// 
        /// <param name="src">メッセージ</param>
        /// 
        /// <returns>例外オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Exception ToException(this string src) =>
            new ArgumentException(src);

        #endregion

        #region Fields
        private static readonly TimeSpan _border = TimeSpan.FromDays(30);
        #endregion
    }
}
