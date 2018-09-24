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
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// JsonContentConverter(TValue)
    ///
    /// <summary>
    /// JSON 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class JsonContentConverter<TValue> : ContentConverter<TValue> where TValue : class
    {
        /* ----------------------------------------------------------------- */
        ///
        /// JsonContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public JsonContentConverter() : base(s =>
        {
            if (s == null) return default(TValue);
            var json = new DataContractJsonSerializer(typeof(TValue));
            return json.ReadObject(s) as TValue;
        }) { }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// JsonOperations
    ///
    /// <summary>
    /// JSON に関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class JsonOperations
    {
        /* ----------------------------------------------------------------- */
        ///
        /// GetJsonAsync
        ///
        /// <summary>
        /// JSON 形式のデータを非同期で取得します。
        /// </summary>
        ///
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        ///
        /// <returns>JSON 形式データの変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetJsonAsync<T>(this HttpClient client, Uri uri)
            where T : class => client.GetAsync(uri, new JsonContentConverter<T>());
    }
}
