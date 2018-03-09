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
using System.Runtime.CompilerServices;

namespace Cube.Net.App.Rss.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// FileHelper
    ///
    /// <summary>
    /// 各種テストファイルを扱う際の補助を行うクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class FileHelper : Cube.Net.Tests.FileHelper
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Copy
        ///
        /// <summary>
        /// 必要なテストファイルをコピーします。
        /// </summary>
        ///
        /// <param name="name">ディレクトリ名</param>
        ///
        /// <returns>コピー先のルートディレクトリ</returns>
        ///
        /* ----------------------------------------------------------------- */
        public string Copy([CallerMemberName] string name = null)
        {
            var dest = Result(name);

            IO.Copy(Example("Sample.json"),   IO.Combine(dest, "Feeds.json"),    true);
            IO.Copy(Example("Settings.json"), IO.Combine(dest, "Settings.json"), true);

            var cache = "Cache";
            foreach (var file in IO.GetFiles(Example(cache)))
            {
                var info = IO.Get(file);
                IO.Copy(file, IO.Combine(dest, $@"{cache}\{info.Name}"), true);
            }

            return dest;
        }
    }
}
