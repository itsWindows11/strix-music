﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.Models;
using StrixMusic.Sdk.Models.Core;
using StrixMusic.Sdk.Models.Merged;
using StrixMusic.Sdk.ViewModels.Helpers;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// Contains bindable information about an <see cref="IUserProfile"/>
    /// </summary>
    public class UserProfileViewModel : ObservableObject, ISdkViewModel, IUserProfile, IImageCollectionViewModel, IUrlCollectionViewModel
    {
        private readonly IUserProfile _userProfile;
        private readonly IReadOnlyList<ICoreUserProfile> _sources;
        private readonly IReadOnlyList<ICore> _sourceCores;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileViewModel"/> class.
        /// </summary>
        /// <param name="root">The <see cref="MainViewModel"/> that this or the object that created this originated from.</param>
        /// <param name="userProfile">The base <see cref="IUserProfile"/></param>
        internal UserProfileViewModel(MainViewModel root, IUserProfile userProfile)
        {
            _userProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
            Root = root;

            var userProfileImpl = userProfile.Cast<CoreUserProfileProxy>();

            _sourceCores = userProfileImpl.SourceCores.Select(root.GetLoadedCore).ToList();
            _sources = userProfileImpl.Sources;

            PopulateMoreImagesCommand = new AsyncRelayCommand<int>(PopulateMoreImagesAsync);
            PopulateMoreUrlsCommand = new AsyncRelayCommand<int>(PopulateMoreUrlsAsync);

            InitImageCollectionAsyncCommand = new AsyncRelayCommand(InitImageCollectionAsync);

            using (Threading.PrimaryContext)
            {
                Images = new ObservableCollection<IImage>();
                Urls = new ObservableCollection<IUrl>();
            }
        }

        /// <inheritdoc />
        public Task InitAsync()
        {
            if (IsInitialized)
                return Task.CompletedTask;

            IsInitialized = true;

            return InitImageCollectionAsync();
        }

        /// <inheritdoc />
        public event EventHandler<string>? DisplayNameChanged
        {
            add => _userProfile.DisplayNameChanged += value;
            remove => _userProfile.DisplayNameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<DateTime>? BirthDateChanged
        {
            add => _userProfile.BirthDateChanged += value;
            remove => _userProfile.BirthDateChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string>? FullNameChanged
        {
            add => _userProfile.FullNameChanged += value;
            remove => _userProfile.FullNameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<CultureInfo>? RegionChanged
        {
            add => _userProfile.RegionChanged += value;
            remove => _userProfile.RegionChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string?>? EmailChanged
        {
            add => _userProfile.EmailChanged += value;
            remove => _userProfile.EmailChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged
        {
            add => _userProfile.ImagesCountChanged += value;
            remove => _userProfile.ImagesCountChanged -= value;
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IImage>? ImagesChanged
        {
            add => _userProfile.ImagesChanged += value;
            remove => _userProfile.ImagesChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<int>? UrlsCountChanged
        {
            add => _userProfile.UrlsCountChanged += value;
            remove => _userProfile.UrlsCountChanged -= value;
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IUrl>? UrlsChanged
        {
            add => _userProfile.UrlsChanged += value;
            remove => _userProfile.UrlsChanged -= value;
        }

        /// <inheritdoc/>
        public MainViewModel Root { get; }

        /// <inheritdoc cref="IMerged{T}.SourceCores" />
        IReadOnlyList<ICore> IMerged<ICoreImageCollection>.SourceCores => _sourceCores;

        /// <inheritdoc cref="IMerged{T}.SourceCores" />
        IReadOnlyList<ICore> IMerged<ICoreUrlCollection>.SourceCores => _sourceCores;

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> IMerged<ICoreImageCollection>.Sources => _sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreUrlCollection> IMerged<ICoreUrlCollection>.Sources => _sources;

        /// <inheritdoc />
        public string Id => _userProfile.Id;

        /// <inheritdoc />
        public int TotalImageCount => _userProfile.TotalImageCount;

        /// <inheritdoc />
        public int TotalUrlCount => _userProfile.TotalUrlCount;

        /// <inheritdoc />
        public string DisplayName => _userProfile.DisplayName;

        /// <inheritdoc />
        public string? FullName => _userProfile.FullName;

        /// <inheritdoc />
        public string? Email => _userProfile.Email;

        /// <inheritdoc />
        public DateTime? Birthdate => _userProfile.Birthdate;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public ObservableCollection<IImage> Images { get; }

        /// <inheritdoc />
        public ObservableCollection<IUrl> Urls { get; }

        /// <inheritdoc />
        public CultureInfo Region => _userProfile.Region;

        /// <inheritdoc />
        public bool IsChangeDisplayNameAvailable => _userProfile.IsChangeDisplayNameAvailable;

        /// <inheritdoc />
        public bool IsChangeBirthDateAsyncAvailable => _userProfile.IsChangeBirthDateAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeFullNameAsyncAsyncAvailable => _userProfile.IsChangeFullNameAsyncAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeRegionAsyncAvailable => _userProfile.IsChangeRegionAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeEmailAsyncAvailable => _userProfile.IsChangeEmailAsyncAvailable;

        /// <inheritdoc />
        public Task<bool> IsAddUrlAvailableAsync(int index) => _userProfile.IsAddUrlAvailableAsync(index);

        /// <inheritdoc />
        public Task<bool> IsAddImageAvailableAsync(int index) => _userProfile.IsAddImageAvailableAsync(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveImageAvailableAsync(int index) => _userProfile.IsRemoveImageAvailableAsync(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveUrlAvailableAsync(int index) => _userProfile.IsRemoveUrlAvailableAsync(index);

        /// <inheritdoc />
        public Task ChangeDisplayNameAsync(string displayName) => _userProfile.ChangeDisplayNameAsync(displayName);

        /// <inheritdoc />
        public Task ChangeBirthDateAsync(DateTime birthdate) => _userProfile.ChangeBirthDateAsync(birthdate);

        /// <inheritdoc />
        public Task ChangeFullNameAsync(string fullname) => _userProfile.ChangeFullNameAsync(fullname);

        /// <inheritdoc />
        public Task ChangeRegionAsync(CultureInfo region) => _userProfile.ChangeRegionAsync(region);

        /// <inheritdoc />
        public Task ChangeEmailAsync(string? email) => _userProfile.ChangeEmailAsync(email);

        /// <inheritdoc />
        public Task<IReadOnlyList<IImage>> GetImagesAsync(int limit, int offset) => _userProfile.GetImagesAsync(limit, offset);

        /// <inheritdoc />
        public Task<IReadOnlyList<IUrl>> GetUrlsAsync(int limit, int offset) => _userProfile.GetUrlsAsync(limit, offset);

        /// <inheritdoc />
        public Task RemoveImageAsync(int index) => _userProfile.RemoveImageAsync(index);

        /// <inheritdoc />
        public Task AddImageAsync(IImage image, int index) => _userProfile.AddImageAsync(image, index);

        /// <inheritdoc />
        public Task AddUrlAsync(IUrl url, int index) => _userProfile.AddUrlAsync(url, index);

        /// <inheritdoc />
        public Task RemoveUrlAsync(int index) => _userProfile.RemoveUrlAsync(index);

        /// <inheritdoc />
        public async Task PopulateMoreImagesAsync(int limit)
        {
            var items = await GetImagesAsync(limit, Images.Count);

            _ = Threading.OnPrimaryThread(() =>
            {
                foreach (var item in items)
                {
                    Images.Add(item);
                }
            });
        }

        /// <inheritdoc />
        public async Task PopulateMoreUrlsAsync(int limit)
        {
            var items = await GetUrlsAsync(limit, Urls.Count);

            _ = Threading.OnPrimaryThread(() =>
            {
                foreach (var item in items)
                {
                    Urls.Add(item);
                }
            });
        }

        /// <inheritdoc />
        public Task InitImageCollectionAsync() => CollectionInit.ImageCollection(this);

        /// <inheritdoc />
        public IAsyncRelayCommand InitImageCollectionAsyncCommand { get; }

        /// <inheritdoc />
        public IAsyncRelayCommand<int> PopulateMoreImagesCommand { get; }

        /// <inheritdoc />
        public IAsyncRelayCommand<int> PopulateMoreUrlsCommand { get; }

        /// <inheritdoc />
        public bool Equals(ICoreImageCollection other) => _userProfile.Equals(other);

        /// <inheritdoc />
        public bool Equals(ICoreUrlCollection other) => _userProfile.Equals(other);

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _userProfile.DisposeAsync();
        }
    }
}
