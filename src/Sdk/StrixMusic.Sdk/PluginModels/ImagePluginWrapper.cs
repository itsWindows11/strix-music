﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StrixMusic.Sdk.AdapterModels;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.CoreModels;
using StrixMusic.Sdk.Plugins.Model;

namespace StrixMusic.Sdk.PluginModels;

/// <summary>
/// Wraps an instance of <see cref="IImage"/> with the provided plugins.
/// </summary>
public class ImagePluginWrapper : IImage, IPluginWrapper
{
    private readonly IImage _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayableCollectionGroupPluginWrapperBase"/> class.
    /// </summary>
    /// <param name="image">The instance to wrap around and apply plugins to.</param>
    /// <param name="plugins">The plugins that are applied to items returned from or emitted by this collection.</param>
    internal ImagePluginWrapper(IImage image, params SdkModelPlugin[] plugins)
    {
        foreach (var item in plugins)
            ActivePlugins.Import(item);

        ActivePlugins = GlobalModelPluginConnector.Create(ActivePlugins);
        
        _image = ActivePlugins.Image.Execute(image);
        
        AttachEvents(_image);
    }
    
    /// <inheritdoc/>
    public SdkModelPlugin ActivePlugins { get; } = new(PluginModelWrapperInfo.Metadata);

    private void AttachEvents(IImage image)
    {
        image.SourcesChanged += OnSourcesChanged;
    }

    private void DetachEvents(IImage image)
    {
        image.SourcesChanged -= OnSourcesChanged;
    }
    
    private void OnSourcesChanged(object sender, EventArgs e) => SourcesChanged?.Invoke(sender, e);
    
    /// <inheritdoc cref="IMerged.SourcesChanged"/>
    public event EventHandler? SourcesChanged;

    /// <inheritdoc/>
    public Uri Uri => _image.Uri;

    /// <inheritdoc/>
    public double Height => _image.Height;

    /// <inheritdoc/>
    public double Width => _image.Width;

    /// <inheritdoc/>
    public bool Equals(ICoreImage other) => _image.Equals(other);

    /// <inheritdoc/>
    public IReadOnlyList<ICoreImage> Sources => _image.Sources;

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        DetachEvents(_image);
        return _image.DisposeAsync();
    }
}
