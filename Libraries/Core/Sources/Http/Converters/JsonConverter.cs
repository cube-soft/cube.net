﻿/* ------------------------------------------------------------------------- */
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
using System.Runtime.Serialization.Json;

namespace Cube.Net.Http
{
    /* --------------------------------------------------------------------- */
    ///
    /// JsonContentConverter(TValue)
    ///
    /// <summary>
    /// Provides functionality to convert HttpContent in JSON format.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class JsonContentConverter<TValue> : ContentConverter<TValue> where TValue : class
    {
        /* ----------------------------------------------------------------- */
        ///
        /// JsonContentConverter
        ///
        /// <summary>
        /// Initializes a new instance of the JsonContentConverter class.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public JsonContentConverter() : base(s =>
        {
            if (s == null) return default;
            var json = new DataContractJsonSerializer(typeof(TValue));
            return json.ReadObject(s) as TValue;
        }) { }
    }
}
