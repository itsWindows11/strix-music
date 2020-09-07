﻿using System;
using Hqub.MusicBrainz.API;
using Microsoft.Extensions.DependencyInjection;
using StrixMusic.Sdk.Interfaces;

namespace StrixMusic.Core.MusicBrainz.Models
{
    /// <summary>
    /// MockCore config
    /// </summary>
    public class MusicBrainzCoreConfig : ICoreConfig
    {
        /// <inheritdoc/>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicBrainzCoreConfig"/> class.
        /// </summary>
        public MusicBrainzCoreConfig(ICore sourceCore)
        {
            Services = ConfigureServices(sourceCore.InstanceId);
        }

        /// <summary>
        /// Configures services for this instance of the core.
        /// </summary>
        private IServiceProvider ConfigureServices(string instanceId)
        {
            IServiceCollection services = new ServiceCollection();

            services.Add(new ServiceDescriptor(typeof(MusicBrainzClient), new MusicBrainzClient()));

            return services.BuildServiceProvider();
        }
    }
}