﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// Properties which must be implemented by all view models in the SDK.
    /// </summary>
    public interface ISdkViewModel
    {
        /// <summary>
        /// The <see cref="MainViewModel"/> that this or the object that created this originated from.
        /// </summary>
        public MainViewModel Root { get; }
    }
}
