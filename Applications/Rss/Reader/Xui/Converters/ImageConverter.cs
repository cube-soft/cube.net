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
using System.IO;
using System.Windows.Media.Imaging;

namespace Cube.Xui.Converters
{
    /* --------------------------------------------------------------------- */
    ///
    /// ImageConversions
    ///
    /// <summary>
    /// BitmapImage オブジェクト関連する拡張用クラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class ImageOperations
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ToBitmapImage
        ///
        /// <summary>
        /// System.Drawing.Image オブジェクトから BitmapImage オブジェクトに
        /// 変換します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public static BitmapImage ToBitmapImage(this System.Drawing.Image src)
        {
            if (src == null) return default(BitmapImage);

            using (var ss = new MemoryStream())
            {
                src.Save(ss, System.Drawing.Imaging.ImageFormat.Png);
                var dest = new BitmapImage();
                dest.BeginInit();
                dest.CacheOption = BitmapCacheOption.OnLoad;
                dest.StreamSource = new MemoryStream(ss.ToArray());
                dest.EndInit();
                if (dest.CanFreeze) dest.Freeze();
                return dest;
            }
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ImageConverter
    ///
    /// <summary>
    /// System.Drawing.Image オブジェクトから BitmapImage オブジェクトに
    /// 変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ImageConverter : SimplexConverter
    {
        /* ----------------------------------------------------------------- */
        ///
        /// ImageValueConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public ImageConverter() : base(e =>
        {
            var src = e is System.Drawing.Image image ? image :
                      e is System.Drawing.Icon icon   ? icon.ToBitmap() :
                      null;
            return src.ToBitmapImage();
        }) { }
    }
}
