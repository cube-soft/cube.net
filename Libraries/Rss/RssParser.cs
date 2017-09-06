/* ------------------------------------------------------------------------- */
///
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System.Linq;
using System.Xml;
using System.ServiceModel.Syndication;

namespace Cube.Net.Rss
{
    /* --------------------------------------------------------------------- */
    ///
    /// RssParser
    ///
    /// <summary>
    /// RSS および Atom 情報を解析するクラスです。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public static class RssParser
    {
        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        /// 
        /// <summary>
        /// ストリームから RSS オブジェクトを生成します。
        /// </summary>
        /// 
        /// <param name="stream">入力ストリーム</param>
        /// 
        /// <returns>RSS オブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static RssFeed Create(System.IO.Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                var feed = SyndicationFeed.Load(reader);

                var count = feed.Links.Count;
                if (count > 1) LogWarn($"Feed.Links.Count:{count}");

                return new RssFeed
                {
                    Id          = feed.Id,
                    Title       = feed.Title?.Text,
                    Description = feed.Description?.Text,
                    Link        = feed.Links?.FirstOrDefault()?.Uri,
                    Items       = feed.Items?.Select(x => Convert(x)),
                };
            }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Convert
        /// 
        /// <summary>
        /// SyndicationItem を RssItem に変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static RssItem Convert(SyndicationItem src)
        {
            var count = src.Links?.Count ?? 0;
            if (count > 1) LogWarn($"Item.Links.Count:{count}\tId:{src.Id}");

            return new RssItem
            {
                Id          = src.Id,
                Title       = src.Title?.Text,
                Summary     = src.Summary?.Text,
                Categories  = src.Categories?.Select(x => x.Name),
                Link        = src.Links?.FirstOrDefault()?.Uri,
                PublishTime = src.PublishDate.LocalDateTime,
            };
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LogWarn
        /// 
        /// <summary>
        /// ログに出力します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void LogWarn(string message)
            => Cube.Log.Operations.Warn(typeof(RssParser), message);

        #endregion
    }
}
