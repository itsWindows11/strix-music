﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.Collections;
using OwlCore.Events;
using OwlCore.Helpers;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// Contains bindable information about an <see cref="IUserProfile"/>
    /// </summary>
    public class UserProfileViewModel : ObservableObject, IUserProfile, IImageCollectionViewModel
    {
        private readonly IUserProfile _userProfile;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileViewModel"/> class.
        /// </summary>
        /// <param name="userProfile">The base <see cref="IUserProfile"/></param>
        public UserProfileViewModel(IUserProfile userProfile)
        {
            _userProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));

            SourceCores = userProfile.GetSourceCores<ICoreUserProfile>().Select(MainViewModel.GetLoadedCore).ToList();
            PopulateMoreImagesCommand = new AsyncRelayCommand<int>(PopulateMoreImagesAsync);

            Urls = Threading.InvokeOnUI(() => new SynchronizedObservableCollection<Uri>(userProfile.Urls));
            Images = Threading.InvokeOnUI(() => new SynchronizedObservableCollection<IImage>());
        }

        private void AttachEvents()
        {
            throw new NotImplementedException();
        }

        private void DetachEvents()
        {
            throw new NotImplementedException();
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
        public event EventHandler<int> ImagesCountChanged
        {
            add => _userProfile.ImagesCountChanged += value;
            remove => _userProfile.ImagesCountChanged -= value;
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IImage> ImagesChanged
        {
            add => _userProfile.ImagesChanged += value;
            remove => _userProfile.ImagesChanged -= value;
        }

        /// <inheritdoc cref="ISdkMember{T}.SourceCores" />
        public IReadOnlyList<ICore> SourceCores { get; }

        /// <summary>
        /// The merged sources for this model.
        /// </summary>
        public IReadOnlyList<ICoreUserProfile> Sources => _userProfile.GetSources<ICoreUserProfile>();

        /// <inheritdoc />
        IReadOnlyList<ICoreUserProfile> ISdkMember<ICoreUserProfile>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> ISdkMember<ICoreImageCollection>.Sources => Sources;

        /// <inheritdoc />
        public string Id => _userProfile.Id;

        /// <inheritdoc />
        public int TotalImageCount => _userProfile.TotalImageCount;

        /// <inheritdoc />
        public string DisplayName => _userProfile.DisplayName;

        /// <inheritdoc />
        public string? FullName => _userProfile.FullName;

        /// <inheritdoc />
        public string? Email => _userProfile.Email;

        /// <inheritdoc />
        public DateTime? Birthdate => _userProfile.Birthdate;

        /// <inheritdoc />
        public SynchronizedObservableCollection<IImage> Images { get; }

        /// <inheritdoc />
        public SynchronizedObservableCollection<Uri>? Urls { get; }

        /// <inheritdoc />
        public CultureInfo Region => _userProfile.Region;

        /// <inheritdoc />
        public bool IsChangeDisplayNameSupported => _userProfile.IsChangeDisplayNameSupported;

        /// <inheritdoc />
        public bool IsChangeBirthDateAsyncSupported => _userProfile.IsChangeBirthDateAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeFullNameAsyncAsyncSupported => _userProfile.IsChangeFullNameAsyncAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeRegionAsyncSupported => _userProfile.IsChangeRegionAsyncSupported;

        /// <inheritdoc />
        public bool IsChangeEmailAsyncSupported => _userProfile.IsChangeEmailAsyncSupported;

        /// <inheritdoc />
        public Task<bool> IsAddUrlSupported(int index) => _userProfile.IsAddUrlSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddImageSupported(int index) => _userProfile.IsAddImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveImageSupported(int index) => _userProfile.IsRemoveImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveUrlSupported(int index) => _userProfile.IsRemoveUrlSupported(index);

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
        public Task<IReadOnlyList<IImage>> GetImagesAsync(int limit, int offset)
        {
            return _userProfile.GetImagesAsync(limit, offset);
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            return _userProfile.RemoveImageAsync(index);
        }

        /// <inheritdoc />
        public Task AddImageAsync(IImage image, int index)
        {
            return _userProfile.AddImageAsync(image, index);
        }

        /// <inheritdoc />
        public async Task PopulateMoreImagesAsync(int limit)
        {
            foreach (var item in await GetImagesAsync(limit, Images.Count))
            {
                Images.Add(item);
            }
        }

        /// <inheritdoc />
        public IAsyncRelayCommand<int> PopulateMoreImagesCommand { get; }
    }
}