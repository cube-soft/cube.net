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
using Cube.Log;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpExtension
    ///
    /// <summary>
    /// HTTP 通信に関する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class HttpExtension
    {
        /* ----------------------------------------------------------------- */
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
        /// <remarks>
        /// 例外の扱いについては、IContentConverter(T).IgnoreException の
        /// 設定に準じます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static async Task<T> GetAsync<T>(this HttpClient client,
            Uri uri, IContentConverter<T> converter)
        {
            try
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        await response.Content.LoadIntoBufferAsync();
                        var stream = await response.Content.ReadAsStreamAsync();
                        return converter.Convert(stream);
                    }
                    else client.LogWarn($"StatusCode:{response.StatusCode}");
                }
            }
            catch (Exception err)
            {
                if (converter.IgnoreException) client.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(T);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync(T)
        ///
        /// <summary>
        /// HTTP 通信を実行し、変換結果を取得します。
        /// </summary>
        ///
        /// <param name="client">HTTP クライアント</param>
        /// <param name="uri">レスポンス取得 URL</param>
        /// <param name="func">変換用の関数オブジェクト</param>
        ///
        /// <returns>変換結果</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetAsync<T>(this HttpClient client,
            Uri uri, Func<Stream, T> func) =>
            client.GetAsync(uri, new ContentConverter<T>(func));
    }
}
