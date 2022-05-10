﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using StrixMusic.Sdk.AdapterModels;
using StrixMusic.Sdk.BaseModels;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Sdk.AppModels
{
    /// <summary>
    /// A collection of arbitrary songs that the user can edit, rearrange and play back.
    /// </summary>
    /// <remarks>Instances of this class may contain data merged from one or more sources.</remarks>
    public interface IPlaylist : IPlaylistBase, ITrackCollection, IImageCollection, IUrlCollection, IPlaylistCollectionItem, IPlayable, IAppModel, IMerged<ICorePlaylist>, IMerged<ICorePlaylistCollectionItem>
    {
        /// <summary>
        /// Owner of the playable item.
        /// </summary>
        #warning TODO needs a changed event to facilitate merging in new sources with non-null values.
        IUserProfile? Owner { get; }

        /// <summary>
        /// A <see cref="IPlayableCollectionGroup"/> of items related to this item.
        /// </summary>
        #warning TODO needs a changed event to facilitate merging in new sources with non-null values.
        IPlayableCollectionGroup? RelatedItems { get; }
    }
}
