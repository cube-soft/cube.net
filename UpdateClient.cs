/* ------------------------------------------------------------------------- */
///
/// UpdateClient.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Net.UpdateClient
    /// 
    /// <summary>
    /// アップデートを行うためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class UpdateClient
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// HttpHandler
        ///
        /// <summary>
        /// HTTP 通信時に追加的な処理を行うためのオブジェクトを取得
        /// または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public HttpMessageHandler HttpHandler { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// アプリケーションの現在のバージョンを設定または取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Version { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// RunAsync
        ///
        /// <summary>
        /// アップデートを非同期で実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Task RunAsync(Uri uri)
        {
            return Task.Factory.StartNew(() => { });
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetMessageAsync
        ///
        /// <summary>
        /// アップデートメッセージを非同期で取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<UpdateMessage> GetMessageAsync(Uri uri)
        {
            try
            {
                var response = await CreateHttpClient().GetAsync(uri);
                if (!response.IsSuccessStatusCode) return null;

                var encoding = System.Text.Encoding.GetEncoding("UTF-8"); // TODO: auto detect
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new System.IO.StreamReader(stream, encoding))
                foreach (var m in CreateMessages(reader))
                {
                    if (m.Version == Version) return m;
                }
            }
            catch (Exception err) { System.Diagnostics.Trace.TraceError(err.ToString()); }

            return null;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// CreateHttpClient
        ///
        /// <summary>
        /// HttpClient オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private HttpClient CreateHttpClient()
        {
            return HttpHandler != null ?
                   new HttpClient(HttpHandler) :
                   new HttpClient();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateMessages
        ///
        /// <summary>
        /// ストリームを解析して UpdateMessage オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private IList<UpdateMessage> CreateMessages(System.IO.StreamReader reader)
        {
            var dest = new List<UpdateMessage>();

            var version = string.Empty;
            var args = new Dictionary<string, string>();

            for (var l = reader.ReadLine(); !reader.EndOfStream; l = reader.ReadLine())
            {
                if (string.IsNullOrEmpty(l)) continue;

                if (l[0] == '[' && l[l.Length - 1] == ']')
                {
                    var m = CreateMessage(version, args);
                    if (m != null) dest.Add(m);
                    version = l.Substring(1, l.Length - 2);
                    args.Clear();
                }
                else Split(l, args);
            }

            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateMessage
        ///
        /// <summary>
        /// 指定されたバージョンと引数から UpdateMessage オブジェクトを
        /// 生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private UpdateMessage CreateMessage(string version, IDictionary<string, string> args)
        {
            if (string.IsNullOrEmpty(version) || args.Count <= 0) return null;

            try
            {
                var update = int.Parse(args["UPDATE"]);
                return new UpdateMessage
                {
                    Version = version,
                    Notify  = (update == 1),
                    Text    = args["MESSAGE"],
                    Uri     = new Uri(args["URL"])
                };
            }
            catch (Exception err) { System.Diagnostics.Trace.TraceError(err.ToString()); }

            return null;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Split
        ///
        /// <summary>
        /// 文字列を KeyValuePair に分割して追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Split(string src, IDictionary<string, string> dest)
        {
            if (string.IsNullOrEmpty(src)) return;

            var pos = src.IndexOf('=');
            if (pos < 0) return;

            var key = src.Substring(0, pos);
            var value = src.Substring(pos + 1, src.Length - (pos + 1));
            if (dest.ContainsKey(key)) dest[key] = value;
            else dest.Add(key, value);
        }

        #endregion
    }
}
