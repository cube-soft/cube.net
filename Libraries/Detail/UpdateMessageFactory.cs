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

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (IsCommand(line))
                    {
                        Add(version, args, dest);
                        version = line.Substring(1, line.Length - 2);
                        args.Clear();
                    }
                    else Split(line, args);
                }
            }

            Add(version, args, dest);
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// 指定されたバージョンと引数から UpdateMessage オブジェクトを
        /// 生成して追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void Add(string version, IDictionary<string, string> args, IList<UpdateMessage> dest)
        {
            try
            {
                if (string.IsNullOrEmpty(version) || args.Count <= 0) return;

                var update = int.Parse(args["UPDATE"]);
                dest.Add(new UpdateMessage
                {
                    Version = version,
                    Notify  = (update == 1),
                    Text    = args["MESSAGE"],
                    Uri     = new Uri(args["URL"])
                });
            }
            catch (Exception err) { LogError(err); }
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
            try
            {
                if (string.IsNullOrEmpty(src)) return;

                var pos = src.IndexOf('=');
                if (pos < 0) return;

                var key = src.Substring(0, pos);
                var value = src.Substring(pos + 1, src.Length - (pos + 1));
                if (dest.ContainsKey(key)) dest[key] = value;
                else dest.Add(key, value);
            }
            catch (Exception err) { LogError(err); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LogError
        ///
        /// <summary>
        /// 例外情報をログに出力します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static void LogError(Exception err)
            => Cube.Log.Operations.Error(typeof(UpdateMessageFactory), err.Message, err);

        /* ----------------------------------------------------------------- */
        ///
        /// IsCommand
        ///
        /// <summary>
        /// コマンドを表す文字列かどうかを判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private static bool IsCommand(string src)
            => !string.IsNullOrEmpty(src) && src[0] == '[' && src[src.Length - 1] == ']';
    }
}
