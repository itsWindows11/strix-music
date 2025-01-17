﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

namespace StrixMusic.Sdk.AppModels
{
    /// <summary>
    /// Holds information about a download operation.
    /// </summary>
    public readonly struct DownloadInfo : System.IEquatable<DownloadInfo>
    {
        /// <summary>
        /// Creates an instance of the <see cref="DownloadInfo"/> struct.
        /// </summary>
        public DownloadInfo(DownloadState state)
        {
            State = state;
            Progress = default;
        }

        /// <summary>
        /// Creates an instance of the <see cref="DownloadInfo"/> struct.
        /// </summary>
        public DownloadInfo(DownloadState state, ushort progress)
        {
            Progress = progress;
            State = state;
        }

        /// <summary>
        /// A value between 0 and 65535 representing how much of this playable item has been downloaded for offline playback.
        /// </summary>
        public ushort Progress { get; }

        /// <summary>
        /// The current download state.
        /// </summary>
        public DownloadState State { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is DownloadInfo status && Equals(status);

        /// <inheritdoc />
        public override int GetHashCode() => State.GetHashCode() + Progress.GetHashCode();

        /// <inheritdoc cref="Equals(object)" />
        public static bool operator ==(DownloadInfo left, DownloadInfo right) => left.Equals(right);

        /// <inheritdoc cref="Equals(object)" />
        public static bool operator !=(DownloadInfo left, DownloadInfo right) => !(left == right);

        /// <inheritdoc />
        public bool Equals(DownloadInfo other) => other.State == State && other.Progress == Progress;
    }
}
