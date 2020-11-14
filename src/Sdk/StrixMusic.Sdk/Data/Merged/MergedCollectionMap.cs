﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using OwlCore.Events;
using OwlCore.Extensions;
using OwlCore.Provisos;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.Services.Settings;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// Maps indices for sources in an an <see cref="IMerged{T}"/>.
    /// </summary>
    /// <typeparam name="TCollection">The collection type that this is part of.</typeparam>
    /// <typeparam name="TCoreCollection">The types of items that were merged to form <typeparamref name="TCollection"/>.</typeparam>
    /// <typeparam name="TCollectionItem">The type of the item returned from the merged collection.</typeparam>
    /// <typeparam name="TCoreCollectionItem">The type of the items returned from the original source collections.</typeparam>
    public class MergedCollectionMap<TCollection, TCoreCollection, TCollectionItem, TCoreCollectionItem> : IMerged<TCoreCollection>, IAsyncInit
        where TCollection : class, IPlayableCollectionBase, ISdkMember<TCoreCollection>
        where TCoreCollection : class, ICorePlayableCollection
        where TCollectionItem : class, ICollectionItemBase, ISdkMember<TCoreCollectionItem>
        where TCoreCollectionItem : class, ICollectionItemBase, ICoreMember
    {
        private readonly TCollection _collection;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// A map where each index contains the representation of an item returned from a source collection, where the value is that source collection.
        /// </summary>
        private readonly List<MappedData> _sortedMap = new List<MappedData>();

        /// <summary>
        /// The same as <see cref="_sortedMap"/>, but data is merged.
        /// </summary>
        private readonly List<IMerged<TCoreCollectionItem>> _mergedSortedMap = new List<IMerged<TCoreCollectionItem>>();

        private IReadOnlyList<Type>? _coreRanking;
        private MergedCollectionSorting? _sortingMethod;

        /// <inheritdoc />
        public IReadOnlyList<TCoreCollection> Sources => _collection.Sources;

        /// <summary>
        /// Fires when a source has been added and the merged collection needs to be re-emitted to include the new source.
        /// </summary>
        public event CollectionChangedEventHandler<TCoreCollectionItem>? ItemsChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="MergedCollectionMap{TCollection,TCollectionItem,TMerged}"/>.
        /// </summary>
        /// <param name="collection">The collection that contains the items </param>
        public MergedCollectionMap(TCollection collection)
        {
            _collection = collection;
            _settingsService = Ioc.Default.GetService<ISettingsService>();
        }

        /// <inheritdoc />
        public async Task InitAsync()
        {
            _coreRanking = await GetCoreRankings();
            _sortingMethod = await GetSortingMethod();
            _settingsService.SettingChanged += SettingsServiceOnSettingChanged;
        }

        /// <summary>
        /// Gets a range of items from the collection, merged and sorted from multiple sources.
        /// </summary>
        /// <param name="limit">The max number of items to return.</param>
        /// <param name="offset">Get items starting at this index.</param>
        /// <returns>The requested range of items, sorted and merged from the sources in the input collection.</returns>
        public Task<IReadOnlyList<TCollectionItem>> GetItems(int limit, int offset)
        {
            Guard.IsNotNull(_sortingMethod, nameof(_sortingMethod));

            return _sortingMethod switch
            {
                MergedCollectionSorting.Ranked => GetItemsByRank(limit, offset),
                _ => ThrowHelper.ThrowNotSupportedException<Task<IReadOnlyList<TCollectionItem>>>($"Merged collection sorting by \"{_sortingMethod}\" not supported.")
            };
        }

        private static void MergeOrAdd(List<IMerged<TCoreCollectionItem>> collection, TCoreCollectionItem itemToMerge)
        {
            foreach (var item in collection)
            {
                if (item.Equals(itemToMerge))
                {
                    item.AddSource(itemToMerge);
                    return;
                }
            }

            // if the collection doesn't contain IMerged<TCollectionItem> at all, create a new Merged
            switch (itemToMerge)
            {
                case ICoreArtist artist:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedArtist(artist.IntoList()));
                    break;
                case ICoreAlbum album:
                    // no idea why this isn't working. will attempt to fix the other errors and see if it builds anyway.
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedAlbum(album.IntoList()));
                    break;
                case ICorePlaylist playlist:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedPlaylist(playlist.IntoList()));
                    break;
                case ICoreTrack track:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedTrack(track.IntoList()));
                    break;
                case ICoreDiscoverables discoverables:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedDiscoverables(discoverables.IntoList()));
                    break;
                case ICoreLibrary library:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedLibrary(library.IntoList()));
                    break;
                case ICoreRecentlyPlayed recentlyPlayed:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedRecentlyPlayed(recentlyPlayed.IntoList()));
                    break;

                // TODO: Search results post search redo
                case ICorePlayableCollectionGroup playableCollection:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedPlayableCollectionGroup(playableCollection.IntoList()));
                    break;
                case ICoreAlbumCollection albumCollection:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedAlbumCollection(albumCollection.IntoList()));
                    break;
                case ICoreArtistCollection artistCollection:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedArtistCollection(artistCollection.IntoList()));
                    break;
                case ICorePlaylistCollection playlistCollection:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedPlaylistCollection(playlistCollection.IntoList()));
                    break;
                case ICoreTrackCollection trackCollection:
                    collection.Add((IMerged<TCoreCollectionItem>)new MergedTrackCollection(trackCollection.IntoList()));
                    break;
                default:
                    throw new NotImplementedException();
                    // Remove the above when this is fully finished.
                    ThrowHelper.ThrowNotSupportedException<IMerged<TCoreCollection>>("Couldn't create merged item. Type not supported.");
            }
        }

        private async Task<IReadOnlyList<TCollectionItem>> GetItemsByRank(int limit, int offset)
        {
            Guard.IsNotNull(_coreRanking, nameof(_coreRanking));

            // Rebuild the sorted map so we're sure it's sorted correctly.
            _sortedMap.AddRange(BuildSortedMapRanked());

            // Get all requested items using the sorted map
            for (var i = offset; i < limit; i++)
            {
                var currentSource = _sortedMap[i];
                var itemsCountForSource = currentSource.SourceCollection.GetItemsCount<TCollection>();
                var itemLimitForSource = limit;

                // If the currentSource and the previous source are the same, skip this iteration
                // because we get the max items from each source once per collection.
                if (i > 0 && currentSource.SourceCollection.SourceCore == _sortedMap[i - 1].SourceCollection.SourceCore)
                    continue;

                // do we end up outside the range if we try getting all items from this source?
                if (currentSource.OriginalIndex + limit > itemsCountForSource)
                {
                    // If so, reduce limit so it only gets the remaining items in this collection.
                    itemLimitForSource = itemsCountForSource - currentSource.OriginalIndex;
                }

                var remainingItemsForSource = await OwlCore.Helpers.APIs.GetAllItemsAsync<TCoreCollectionItem>(
                    itemLimitForSource, // Try to get as many items as possible for each page.
                    currentSource.OriginalIndex,
                    async currentOffset => await currentSource.SourceCollection.GetItems<TCoreCollection, TCoreCollectionItem>(itemLimitForSource, currentOffset).ToListAsync().AsTask());

                // For each item that we just retrieved, find the index in the sorted maps and assign the item.
                for (var o = 0; o < remainingItemsForSource.Count; o++)
                {
                    var item = remainingItemsForSource[o];

                    _sortedMap[i + o].CollectionItem = item;
                }
            }

            var merged = (IReadOnlyList<TCollectionItem>)MergeMappedData(_sortedMap);

            return merged;
        }

        private List<MappedData> BuildSortedMap()
        {
            Guard.IsNotNull(_sortingMethod, nameof(_sortingMethod));

            return _sortingMethod switch
            {
                MergedCollectionSorting.Ranked => BuildSortedMapRanked(),
                _ => throw new NotSupportedException($"Merged collection sorting by \"{_sortingMethod}\" not supported.")
            };
        }

        private List<MappedData> BuildSortedMapRanked()
        {
            Guard.IsNotNull(_coreRanking, nameof(_coreRanking));

            // Rank the sources by core
            var rankedSources = new List<TCoreCollection>();
            foreach (var coreType in _coreRanking)
            {
                var source = Sources.First(x => x.SourceCore.GetType() == coreType);
                rankedSources.Add(source);
            }

            // Create the map for each possible item returned from a source collection.
            var itemsMap = new List<MappedData>();

            foreach (var source in rankedSources)
            {
                var itemsCount = source.GetItemsCount<TCollection>();

                for (var i = 0; i < itemsCount; i++)
                {
                    itemsMap.Add(new MappedData(i, source));
                }
            }

            return itemsMap;
        }

        private List<IMerged<TCoreCollectionItem>> MergeMappedData(IList<MappedData> sortedData)
        {
            var returnedValue = new List<IMerged<TCoreCollectionItem>>();

            foreach (var item in sortedData)
            {
                if (item.CollectionItem is null)
                    continue;

                MergeOrAdd(returnedValue, item.CollectionItem);
            }

            return returnedValue;
        }

        private async Task<IReadOnlyList<Type>> GetCoreRankings()
        {
            return await _settingsService.GetValue<IReadOnlyList<Type>>(nameof(SettingsKeys.CoreRanking));
        }

        private async Task<MergedCollectionSorting> GetSortingMethod()
        {
            return await _settingsService.GetValue<MergedCollectionSorting>(nameof(SettingsKeys.MergedCollectionSorting));
        }

        private void SettingsServiceOnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            switch (e.Key)
            {
                case nameof(SettingsKeys.CoreRanking):
                    _coreRanking = e.Value as IReadOnlyList<Type>;
                    break;
                case nameof(SettingsKeys.MergedCollectionSorting) when e.Value != null:
                    _sortingMethod = (MergedCollectionSorting)e.Value;
                    break;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Handles the internal merged map when a source is added (when one collection is merged into another)
        /// </remarks>
        public void AddSource(TCoreCollection itemToMerge)
        {
            // When a new source is added, that source could be anywhere in a ranked map, or the data could be scattered or mingled arbitrarily.
            // To keep the collection sorted by the user's preferred method
            // We re-get all the data, which includes rebuilding the collection map 
            // Then re-emit ALL data

            // TODO: Optimize this (these instruction for ranked sorting only)
            // Find where this source lies in the ranking
            // If the items have already been requested and another source returned them
            // Get all the items from ONLY the new source 
            // "insert" these and every item that shifted from the insert
            // By firing the event with removed, then again with added
            Task.Run(async () =>
                {
                    var itemsFromPreviousMerge = _sortedMap.ToList();
                    _sortedMap.Clear();

                    foreach (var item in itemsFromPreviousMerge)
                    {
                        var i = itemsFromPreviousMerge.IndexOf(item);

                        // If the currentSource and the previous source are the same, skip this iteration
                        // because we get and re-emit the range of items for this source.
                        if (i > 0 && item.SourceCollection.SourceCore == _sortedMap[i - 1].SourceCollection.SourceCore)
                            continue;

                        // The items retrieved will exist in the sorted map.
                        await GetItems(item.OriginalIndex, i);
                    }

                    var addedItems = new List<CollectionChangedEventItem<TCoreCollectionItem>>();

                    // For each item that we just retrieved, find the index in the sorted map and assign the item.
                    for (var o = 0; o < _sortedMap.Count; o++)
                    {
                        var addedItem = _sortedMap[o];

                        Guard.IsNotNull(addedItem.CollectionItem, nameof(addedItem.CollectionItem));

                        var x = new CollectionChangedEventItem<TCoreCollectionItem>(addedItem.CollectionItem, o);
                        addedItems.Add(x);
                    }

                    // logic for removed was copy-pasted and tweaked from the added logic. Not checked or tested.
                    var removedItems = new List<CollectionChangedEventItem<TCoreCollectionItem>>();

                    for (var o = 0; o < itemsFromPreviousMerge.Count; o++)
                    {
                        var addedItem = itemsFromPreviousMerge[o];

                        Guard.IsNotNull(addedItem.CollectionItem, nameof(addedItem.CollectionItem));

                        var x = new CollectionChangedEventItem<TCoreCollectionItem>(addedItem.CollectionItem, o);
                        removedItems.Add(x);
                    }

                    ItemsChanged?.Invoke(this, addedItems, removedItems);
                })
                .FireAndForget();
        }

        /// <inheritdoc />
        public bool Equals(TCoreCollection other)
        {
            // We're just here for the Merged, not the Equatable.
            // TSourceCollection and MergedCollectionMap have no overlap and never equal each other.
            return false;
        }

        private class MappedData
        {
            public MappedData(int originalIndex, TCoreCollection sourceCollection, TCoreCollectionItem? collectionItem = null)
            {
                OriginalIndex = originalIndex;
                SourceCollection = sourceCollection;
                CollectionItem = collectionItem;
            }

            public int OriginalIndex { get; }

            public TCoreCollection SourceCollection { get; }

            public TCoreCollectionItem? CollectionItem { get; set; }
        }
    }
}