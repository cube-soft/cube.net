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
using Cube.Collections.Mixin;
using Cube.DataContract;
using Cube.FileSystem;
using Cube.FileSystem.Mixin;
using Cube.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cube.Net.Rss.App.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssExtension
    ///
    /// <summary>
    /// RSS エントリに関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class RssExtension
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Backup
        ///
        /// <summary>
        /// ファイルのバックアップを作成します。
        /// </summary>
        ///
        /// <param name="src">ファイルのパス</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public static void Backup(string src, IO io)
        {
            var info = io.Get(src);
            var dir  = io.Combine(info.DirectoryName, "Backup");
            var dest = io.Combine(dir, $"{DateTime.Now.ToString("yyyyMMdd")}{info.Extension}");

            if (io.Exists(dest)) return;
            io.Copy(src, dest, true);

            foreach (var f in io.GetFiles(dir).OrderByDescending(e => e).Skip(30))
            {
                Logger.Warn(typeof(RssExtension), () => io.Delete(f));
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// ファイルから RSS エントリ情報を読み込みます。
        /// </summary>
        ///
        /// <param name="src">ファイルのパス</param>
        /// <param name="context">同期用コンテキスト</param>
        /// <param name="io">入出力用オブジェクト</param>
        ///
        /// <returns>RSS エントリ情報</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<RssCategory> Load(string src,
            SynchronizationContext context, IO io) =>
            io.Exists(src) ?
            io.Load(
                src,
                ss => Format.Json
                    .Deserialize<List<RssCategory.Json>>(ss)
                    .Select(e => e.Convert(context))
            ) :
            new RssCategory[0];

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
        public static void Save(this IEnumerable<RssCategory> src, string dest, IO io) =>
            io.Save(
                dest,
                ms => Format.Json.Serialize(ms, src.Select(e => new RssCategory.Json(e)))
            );

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
            if (item != null && item.Status != RssItemStatus.Read)
            {
                src.Count = Math.Max(src.UnreadItems.Count() - 1, 0);
                item.Status = RssItemStatus.Read;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Update
        ///
        /// <summary>
        /// RSS の内容を更新します。
        /// </summary>
        ///
        /// <param name="dest">更新先オブジェクト</param>
        /// <param name="src">更新内容</param>
        ///
        /// <returns>新着記事数</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static int Update(this RssEntry dest, RssFeed src)
        {
            var threshold = dest.LastChecked;
            dest.LastChecked = src.LastChecked;

            if (src.Error != null)
            {
                dest.LogDebug($"{dest.Uri} ({src.Error.GetType().Name})");
                src.Title = dest.Title;
                return 0;
            }

            var received = src.Items.Shrink(threshold).ToList();
            foreach (var item in received) dest.Items.Insert(0, item);

            dest.Description   = src.Description;
            dest.Count         = dest.UnreadItems.Count();
            dest.Link          = src.Link;
            dest.LastPublished = src.LastPublished;

            return received.Count;
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
               .SkipWhile(e => e.PublishTime <= threshold);

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
            var current = src;
            while (current is RssCategory category)
            {
                category.Expanded = true;
                current = category.Parent;
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
        /// <param name="src">RSS エントリ</param>
        ///
        /// <returns>変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<IRssEntry> Flatten(this IRssEntry src) =>
            new[] { src }.Flatten();

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
        /// Flatten
        ///
        /// <summary>
        /// 木構造の RSS エントリ一覧を一次元配列に変換します。
        /// </summary>
        ///
        /// <param name="src">RSS エントリ</param>
        ///
        /// <returns>変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static IEnumerable<T> Flatten<T>(this IRssEntry src) =>
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
            (!src.LastChecked.HasValue || now - src.LastPublished <= TimeSpan.FromDays(30));

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
            src.Frequency != RssCheckFrequency.None && !IsHighFrequency(src, now);

        /* ----------------------------------------------------------------- */
        ///
        /// ToMessage
        ///
        /// <summary>
        /// RSS フィードの結果を伝えるメッセージを取得します
        /// </summary>
        ///
        /// <param name="src">RSS フィード</param>
        /// <param name="count">新着記事数</param>
        ///
        /// <returns>メッセージ</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static string ToMessage(this RssFeed src, int count) =>
            src.Error != null ? string.Format(Properties.Resources.ErrorFeed, src.Title) :
            count > 0         ? string.Format(Properties.Resources.MessageReceived, count, src.Title) :
                                string.Format(Properties.Resources.MessageNoReceived, src.Title);

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
        public static string ToMessage(this RssCheckFrequency src) =>
            new Dictionary<RssCheckFrequency, string>
            {
                { RssCheckFrequency.Auto, Properties.Resources.MessageAutoFrequency },
                { RssCheckFrequency.High, Properties.Resources.MessageHighFrequency },
                { RssCheckFrequency.Low,  Properties.Resources.MessageLowFrequency  },
                { RssCheckFrequency.None, Properties.Resources.MessageNoneFrequency },
            }[src];

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
    }
}
