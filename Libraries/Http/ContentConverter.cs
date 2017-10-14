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
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Cube.Log;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// IContentConverter
    ///
    /// <summary>
    /// HttpContent を変換するためのインターフェースです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public interface IContentConverter<TValue>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        /// 
        /// <remarks>
        /// HttpMonitor のように、例外発生時に既定回数再試行するような
        /// クラスで使用された場合、変換の失敗が原因で余分な通信を発生
        /// させる可能性があります。そのような場合には IgnoreException を
        /// true にして抑制します。
        /// </remarks>
        /// 
        /* ----------------------------------------------------------------- */
        bool IgnoreException { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        Task<TValue> ConvertAsync(HttpContent src);
    }

    /* --------------------------------------------------------------------- */
    ///
    /// ContentConverter(TValue)
    ///
    /// <summary>
    /// 関数オブジェクトを IContentConverter に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ContentConverter<TValue> : IContentConverter<TValue>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// GenericContentConverter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="func">変換用オブジェクト</param>
        /// 
        /* ----------------------------------------------------------------- */
        public ContentConverter(Func<HttpContent, Task<TValue>> func)
        {
            _func = func;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; } = false;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<TValue> ConvertAsync(HttpContent src)
        {
            try { return await _func(src); }
            catch (Exception err)
            {
                if (IgnoreException) this.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(TValue);
        }

        #endregion

        #region Fields
        private Func<HttpContent, Task<TValue>> _func;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// JsonContentConverter(TValue)
    ///
    /// <summary>
    /// JSON 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class JsonContentConverter<TValue> : IContentConverter<TValue>
        where TValue : class
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; } = false;

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<TValue> ConvertAsync(HttpContent src)
        {
            try
            {
                if (src == null) return default(TValue);
                var stream = await src.ReadAsStreamAsync();
                var json = new DataContractJsonSerializer(typeof(TValue));
                return json.ReadObject(stream) as TValue;
            }
            catch (Exception err)
            {
                if (IgnoreException) this.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(TValue);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// XmlContentConverter(TValue)
    ///
    /// <summary>
    /// XML 形式の HttpContent を変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class XmlContentConverter<TValue> : IContentConverter<TValue>
        where TValue : class
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; } = false;

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<TValue> ConvertAsync(HttpContent src)
        {
            try
            {
                if (src == null) return default(TValue);
                var stream = await src.ReadAsStreamAsync();
                var xml = new XmlSerializer(typeof(TValue));
                return xml.Deserialize(stream) as TValue;
            }
            catch (Exception err)
            {
                if (IgnoreException) this.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(TValue);
        }
    }

    /* --------------------------------------------------------------------- */
    ///
    /// RssContentConverter
    ///
    /// <summary>
    /// HttpContent を RssFeed に変換するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class RssContentConverter : IContentConverter<Rss.RssFeed>
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IgnoreException
        ///
        /// <summary>
        /// 例外を無視するかどうかを示す値を取得または設定します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool IgnoreException { get; set; } = false;

        /* ----------------------------------------------------------------- */
        ///
        /// ConvertAsync(TValue)
        ///
        /// <summary>
        /// 非同期で変換処理を実行します。
        /// </summary>
        /// 
        /// <param name="src">HttpContent オブジェクト</param>
        /// 
        /// <returns>変換後のオブジェクト</returns>
        ///
        /* ----------------------------------------------------------------- */
        public async Task<Rss.RssFeed> ConvertAsync(HttpContent src)
        {
            try
            {
                if (src == null) return default(Rss.RssFeed);
                var stream = await src.ReadAsStreamAsync();
                return Rss.RssParser.Create(stream);
            }
            catch (Exception err)
            {
                if (IgnoreException) this.LogWarn(err.ToString(), err);
                else throw;
            }
            return default(Rss.RssFeed);
        }
    }
}
