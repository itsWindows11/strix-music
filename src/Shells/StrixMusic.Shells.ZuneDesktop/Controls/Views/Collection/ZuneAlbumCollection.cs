﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Collections;
using Microsoft.Toolkit.Uwp.UI.Animations;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.ViewModels;
using StrixMusic.Sdk.WinUI.Controls.Collections;
using StrixMusic.Shells.ZuneDesktop.Controls.Views.Collections;
using StrixMusic.Shells.ZuneDesktop.Controls.Views.Items;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace StrixMusic.Shells.ZuneDesktop.Controls.Views.Collection
{
    /// <summary>
    /// The collection to perform zune specific behaviors.
    /// </summary>
    public class ZuneAlbumCollection : AlbumCollection
    {
        private object _lockObj = new object();
        private ResourceLoader _loacalizationService;

        /// <summary>
        /// Creates a new instance of <see cref="ZuneAlbumCollection"/>.
        /// </summary>
        public ZuneAlbumCollection()
        {
            AlbumGroupedByCollection = new ObservableGroupedCollection<string, IAlbumCollectionItem>();

            _loacalizationService = ResourceLoader.GetForCurrentView("StrixMusic.Shells.ZuneDesktop/ZuneSettings");
        }

        /// <summary>
        /// Holds the current state of the zune <see cref="CollectionContent"/>.
        /// </summary>
        public ObservableGroupedCollection<string, IAlbumCollectionItem> AlbumGroupedByCollection
        {
            get { return (ObservableGroupedCollection<string, IAlbumCollectionItem>)GetValue(AlbumGroupedByCollectionProperty); }
            set { SetValue(AlbumGroupedByCollectionProperty, value); }
        }

        /// <summary>
        /// Dependency property for <ses cref="CollectionContent" />.
        /// </summary>
        public static readonly DependencyProperty AlbumGroupedByCollectionProperty =
            DependencyProperty.Register(nameof(AlbumGroupedByCollection), typeof(ObservableGroupedCollection<string, List<IAlbumCollectionItem>>), typeof(ZuneAlbumCollection), new PropertyMetadata(null, null));

        /// <summary>
        /// Holds the current state of the zune <see cref="CollectionContent"/>.
        /// </summary>
        public CollectionContentType ZuneCollectionType
        {
            get { return (CollectionContentType)GetValue(ZuneCollectionTypeProperty); }
            set { SetValue(ZuneCollectionTypeProperty, value); }
        }

        /// <summary>
        /// Dependency property for <ses cref="CollectionContent" />.
        /// </summary>
        public static readonly DependencyProperty ZuneCollectionTypeProperty =
            DependencyProperty.Register(nameof(ZuneCollectionType), typeof(CollectionContent), typeof(ZuneAlbumCollection), new PropertyMetadata(CollectionContentType.Albums, null));

        /// <summary>
        /// Holds the instance of the sort textblock.
        /// </summary>
        public TextBlock? PART_SortLbl { get; private set; }

        /// <summary>
        /// Flag to determine if albums are already loaded.
        /// </summary>
        public bool AlbumsLoaded { get; private set; }

        /// <summary>
        /// The AlbumCollection GridView control.
        /// </summary>
        public GridView? PART_Selector { get; private set; }

        /// <summary>
        /// Dependency property for <see cref="SortState" />.
        /// </summary>
        public static readonly DependencyProperty SortStateProperty =
            DependencyProperty.Register(nameof(SortState), typeof(ZuneSortState), typeof(ZuneArtistCollection), new PropertyMetadata(ZuneSortState.AZ, null));

        /// <summary>
        /// Holds the current sort state of the zune <see cref="ZuneAlbumCollection"/>.
        /// </summary>
        public ZuneSortState SortState
        {
            get { return (ZuneSortState)GetValue(SortStateProperty); }
            set { SetValue(SortStateProperty, value); }
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Selector = GetTemplateChild(nameof(PART_Selector)) as GridView;
            PART_SortLbl = GetTemplateChild(nameof(PART_SortLbl)) as TextBlock;

            Guard.IsNotNull(PART_SortLbl, nameof(PART_SortLbl));
            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));

            PART_Selector.Loaded += PART_Selector_Loaded;
            Unloaded += ZuneAlbumCollection_Unloaded;

            PART_SortLbl.Tapped += PART_SortLbl_Tapped;
        }

        private void PART_SortLbl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));
            Guard.IsNotNull(PART_SortLbl, nameof(PART_SortLbl));
            Guard.IsNotNull(Collection, nameof(Collection));

            switch (SortState)
            {
                // State defines the next sort state in queue which differs from the current state.
                case ZuneSortState.AZ:
                    Collection.SortAlbumCollection(AlbumSortingType.Alphanumerical, SortDirection.Descending);
                    PART_Selector.ItemsSource = Collection.Albums;
                    SortState = ZuneSortState.ZA;
                    PART_SortLbl.Text = _loacalizationService.GetString("Z-A");
                    break;
                case ZuneSortState.ZA:
                    PopulateAlbumGroupedByArtists();
                    SortState = ZuneSortState.Artists;
                    PART_SortLbl.Text = _loacalizationService.GetString("By Artists");
                    break;
                case ZuneSortState.Artists:
                    PopulateAlbumGroupedByReleaseYear();
                    SortState = ZuneSortState.ReleaseYear;
                    PART_SortLbl.Text = _loacalizationService.GetString("By Release Year");
                    break;
                case ZuneSortState.ReleaseYear:
                    Collection.SortAlbumCollection(AlbumSortingType.DateAdded, SortDirection.Ascending);
                    PART_Selector.ItemsSource = Collection.Albums;
                    SortState = ZuneSortState.DateAdded;
                    PART_SortLbl.Text = _loacalizationService.GetString("By Date Added");
                    break;
                case ZuneSortState.DateAdded:
                    Collection.SortAlbumCollection(AlbumSortingType.Alphanumerical, SortDirection.Ascending);
                    PART_Selector.ItemsSource = Collection.Albums;
                    SortState = ZuneSortState.AZ;
                    PART_SortLbl.Text = _loacalizationService.GetString("A-Z");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private async void Albums_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Intentional delay (for safe side), this doesn't freeze anything as event attachment can be done silently, it waits for the emitted album to load in Visual Tree.
                // There is no event on GridView that tells when a UI Element is added to Items list.
                // ------------
                // TODO: We shouldn't be using a delay to do this.
                // ------------
                // TODO: this method of getting a loaded item does not work with virtualization.
                // Instead
                // - Create a custom ViewModel that wraps around AlbumViewModel
                // - Give it a "RequestPlaybackCommand" we can invoke from XAML, and a "PlaybackRequested" event that's raised by the command.
                // - Put it in a new ObservableCollection<TheNewViewModel> property. Bind to that in the UI instead of what we use now.
                // - Use behaviors in XAML to invoke this new command when the "ZuneAlbumItem.AlbumPlaybackTriggered" event fires.
                // - Sync the new ObservableCollection with the source, wrapping the data with our new ViewModel.
                // - Listen to the event on each item in this new ViewModel and invoke playback when it's called
                // - Clean up all the code from the existing approach.
                await Task.Delay(1000);
                
                Guard.IsNotNull(PART_Selector);
                Guard.IsNotNull(Collection);

                foreach (var item in e.NewItems)
                {
                    Guard.IsNotNull(item, nameof(item));

                    var index = Collection.Albums.IndexOf((IAlbumCollectionItem)item);
                    var gridViewItem = (GridViewItem)PART_Selector.ContainerFromIndex(index);

                    // PATCH! HACK! See above TODO.
                    if (gridViewItem is null)
                    {
                        continue;
                    }

                    var uiElement = gridViewItem.ContentTemplateRoot;
                    if (uiElement is ZuneAlbumItem zuneAlbumItem)
                    {
                        zuneAlbumItem.AlbumPlaybackTriggered += ZuneAlbumItem_AlbumPlaybackTriggered;
                        zuneAlbumItem.Unloaded += ZuneAlbumItem_Unloaded;
                        zuneAlbumItem.ZuneCollectionType = ZuneCollectionType;
                    }
                }

                try
                {
                    // Gives breathing space to the processor and reduces the odds the where observable collection changed during CollectionChanged event to minimum. 
                    lock (_lockObj)
                        Collection.SortAlbumCollection(AlbumSortingType.Alphanumerical, SortDirection.Ascending);
                }
                catch (InvalidOperationException)
                {
                    // It handles a rare case where observable collection changed during CollectionChanged event. 
                    // More precisely "Cannot change ObservableCollection during a CollectionChanged event."
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnCollectionChanged(IAlbumCollectionViewModel? oldValue, IAlbumCollectionViewModel? newValue)
        {
            if (oldValue is IAlbumCollectionViewModel oldCollection)
            {
                oldCollection.Albums.CollectionChanged -= Albums_CollectionChanged;
            }

            if (newValue is IAlbumCollectionViewModel newCollection)
            {
                newCollection.Albums.CollectionChanged += Albums_CollectionChanged;
            }

            base.OnCollectionChanged(oldValue, newValue);
        }

        private void ZuneAlbumItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ZuneAlbumItem zuneAlbum)
                zuneAlbum.AlbumPlaybackTriggered -= ZuneAlbumItem_AlbumPlaybackTriggered;
        }

        private void ZuneAlbumCollection_Unloaded(object sender, RoutedEventArgs e)
        {
            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));
            Guard.IsNotNull(PART_SortLbl, nameof(PART_SortLbl));

            PART_Selector.Loaded -= PART_Selector_Loaded;
            Unloaded -= ZuneAlbumCollection_Unloaded;
            PART_SortLbl.Tapped -= PART_SortLbl_Tapped;
        }

        private void PART_Selector_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateCollection();
        }

        /// <summary>
        /// Gets the list of the <see cref="UIElement"/> and animates it.
        /// </summary>
        public void AnimateCollection()
        {
            if (Collection is null)
                return;

            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));

            var uiElments = new List<UIElement>();

            int itemIndex = 0;
            foreach (var item in Collection.Albums)
            {
                // NOTE: ContainerFromItem doesn't work in uno.
                var gridViewItem = (GridViewItem)PART_Selector.ContainerFromIndex(itemIndex);

                if (gridViewItem == null)
                    break;

                var uiElement = gridViewItem.ContentTemplateRoot;

                if (uiElement is ZuneAlbumItem zuneAlbumItem)
                {
                    zuneAlbumItem.AlbumPlaybackTriggered += ZuneAlbumItem_AlbumPlaybackTriggered;
                    zuneAlbumItem.ZuneCollectionType = ZuneCollectionType;
                }

                // This needs to be explicitly casted to UIElement to avoid a compiler error specific to android in uno.
                uiElments.Add((UIElement)uiElement);
                itemIndex++;
            }

            FadeInAlbumCollectionItems(uiElments);
        }

        private void PopulateAlbumGroupedByReleaseYear()
        {
            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));
            Guard.IsNotNull(Collection, nameof(Collection));

            AlbumGroupedByCollection.Clear();

            var typecastedAlbums = Collection.Albums.Where(c => c is AlbumViewModel).Cast<AlbumViewModel>();

            AlbumGroupedByCollection = new ObservableGroupedCollection<string, IAlbumCollectionItem>(
            typecastedAlbums.GroupBy(c => c.DatePublished?.Year.ToString() ?? "Unknown")
            .OrderBy(g => g.Key));

            // Set up the CollectionViewSource.
            var cvs = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = AlbumGroupedByCollection,
            };

            PART_Selector.ItemsSource = cvs.View;
        }

        private void PopulateAlbumGroupedByArtists()
        {
            Guard.IsNotNull(PART_Selector, nameof(PART_Selector));
            Guard.IsNotNull(Collection, nameof(Collection));

            AlbumGroupedByCollection.Clear();

            var typecastedAlbums = Collection.Albums.Where(c => c is AlbumViewModel).Cast<AlbumViewModel>();

            AlbumGroupedByCollection = new ObservableGroupedCollection<string, IAlbumCollectionItem>(
           typecastedAlbums.GroupBy(c => c.Artists.FirstOrDefault()?.Name ?? "Untitled")
           .OrderBy(g => g.Key));

            // Set up the CollectionViewSource.
            var cvs = new CollectionViewSource
            {
                IsSourceGrouped = true,
                Source = AlbumGroupedByCollection,
            };

            PART_Selector.ItemsSource = cvs.View;
        }

        private async void ZuneAlbumItem_AlbumPlaybackTriggered(object sender, AlbumViewModel e)
        {
            Guard.IsNotNull(Collection);
            await Collection.PlayAlbumCollectionAsync(e);
        }

        private void FadeInAlbumCollectionItems(List<UIElement> uiElements)
        {
            double delay = 0;

            foreach (var item in uiElements)
            {
                var animationSet = new AnimationSet();
                var duration = 250;

                animationSet.Add(new OpacityAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(duration),
                    Delay = TimeSpan.FromMilliseconds(delay),
                    EasingMode = Windows.UI.Xaml.Media.Animation.EasingMode.EaseInOut,
                    EasingType = EasingType.Linear
                });
                delay += 75;

                animationSet.Start(item);
            }

            AlbumsLoaded = true;
        }
    }
}