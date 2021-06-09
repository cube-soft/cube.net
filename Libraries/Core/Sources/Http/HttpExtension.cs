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
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cube.Mixin.Logging;
using Cube.Net.Http;

namespace Cube.Mixin.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// HttpExtension
    ///
    /// <summary>
    /// Provides extended methods of the HttpClient class.
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
        /// Gets the result of HTTP transmission with the specified
        /// conversion.
        /// </summary>
        ///
        /// <param name="client">HTTP client.</param>
        /// <param name="uri">Request URL.</param>
        /// <param name="converter">Converter object.</param>
        ///
        /// <returns>HTTP transmission and conversion result.</returns>
        ///
        /// <remarks>
        /// If an exception is thrown, the method will follow the settings
        /// of IContentConverter.IgnoreException property.
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public static async Task<T> GetAsync<T>(this HttpClient client,
            System.Uri uri,
            IContentConverter<T> converter
        ) {
            try
            {
                using var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    await response.Content.LoadIntoBufferAsync();
                    var stream = await response.Content.ReadAsStreamAsync();
                    return converter.Convert(stream);
                }
                else client.GetType().LogWarn($"StatusCode:{response.StatusCode}");
            }
            catch (Exception err)
            {
                if (converter.IgnoreException) client.GetType().LogWarn(err);
                else throw;
            }
            return default;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAsync(T)
        ///
        /// <summary>
        /// Gets the result of HTTP transmission with the specified
        /// function.
        /// </summary>
        ///
        /// <param name="client">HTTP client.</param>
        /// <param name="uri">Request URL.</param>
        /// <param name="func">Function to convert.</param>
        ///
        /// <returns>HTTP transmission and conversion result.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetAsync<T>(this HttpClient client, System.Uri uri, Func<Stream, T> func) =>
            client.GetAsync(uri, new ContentConverter<T>(func));

        /* ----------------------------------------------------------------- */
        ///
        /// GetJsonAsync
        ///
        /// <summary>
        /// Gets data in JSON format asynchronously.
        /// </summary>
        ///
        /// <param name="client">HTTP client.</param>
        /// <param name="uri">Request URL.</param>
        ///
        /// <returns>Result in JSON format.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetJsonAsync<T>(this HttpClient client, System.Uri uri) where T : class =>
            client.GetAsync(uri, new JsonContentConverter<T>());

        /* ----------------------------------------------------------------- */
        ///
        /// GetXmlAsync
        ///
        /// <summary>
        /// Gets data in XML format asynchronously.
        /// </summary>
        ///
        /// <param name="client">HTTP client.</param>
        /// <param name="uri">Request URL.</param>
        ///
        /// <returns>Result in XML format.</returns>
        ///
        /* ----------------------------------------------------------------- */
        public static Task<T> GetXmlAsync<T>(this HttpClient client, System.Uri uri) where T : class =>
            client.GetAsync(uri, new XmlContentConverter<T>());
    }
}
