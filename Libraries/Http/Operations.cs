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
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// Net.Http.Operations
    ///
    /// <summary>
    /// HTTP 通信に関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class Operations
    {
        #region HttpClient

        /* --------------------------------------------------------------------- */
        ///
        /// GetAsync(T)
        /// 
        /// <summary>
        /// HTTP 通信を実行し、変換結果を取得します。
        /// </summary>
        /// 
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        /// <param name="converter">変換用オブジェクト</param>
        /// 
        /// <returns>変換結果</returns>
        ///
        /* --------------------------------------------------------------------- */
        public static async Task<T> GetAsync<T>(this HttpClient client,
            Uri uri, IContentConverter<T> converter) where T : class
        {
            using (var response = await client?.GetAsync(uri))
            {
                if (response == null) return null;
                else if (!response.IsSuccessStatusCode)
                {
                    client.LogWarn($"StatusCode:{response.StatusCode}");
                    return null;
                }

                try { return await converter.ConvertAsync(response.Content); }
                catch (Exception err)
                {
                    client.LogWarn(err.ToString());
                    if (response?.Content != null)
                    {
                        client.LogWarn(await response.Content.ReadAsStringAsync());
                    }
                    return null;
                }
            }
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetAsync(T)
        /// 
        /// <summary>
        /// HTTP 通信を実行し、変換結果を取得します。
        /// </summary>
        /// 
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        /// <param name="converter">変換用の関数オブジェクト</param>
        /// 
        /// <returns>変換結果</returns>
        ///
        /* --------------------------------------------------------------------- */
        public static Task<T> GetAsync<T>(this HttpClient client,
            Uri uri, Func<HttpContent, Task<T>> func) where T : class
            => client.GetAsync(uri, new ContentConverter<T>(func));

        /* --------------------------------------------------------------------- */
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
        /* --------------------------------------------------------------------- */
        public static Task<T> GetJsonAsync<T>(this HttpClient client, Uri uri) where T : class
            => client.GetAsync(uri, new JsonContentConverter<T>());

        /* --------------------------------------------------------------------- */
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
        /* --------------------------------------------------------------------- */
        public static Task<T> GetXmlAsync<T>(this HttpClient client, Uri uri) where T : class
            => client.GetAsync(uri, new XmlContentConverter<T>());

        #endregion
    }
}
