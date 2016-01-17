/* ------------------------------------------------------------------------- */
///
/// UpdateMessageFactory.cs
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
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Cube.Net
{
    /* --------------------------------------------------------------------- */
    ///
    /// UpdateMessageFactory
    ///
    /// <summary>
    /// UpdateMessage オブジェクトを生成するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class UpdateMessageFactory
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// ストリームを解析して UpdateMessage オブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public static IList<UpdateMessage> Create(Stream stream)
        {
            var dest = new List<UpdateMessage>();

            var version = string.Empty;
            var args = new Dictionary<string, string>();

            // TODO: auto detect encoding
            using (var reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    var m = Create(version, args);
                    if (m != null) dest.Add(m);
                    version = line.Substring(1, line.Length - 2);
                    args.Clear();
                }
                else Split(line, args);
            }

            if (!string.IsNullOrEmpty(version) && args.Count > 0)
            {
                var m = Create(version, args);
                if (m != null) dest.Add(m);
            }
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// 指定されたバージョンと引数から UpdateMessage オブジェクトを
        /// 生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static UpdateMessage Create(string version, IDictionary<string, string> args)
        {
            if (string.IsNullOrEmpty(version) || args.Count <= 0) return null;

            try
            {
                var update = int.Parse(args["UPDATE"]);
                return new UpdateMessage
                {
                    Version = version,
                    Notify = (update == 1),
                    Text = args["MESSAGE"],
                    Uri = new Uri(args["URL"])
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
        private static void Split(string src, IDictionary<string, string> dest)
        {
            if (string.IsNullOrEmpty(src)) return;

            var pos = src.IndexOf('=');
            if (pos < 0) return;

            var key = src.Substring(0, pos);
            var value = src.Substring(pos + 1, src.Length - (pos + 1));
            if (dest.ContainsKey(key)) dest[key] = value;
            else dest.Add(key, value);
        }
    }
}
