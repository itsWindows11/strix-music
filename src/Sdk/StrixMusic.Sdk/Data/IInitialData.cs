﻿using System.Collections.Generic;
using StrixMusic.Sdk.Data.Core;

namespace StrixMusic.Sdk.Data
{
    /// <summary>
    /// Used as a common interface for data that is created in the UI.
    /// </summary>
    public interface IInitialData
    {
        /// <summary>
        /// The created data will be added to any core in this list. If empty, this is decided by the Sdk.
        /// </summary>
        List<ICore>? TargetSourceCores { get; set; }
    }
}