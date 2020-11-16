﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using OwlCore.Collections;
using OwlCore.Extensions;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;

namespace StrixMusic.Sdk.Data.Merged
{
    public class MergedUser : IUser, IMerged<ICoreUser>
    {
        private readonly ICoreUser _user;

        /// <summary>
        /// Creates a new instance of <see cref="MergedUser"/>.
        /// </summary>
        /// <param name="user">The user to wrap around.</param>
        public MergedUser(ICoreUser user)
        {
            _user = user;

            Library = new MergedLibrary(_user.Library.IntoList());
            Images = new SynchronizedObservableCollection<IImage>()
        }

        /// <inheritdoc />
        public string Id => _user.Id;

        /// <inheritdoc />
        public string DisplayName => _user.DisplayName;

        /// <inheritdoc />
        public string? FullName => _user.FullName;

        /// <inheritdoc />
        public SynchronizedObservableCollection<Uri>? Urls => _user.Urls;

        /// <inheritdoc />
        public string? Email => _user.Email;

        /// <inheritdoc />
        public DateTime? Birthdate => _user.Birthdate;

        /// <inheritdoc />
        public CultureInfo Region => _user.Region;

        /// <inheritdoc />
        public bool IsChangeDisplayNameSupported => _user.IsChangeDisplayNameSupported;

        /// <inheritdoc />
        public bool IsChangeBirthDateAsyncSupported => _user.IsChangeBirthDateAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeFullNameAsyncAsyncSupported => _user.IsChangeFullNameAsyncAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeRegionAsyncSupported => _user.IsChangeRegionAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeEmailAsyncSupported => _user.IsChangeEmailAsyncSupported;

        /// <inheritdoc />
        public Task<bool> IsAddUrlSupported(int index)
        {
            return _user.IsAddUrlSupported(index);
        }

        /// <inheritdoc />
        public Task<bool> IsAddImageSupported(int index)
        {
            return _user.IsAddImageSupported(index);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveUrlSupported(int index)
        {
            return _user.IsRemoveUrlSupported(index);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveImageSupported(int index)
        {
            return _user.IsRemoveImageSupported(index);
        }

        /// <inheritdoc />
        public Task ChangeDisplayNameAsync(string displayName)
        {
            return _user.ChangeDisplayNameAsync(displayName);
        }

        /// <inheritdoc />
        public Task ChangeBirthDateAsync(DateTime birthdate)
        {
            return _user.ChangeBirthDateAsync(birthdate);
        }

        /// <inheritdoc />
        public Task ChangeFullNameAsync(string fullname)
        {
            return _user.ChangeFullNameAsync(fullname);
        }

        /// <inheritdoc />
        public Task ChangeRegionAsync(CultureInfo region)
        {
            return _user.ChangeRegionAsync(region);
        }

        /// <inheritdoc />
        public Task ChangeEmailAsync(string? email)
        {
            return _user.ChangeEmailAsync(email);
        }

        /// <inheritdoc />
        public event EventHandler<string>? DisplayNameChanged
        {
            add => _user.DisplayNameChanged += value;
            remove => _user.DisplayNameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<DateTime>? BirthDateChanged
        {
            add => _user.BirthDateChanged += value;
            remove => _user.BirthDateChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string>? FullNameChanged
        {
            add => _user.FullNameChanged += value;
            remove => _user.FullNameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<CultureInfo>? RegionChanged
        {
            add => _user.RegionChanged += value;
            remove => _user.RegionChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string?>? EmailChanged
        {
            add => _user.EmailChanged += value;
            remove => _user.EmailChanged -= value;
        }

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> ISdkMember<ICoreImageCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreUserProfile> ISdkMember<ICoreUserProfile>.Sources => Sources;
        
        /// <inheritdoc />
        IReadOnlyList<ICoreUser> IMerged<ICoreUser>.Sources => Sources;

        /// <inheritdoc cref="ISdkMember{T}.Sources" />
        public IReadOnlyList<ICoreUser> Sources => this.GetSources<ICoreUser>();

        /// <inheritdoc cref="ISdkMember{T}.SourceCores" />
        public IReadOnlyList<ICore> SourceCores => this.GetSourceCores<ICoreUser>();

        /// <inheritdoc />
        public ILibrary Library { get; }

        /// <inheritdoc />
        public bool Equals(ICoreUser other)
        {
            // We don't merge these.
            return false;
        }

        /// <inheritdoc />
        public void AddSource(ICoreUser itemToMerge)
        {
            throw new NotSupportedException();
        }
    }
}