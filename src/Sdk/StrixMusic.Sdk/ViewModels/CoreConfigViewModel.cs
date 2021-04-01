﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using OwlCore;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// ViewModel for an <see cref="ICoreConfig"/>
    /// </summary>
    public class CoreConfigViewModel : ObservableObject, ICoreConfig
    {
        private readonly ICoreConfig _coreConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConfigViewModel"/> class.
        /// </summary>
        /// <param name="coreConfig">The instance of <see cref="ICoreConfig"/> to wrap around for this view model.</param>
        public CoreConfigViewModel(ICoreConfig coreConfig)
        {
            _coreConfig = coreConfig;

            AbstractUIElements = new ObservableCollection<AbstractUIElementGroupViewModel>();

            foreach (var abstractUIElement in _coreConfig.AbstractUIElements)
            {
                AbstractUIElements.Clear();
                AbstractUIElements.Add(new AbstractUIElementGroupViewModel(abstractUIElement));
            }

            AttachEvents();
        }

        private void AttachEvents()
        {
            _coreConfig.AbstractUIElementsChanged += CoreConfig_AbstractUIElementsChanged;
        }

        private void DetachEvents()
        {
            _coreConfig.AbstractUIElementsChanged -= CoreConfig_AbstractUIElementsChanged;
        }

        private void CoreConfig_AbstractUIElementsChanged(object sender, EventArgs e)
        {
            _ = Threading.OnPrimaryThread(() =>
            {
                foreach (var abstractUIElement in _coreConfig.AbstractUIElements)
                {
                    AbstractUIElements.Clear();
                    AbstractUIElements.Add(new AbstractUIElementGroupViewModel(abstractUIElement));
                }
            });
        }

        /// <inheritdoc/>
        public IServiceProvider? Services => _coreConfig.Services;

        /// <inheritdoc/>
        IReadOnlyList<AbstractUIElementGroup> ICoreConfigBase.AbstractUIElements => _coreConfig.AbstractUIElements;

        /// <inheritdoc cref="ICoreConfigBase.AbstractUIElements" />
        public ObservableCollection<AbstractUIElementGroupViewModel> AbstractUIElements { get; private set; }

        /// <inheritdoc/>
        public MediaPlayerType PlaybackType => _coreConfig.PlaybackType;

        /// <inheritdoc />
        public event EventHandler? AbstractUIElementsChanged;

        /// <inheritdoc/>
        public ICore SourceCore => _coreConfig.SourceCore;

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            DetachEvents();
            return _coreConfig.DisposeAsync();
        }
    }
}