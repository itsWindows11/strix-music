﻿using System;
using System.Collections.Generic;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Files.Models
{
    /// <summary>
    /// Represents a the user that is accessing files.
    /// </summary>
    public class FileUser : IUser
    {
        /// <summary>
        /// Constructs a new <see cref="FileUser"/>.
        /// </summary>
        /// <param name="sourceCore"></param>
        public FileUser(ICore sourceCore)
        {
            SourceCore = sourceCore;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IDevice> Devices => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Id => nameof(FileUser);

        /// <inheritdoc/>
        public string DisplayName => throw new NotImplementedException();

        /// <inheritdoc/>
        public string FullName => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Email => throw new NotImplementedException();

        /// <inheritdoc/>
        public IReadOnlyList<IImage> Images => throw new NotImplementedException();

        /// <inheritdoc/>
        public DateTime? Birthdate => null;

        /// <inheritdoc/>
        public IReadOnlyList<Uri> Url => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Region { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ICore SourceCore { get; set; }
    }
}