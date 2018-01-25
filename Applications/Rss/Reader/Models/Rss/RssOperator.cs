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
    }
}
