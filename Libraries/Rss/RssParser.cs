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

                return new RssFeed
                {
                    Id          = feed.Id,
                    Title       = feed.Title?.Text,
                    Description = feed.Description?.Text,
                    Links       = feed.Links?.Select(e => e.Uri),
                    Items       = feed.Items?.Select(e => Convert(e)),
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
        private static RssArticle Convert(SyndicationItem src) => new RssArticle
        {
            Id          = src.Id,
            Title       = src.Title?.Text,
            Summary     = src.Summary?.Text,
            Content     = GetContent(src),
            Categories  = src.Categories?.Select(x => x.Name),
            Links       = src.Links?.Select(e => e.Uri),
            PublishTime = src.PublishDate.LocalDateTime,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// GetContent
        /// 
        /// <summary>
        /// Content の内容を取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// content:encoded タグが存在する場合、その内容を優先します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private static string GetContent(SyndicationItem src)
        {
            foreach (var extension in src.ElementExtensions)
            {
                var element = extension.GetObject<XmlElement>();
                if (element.Name != "content:encoded") continue;
                return element.InnerText;
            }

            if (src.Content is TextSyndicationContent tsc) return tsc.Text;
            else return null;
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
