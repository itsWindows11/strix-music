﻿using System.Collections.Generic;

namespace StrixMusic.CoreInterfaces.Interfaces.CoreConfig
{
    /// <summary>
    /// Provides methods to configure files access.
    /// </summary>
    public interface IFileConfig
    {
        /// <summary>
        /// Sets a list of file Ids, representing files that the user has allowed access.
        /// </summary>
        void SetFileAccessList(IReadOnlyList<string> fileIds);
    }
}