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
using Cube.Collections;
using Cube.FileSystem;
using Cube.Log;
using Cube.Net.Rss;
using Cube.Settings;

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
        /// Load
        ///
        /// <summary>
        /// ファイルから RSS エントリ情報を読み込みます。
        /// </summary>
        ///
        /// <param name="src">ファイルのパス</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /// <returns>RSS エントリ情報</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<RssCategory> Load(string src, Operator io)
        {
            var info = io.Get(src);
            if (info.Exists && info.Length > 0)
            {
                try
                {
                    using (var ss = io.OpenRead(src))
                    {
                        return SettingsType.Json
                                           .Load<List<RssCategory.Json>>(ss)
                                           .Select(e => e.Convert(null));
                    }
                }
                catch (Exception e) { LogOperator.Warn(typeof(RssOperator), e.ToString(), e); }
            }
            return new RssCategory[0];
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// RSS エントリ情報をファイルに保存します。
        /// </summary>
        ///
        /// <param name="src">RSS エントリ情報</param>
        /// <param name="dest">ファイルのパス</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Save(this IEnumerable<RssCategory> src, string dest, Operator io)
        {
            try
            {
                var json = src.Select(e => new RssCategory.Json(e));
                using (var ms = new System.IO.MemoryStream())
                {
                    SettingsType.Json.Save(ms, json);
                    using (var ss = io.Create(dest))
                    {
                        ms.Position = 0;
                        ms.CopyTo(ss);
                    }
                }
            }
            catch (Exception e) { LogOperator.Warn(typeof(RssOperator), e.ToString(), e); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        ///
        /// <summary>
        /// 指定されたオブジェクトの全ての記事を既読設定にします。
        /// </summary>
        ///
        /// <param name="src">RSS エントリまたはカテゴリ</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Read(this IRssEntry src)
        {
            if (src is RssCategory rc) ReadCore(rc);
            else if (src is RssEntry re) ReadCore(re);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Read
        ///
        /// <summary>
        /// 既読設定にします。
        /// </summary>
        ///
        /// <param name="src">RssEntry オブジェクト</param>
        /// <param name="item">RssItem オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Read(this RssEntry src, RssItem item)
        {
            if (src != null && item != null && item.Status != RssItemStatus.Read)
            {
                src.Count = Math.Max(src.UnreadItems.Count() - 1, 0);
                item.Status = RssItemStatus.Read;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Shrink
        ///
        /// <summary>
        /// 更新日時を基準として不要な項目を削除します。
        /// </summary>
        ///
        /// <param name="src">新着記事一覧</param>
        /// <param name="threshold">基準日時</param>
        ///
        /// <remarks>
        /// 新着記事一覧は発行日時で降順に並んでいる事を前提としています。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<RssItem> Shrink(this IEnumerable<RssItem> src, DateTime? threshold) =>
            src.Reverse()
               .SkipWhile(e => e.PublishTime <= threshold)
               .ToList();

        /* ----------------------------------------------------------------- */
        ///
        /// Shrink
        ///
        /// <summary>
        /// 既読記事を削除します。
        /// </summary>
        ///
        /// <param name="src">RSS フィード</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Shrink(this RssEntry src)
        {
            if (src == null || src.Items.Count <= 0) return;
            for (var i = src.Items.Count - 1; i >= 0; --i)
            {
                if (src.Items[i].Status == RssItemStatus.Read) src.Items.RemoveAt(i);
            }
        }

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
        /// Flatten
        ///
        /// <summary>
        /// 木構造の RSS エントリ一覧を一次元配列に変換します。
        /// </summary>
        ///
        /// <param name="src">木構造の RSS エントリ一覧</param>
        ///
        /// <returns>変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<IRssEntry> Flatten(this IEnumerable<IRssEntry> src) =>
            src.Flatten(e => (e is RssCategory c) ? c.Children : null);

        /* ----------------------------------------------------------------- */
        ///
        /// Flatten
        ///
        /// <summary>
        /// 木構造の RSS エントリ一覧を一次元配列に変換します。
        /// </summary>
        ///
        /// <param name="src">木構造の RSS エントリ一覧</param>
        ///
        /// <returns>変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IRssEntry> src) =>
            src.Flatten().OfType<T>();


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
            (!src.LastChecked.HasValue || now - src.LastPublished <= _border);

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
            src.LastChecked.HasValue && now - src.LastPublished > _border;

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
        public static Exception ToException(this string src) => new ArgumentException(src);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ReadCore
        ///
        /// <summary>
        /// カテゴリ中の全記事を既読設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void ReadCore(RssCategory src)
        {
            foreach (var item in src.Children) Read(item);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ReadCore
        ///
        /// <summary>
        /// RSS エントリ中の全記事を既読設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void ReadCore(RssEntry src)
        {
            foreach (var item in src.UnreadItems) item.Status = RssItemStatus.Read;
            src.Count = 0;
        }

        #endregion

        #region Fields
        private static readonly TimeSpan _border = TimeSpan.FromDays(30);
        #endregion
    }
}
