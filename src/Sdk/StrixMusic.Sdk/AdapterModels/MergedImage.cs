﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using OwlCore.Extensions;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Sdk.AdapterModels
{
    /// <summary>
    /// Merged multiple <see cref="ICoreImage"/> into a single <see cref="IImage"/>
    /// </summary>
    public class MergedImage : IImage, IMergedMutable<ICoreImage>
    {
        private readonly ICoreImage _preferredSource;
        private readonly List<ICoreImage> _sources;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedImage"/> class.
        /// </summary>
        public MergedImage(IReadOnlyList<ICoreImage> sources)
        {
            Guard.IsNotNull(sources, nameof(sources));
            _sources = sources.ToList();

            _preferredSource = _sources[0];
        }

        /// <inheritdoc/>
        public Uri Uri => _preferredSource.Uri;

        /// <inheritdoc/>
        public double Height => _preferredSource.Height;

        /// <inheritdoc/>
        public double Width => _preferredSource.Width;

        /// <inheritdoc cref="IMerged{T}.Sources"/>
        public IReadOnlyList<ICoreImage> Sources => _sources;

        /// <inheritdoc/>
        public event EventHandler? SourcesChanged;

        /// <inheritdoc/>
        public void AddSource(ICoreImage itemToMerge)
        {
            Guard.IsNotNull(itemToMerge, nameof(itemToMerge));

            _sources.Add(itemToMerge);
            
            SourcesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void RemoveSource(ICoreImage itemToRemove)
        {
            Guard.IsNotNull(itemToRemove, nameof(itemToRemove));

            _sources.Remove(itemToRemove);
            
            SourcesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public bool Equals(ICoreImage other)
        {
            return other?.Uri == Uri;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _sources.InParallel(x => x.DisposeAsync().AsTask());
        }
    }
}
