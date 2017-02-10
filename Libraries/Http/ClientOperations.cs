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
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// ClientOperations
    ///
    /// <summary>
    /// System.Net.Http.HttpClient の拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class ClientOperations
    {
        /* --------------------------------------------------------------------- */
        ///
        /// GetJsonAsync
        /// 
        /// <summary>
        /// JSON 形式のデータを非同期で取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, Uri uri) where T : class
        {
            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                client.LogError($"StatusCode:{response.StatusCode}");
                return null;
            }

            try
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var json = new DataContractJsonSerializer(typeof(T));
                return json.ReadObject(stream) as T;
            }
            catch (Exception err) { await client.LogError(response, err); }

            return null;
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetXmlAsync
        /// 
        /// <summary>
        /// XML 形式のデータを非同期で取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static async Task<T> GetXmlAsync<T>(this HttpClient client, Uri uri) where T : class
        {
            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                client.LogError($"StatusCode:{response.StatusCode}");
                return null;
            }

            try
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var xml = new XmlSerializer(typeof(T));
                return xml.Deserialize(stream) as T;
            }
            catch (Exception err) { await client.LogError(response, err); }

            return null;
        }

        #region Other private methods

        /* --------------------------------------------------------------------- */
        ///
        /// LogError
        /// 
        /// <summary>
        /// エラーログを出力します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        private static async Task LogError(this HttpClient client, HttpResponseMessage response, Exception err)
        {
            var content = await response.Content.ReadAsStringAsync();
            client.LogError(err.Message, err);
            client.LogError(content);
        }

        #endregion
    }
}
