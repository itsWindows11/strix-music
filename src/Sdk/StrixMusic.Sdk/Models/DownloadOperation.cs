﻿namespace StrixMusic.Sdk.Models
{
    /// <summary>
    /// Represents the actions that can be performed for a download.
    /// </summary>
    public enum DownloadOperation
    {
        /// <summary>
        /// Begins or resumes a download.
        /// </summary>
        Start,

        /// <summary>
        /// Pauses the download operation.
        /// </summary>
        Pause,

        /// <summary>
        /// Cancels the ongoing download operation, if any.
        /// </summary>
        Cancel,

        /// <summary>
        /// Removes the downloaded item from disk.
        /// </summary>
        Delete,
    }
}