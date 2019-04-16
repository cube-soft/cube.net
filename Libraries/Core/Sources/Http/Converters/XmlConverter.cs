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
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// XmlContentConverter(TValue)
    ///
    /// <summary>
    /// XML 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class XmlContentConverter<TValue> : ContentConverter<TValue> where TValue : class
    {
        /* ----------------------------------------------------------------- */
        ///
        /// XmlContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public XmlContentConverter() : base(s =>
        {
            if (s == null) return default(TValue);
            var xml = new XmlSerializer(typeof(TValue));
            return xml.Deserialize(s) as TValue;
        }) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// XmlOperations
    ///
    /// <summary>
    /// XML に関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class XmlOperations
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetXmlAsync
        ///
        /// <summary>
        /// XML 形式のデータを非同期で取得します。
        /// </summary>
        ///
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        ///
        /// <returns>XML 形式データの変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetXmlAsync<T>(this HttpClient client, Uri uri)
            where T : class => client.GetAsync(uri, new XmlContentConverter<T>());
    }
}
