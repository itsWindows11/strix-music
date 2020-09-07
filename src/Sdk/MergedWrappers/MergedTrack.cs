﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using StrixMusic.Sdk;
using StrixMusic.Sdk.Enums;
using StrixMusic.Sdk.Interfaces;

namespace StrixMusic.Sdk.MergedWrappers
{
    /// <summary>
    /// A concrete class that merged multiple <see cref="ISearchResults"/>
    /// </summary>
    public class MergedTrack : ITrack, IEquatable<ITrack>
    {
        private readonly List<IArtist> _artists = new List<IArtist>();

        private ITrack _preferredSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedSearchResults"/> class.
        /// </summary>
        /// <param name="tracks">The <see cref="ITrack"/>s to merge together.</param>
        public MergedTrack(ITrack[] tracks)
        {
            if (tracks == null)
            {
                throw new ArgumentNullException(nameof(tracks));
            }

            // TODO: Use top preffered core.
            _preferredSource = tracks.First();

            foreach (var item in tracks)
            {
                _artists.AddRange(item.Artists);

                item.ArtistsChanged += Item_ArtistsChanged;
            }
        }

        private void Item_ArtistsChanged(object sender, CollectionChangedEventArgs<IArtist> e)
        {
            // TODO: Handle merging
            foreach (var item in e.AddedItems)
            {
                _artists.Add(item);
            }

            foreach (var item in e.RemovedItems)
            {
                _artists.Remove(item);
            }
        }

        /// <inheritdoc/>
        public TrackType Type => _preferredSource.Type;

        /// <inheritdoc/>
        public IReadOnlyList<IArtist> Artists => _artists;

        /// <inheritdoc/>
        public IAlbum? Album => _preferredSource.Album;

        /// <inheritdoc/>
        public DateTime? DatePublished => _preferredSource.DatePublished;

        /// <inheritdoc/>
        public IReadOnlyList<string>? Genres => _preferredSource.Genres;

        /// <inheritdoc/>
        public int? TrackNumber => _preferredSource.TrackNumber;

        /// <inheritdoc/>
        public int? PlayCount => _preferredSource.PlayCount;

        /// <inheritdoc/>
        public CultureInfo? Language => _preferredSource.Language;

        /// <inheritdoc/>
        public ILyrics? Lyrics => _preferredSource.Lyrics;

        /// <inheritdoc/>
        public bool IsExplicit => _preferredSource.IsExplicit;

        /// <inheritdoc/>
        public ICore SourceCore => _preferredSource.SourceCore;

        /// <inheritdoc/>
        public string Id => _preferredSource.Id;

        /// <inheritdoc/>
        public Uri? Url => _preferredSource.Url;

        /// <inheritdoc/>
        public string Name => _preferredSource.Name;

        /// <inheritdoc/>
        public IReadOnlyList<IImage> Images => _preferredSource.Images;

        /// <inheritdoc/>
        public string? Description => _preferredSource.Description;

        /// <inheritdoc/>
        public PlaybackState PlaybackState => _preferredSource.PlaybackState;

        /// <inheritdoc/>
        public TimeSpan Duration => _preferredSource.Duration;

        /// <inheritdoc/>
        public IPlayableCollectionGroup RelatedItems => _preferredSource.RelatedItems;

        /// <inheritdoc/>
        public bool IsChangeArtistsAsyncSupported => _preferredSource.IsChangeArtistsAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeAlbumAsyncSupported => _preferredSource.IsChangeAlbumAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeDatePublishedAsyncSupported => _preferredSource.IsChangeDatePublishedAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeGenresAsyncSupported => _preferredSource.IsChangeGenresAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeTrackNumberAsyncSupported => _preferredSource.IsChangeTrackNumberAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeLanguageAsyncSupported => _preferredSource.IsChangeLanguageAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeLyricsAsyncSupported => _preferredSource.IsChangeLyricsAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeIsExplicitAsyncSupported => _preferredSource.IsChangeIsExplicitAsyncSupported;

        /// <inheritdoc/>
        public bool IsPlayAsyncSupported => _preferredSource.IsPlayAsyncSupported;

        /// <inheritdoc/>
        public bool IsPauseAsyncSupported => _preferredSource.IsPauseAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncSupported => _preferredSource.IsChangeNameAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeImagesAsyncSupported => _preferredSource.IsChangeImagesAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncSupported => _preferredSource.IsChangeDescriptionAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncSupported => _preferredSource.IsChangeDurationAsyncSupported;

        /// <inheritdoc/>
        public event EventHandler<CollectionChangedEventArgs<IArtist>> ArtistsChanged
        {
            add
            {
                _preferredSource.ArtistsChanged += value;
            }

            remove
            {
                _preferredSource.ArtistsChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<CollectionChangedEventArgs<string>> GenresChanged
        {
            add
            {
                _preferredSource.GenresChanged += value;
            }

            remove
            {
                _preferredSource.GenresChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<IAlbum?> AlbumChanged
        {
            add
            {
                _preferredSource.AlbumChanged += value;
            }

            remove
            {
                _preferredSource.AlbumChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<DateTime?> DatePublishedChanged
        {
            add
            {
                _preferredSource.DatePublishedChanged += value;
            }

            remove
            {
                _preferredSource.DatePublishedChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<int?> TrackNumberChanged
        {
            add
            {
                _preferredSource.TrackNumberChanged += value;
            }

            remove
            {
                _preferredSource.TrackNumberChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<int?> PlayCountChanged
        {
            add
            {
                _preferredSource.PlayCountChanged += value;
            }

            remove
            {
                _preferredSource.PlayCountChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<CultureInfo?> LanguageChanged
        {
            add
            {
                _preferredSource.LanguageChanged += value;
            }

            remove
            {
                _preferredSource.LanguageChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<ILyrics?> LyricsChanged
        {
            add
            {
                _preferredSource.LyricsChanged += value;
            }

            remove
            {
                _preferredSource.LyricsChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<bool> IsExplicitChanged
        {
            add
            {
                _preferredSource.IsExplicitChanged += value;
            }

            remove
            {
                _preferredSource.IsExplicitChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<PlaybackState> PlaybackStateChanged
        {
            add
            {
                _preferredSource.PlaybackStateChanged += value;
            }

            remove
            {
                _preferredSource.PlaybackStateChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<string> NameChanged
        {
            add
            {
                _preferredSource.NameChanged += value;
            }

            remove
            {
                _preferredSource.NameChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<string?> DescriptionChanged
        {
            add
            {
                _preferredSource.DescriptionChanged += value;
            }

            remove
            {
                _preferredSource.DescriptionChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<Uri?> UrlChanged
        {
            add
            {
                _preferredSource.UrlChanged += value;
            }

            remove
            {
                _preferredSource.UrlChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<CollectionChangedEventArgs<IImage>>? ImagesChanged
        {
            add
            {
                _preferredSource.ImagesChanged += value;
            }

            remove
            {
                _preferredSource.ImagesChanged -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<TimeSpan>? DurationChanged
        {
            add
            {
                _preferredSource.DurationChanged += value;
            }

            remove
            {
                _preferredSource.DurationChanged -= value;
            }
        }

        /// <inheritdoc/>
        public Task PauseAsync()
        {
            return _preferredSource.PauseAsync();
        }

        /// <inheritdoc/>
        public Task PlayAsync()
        {
            return _preferredSource.PlayAsync();
        }

        /// <summary>
        /// Indicates whether the current item can be merged with another item of the same type.
        /// </summary>
        /// <param name="other">The item to check against.</param>
        /// <returns><see langword="True"/> if the item can be merged, otherwise <see langword="false"/></returns>
        public bool Equals(ITrack? other)
        {
            return other?.Name == Name
                && other?.Type.Equals(Type) == true
                && other?.Album?.Equals(Album) == true
                && other?.Language?.Equals(Language) == true
                && other?.TrackNumber?.Equals(TrackNumber) == true
                && other?.Artists?.SequenceEqual(Artists) == true;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj);

        /// <inheritdoc/>
        public override int GetHashCode() => _preferredSource.Id.GetHashCode();

        /// <inheritdoc/>
        public Task ChangeArtistsAsync(IReadOnlyList<IArtist>? artists)
        {
            return _preferredSource.ChangeArtistsAsync(artists);
        }

        /// <inheritdoc/>
        public Task ChangeAlbumAsync(IAlbum? albums)
        {
            return _preferredSource.ChangeAlbumAsync(albums);
        }

        /// <inheritdoc/>
        public Task ChangeDatePublishedAsync(DateTime datePublished)
        {
            return _preferredSource.ChangeDatePublishedAsync(datePublished);
        }

        /// <inheritdoc/>
        public Task ChangeGenresAsync(IReadOnlyList<string>? genres)
        {
            return _preferredSource.ChangeGenresAsync(genres);
        }

        /// <inheritdoc/>
        public Task ChangeTrackNumberAsync(int? trackNumber)
        {
            return _preferredSource.ChangeTrackNumberAsync(trackNumber);
        }

        /// <inheritdoc/>
        public Task ChangeLanguageAsync(CultureInfo language)
        {
            return _preferredSource.ChangeLanguageAsync(language);
        }

        /// <inheritdoc/>
        public Task ChangeLyricsAsync(ILyrics? lyrics)
        {
            return _preferredSource.ChangeLyricsAsync(lyrics);
        }

        /// <inheritdoc/>
        public Task ChangeIsExplicitAsync(bool isExplicit)
        {
            return _preferredSource.ChangeIsExplicitAsync(isExplicit);
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name)
        {
            return _preferredSource.ChangeNameAsync(name);
        }

        /// <inheritdoc/>
        public Task ChangeImagesAsync(IReadOnlyList<IImage> images)
        {
            return _preferredSource.ChangeImagesAsync(images);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description)
        {
            return _preferredSource.ChangeDescriptionAsync(description);
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            return _preferredSource.ChangeDurationAsync(duration);
        }
    }
}