/* ------------------------------------------------------------------------- */
///
/// Article.cs
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
using System.Runtime.Serialization;

namespace Cube.Net.News
{
    /* --------------------------------------------------------------------- */
    ///
    /// Article
    /// 
    /// <summary>
    /// ニュース記事の情報を保持するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [DataContract]
    public class Article
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Title
        ///
        /// ニュース記事のタイトルを取得または設定します。
        ///
        /* ----------------------------------------------------------------- */
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Url
        ///
        /// <summary>
        /// ニュース記事本文への URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [DataMember(Name = "url")]
        public string Url { get; set; }

        #endregion

        #region Implementation for IEquatable<NewsItem>

        /* ----------------------------------------------------------------- */
        /// Equals
        /* ----------------------------------------------------------------- */
        public bool Equals(Article other)
            => Title == other.Title && Url.Equals(other.Url);

        /* ----------------------------------------------------------------- */
        /// Equals
        /* ----------------------------------------------------------------- */
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var other = obj as Article;
            if (other == null) return false;

            return Equals(other);
        }

        /* ----------------------------------------------------------------- */
        /// GetHashCode
        /* ----------------------------------------------------------------- */
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
