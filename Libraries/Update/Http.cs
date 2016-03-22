/* ------------------------------------------------------------------------- */
///
/// Http.cs
///
/// Copyright (c) 2010 CubeSoft, Inc.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as published
/// by the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cube.Conversions;

namespace Cube.Net.Update
{
    /* --------------------------------------------------------------------- */
    ///
    /// Http
    ///
    /// <summary>
    /// System.Net.Http.HttpClient の拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class Http
    {
        /* --------------------------------------------------------------------- */
        ///
        /// GetUpdateMessageAsync
        ///
        /// <summary>
        /// 対象とするバージョンのアップデートに関するメッセージを
        /// 非同期で取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static Task<Message> GetUpdateMessageAsync(this HttpClient client,
            Uri uri, string version)
        {
            var query = new Dictionary<string, string>();
            return GetUpdateMessageAsync(client, uri, version, query);
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetUpdateMessageAsync
        ///
        /// <summary>
        /// 対象とするバージョンのアップデートに関するメッセージを
        /// 非同期で取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static Task<Message> GetUpdateMessageAsync(this HttpClient client,
            Uri uri, string version, string key, string value)
        {
            var query = new Dictionary<string, string>();
            query.Add(key, value);
            return GetUpdateMessageAsync(client, uri, version, query);
        }

        /* --------------------------------------------------------------------- */
        ///
        /// GetUpdateMessageAsync
        ///
        /// <summary>
        /// 対象とするバージョンのアップデートに関するメッセージを
        /// 非同期で取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        public static async Task<Message> GetUpdateMessageAsync(this HttpClient client,
            Uri uri, string version, IDictionary<string, string> query)
        {
            var request = uri.With("ver", version).With(query);
            var response = await client.GetAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var stream = await response.Content.ReadAsStreamAsync();
            var items  = MessageFactory.Create(stream);
            if (items == null) return null;

            foreach (var item in items)
            {
                if (item.Version == version) return item;
            }
            return null;
        }
    }
}
