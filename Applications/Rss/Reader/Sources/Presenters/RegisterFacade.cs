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
namespace Cube.Net.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RegisterFacade
    ///
    /// <summary>
    /// Provides functionality to communicate with the RegisterViewModel
    /// and other model classes.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class RegisterFacade : ObservableBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterFacade
        ///
        /// <summary>
        /// Initializes a new instance of the RegisterFacade class.
        /// </summary>
        ///
        /// <param name="dispatcher">Invoker object.</param>
        ///
        /* ----------------------------------------------------------------- */
        public RegisterFacade(Dispatcher dispatcher) : base(dispatcher) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Url
        ///
        /// <summary>
        /// Gets or sets the URL string to register.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Url
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Busy
        ///
        /// <summary>
        /// Gets or sets a value indicating whether any operations are
        /// in progress.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Busy
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// Releases the unmanaged resources used by the object and
        /// optionally releases the managed resources.
        /// </summary>
        ///
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
        ///
        /* ----------------------------------------------------------------- */
        protected override void Dispose(bool disposing) { }

        #endregion
    }
}
