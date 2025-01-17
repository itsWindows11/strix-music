﻿using System.Collections.Generic;
using System.Linq;
using OwlCore;
using StrixMusic.Sdk.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic.Sdk.WinUI.Controls.Shells
{
    /// <summary>
    /// Displays the content of a PlayableCollectionGroupViewModel in a Pivot.
    /// </summary>
    public sealed partial class PlayableCollectionGroupPivot : Control
    {
        private static readonly Dictionary<string, int> _pivotItemPositionMemo = new();

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> for the <see cref="RestoreSelectedPivot"/> property.
        /// </summary>
        public static readonly DependencyProperty RestoreSelectedPivotProperty =
            DependencyProperty.Register(
                nameof(RestoreSelectedPivot),
                typeof(bool),
                typeof(PlayableCollectionGroupPivot),
                new PropertyMetadata(false));

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> for the <see cref="PivotTitle"/> property.
        /// </summary>
        public static readonly DependencyProperty PivotTitleProperty =
            DependencyProperty.Register(
                nameof(PivotTitle),
                typeof(string),
                typeof(PlayableCollectionGroupPivot),
                new PropertyMetadata(string.Empty, (d, e) => ((PlayableCollectionGroupPivot)d).SetPivotTitle((string)e.NewValue)));

        /// <summary>
        /// If true, remember and restore the last pivot that the user had selected when the control is loaded.
        /// </summary>
        public bool RestoreSelectedPivot
        {
            get => (bool)GetValue(RestoreSelectedPivotProperty);
            set => SetValue(RestoreSelectedPivotProperty, value);
        }

        /// <summary>
        /// If true, remember and restore the last pivot that the user had selected when the control is loaded.
        /// </summary>
        public string PivotTitle
        {
            get => (string)GetValue(PivotTitleProperty);
            set => SetValue(PivotTitleProperty, value);
        }

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> for the <see cref="AllEmptyContent"/> property.
        /// </summary>
        public static readonly DependencyProperty AllEmptyContentProperty =
            DependencyProperty.Register(
                nameof(AllEmptyContent),
                typeof(string),
                typeof(PlayableCollectionGroupPivot),
                new PropertyMetadata(null, (d, e) => ((PlayableCollectionGroupPivot)d).SetNoContentTemplate((FrameworkElement)e.NewValue)));

        /// <summary>
        /// The content to show when all the collections in this <see cref="IPlaylistCollectionViewModel"/> are empty.
        /// </summary>
        public FrameworkElement AllEmptyContent
        {
            get => (FrameworkElement)GetValue(AllEmptyContentProperty);
            set => SetValue(AllEmptyContentProperty, value);
        }

        /// <summary>
        /// The backing <see cref="DependencyProperty"/> for the <see cref="HideEmptyPivots"/> property.
        /// </summary>
        public static readonly DependencyProperty HideEmptyPivotsProperty =
            DependencyProperty.Register(
                nameof(HideEmptyPivots),
                typeof(bool),
                typeof(PlayableCollectionGroupPivot),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether or not to hide pivots with no content.
        /// </summary>
        public bool HideEmptyPivots
        {
            get => (bool)GetValue(HideEmptyPivotsProperty);
            set => SetValue(HideEmptyPivotsProperty, value);
        }

        /// <summary>
        /// The primary pivot displayed by this control.
        /// </summary>
        public Pivot? PART_Pivot { get; private set; }

        /// <summary>
        /// The pivot item that displays an <see cref="ITrackCollectionViewModel" />
        /// </summary>
        public PivotItem? PART_SongsPivotItem { get; private set; }

        /// <summary>
        /// The pivot item that displays an <see cref="IAlbumCollectionViewModel" />
        /// </summary>
        public PivotItem? PART_AlbumsPivotItem { get; private set; }

        /// <summary>
        /// The pivot item that displays an <see cref="IArtistCollectionViewModel" />
        /// </summary>
        public PivotItem? PART_ArtistsPivotItem { get; private set; }

        /// <summary>
        /// The pivot item that displays an <see cref="IPlaylistCollectionViewModel" />
        /// </summary>
        public PivotItem? PART_PlaylistsPivotItem { get; private set; }

        /// <inheritdoc cref="AllEmptyContent"/>
        public ContentPresenter? PART_AllEmptyContentPresenter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayableCollectionGroupPivot"/> class.
        /// </summary>
        public PlayableCollectionGroupPivot()
        {
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Pivot = GetTemplateChild(nameof(PART_Pivot)) as Pivot;

            PART_SongsPivotItem = GetTemplateChild(nameof(PART_SongsPivotItem)) as PivotItem;
            PART_AlbumsPivotItem = GetTemplateChild(nameof(PART_AlbumsPivotItem)) as PivotItem;
            PART_ArtistsPivotItem = GetTemplateChild(nameof(PART_ArtistsPivotItem)) as PivotItem;
            PART_PlaylistsPivotItem = GetTemplateChild(nameof(PART_PlaylistsPivotItem)) as PivotItem;
            PART_AllEmptyContentPresenter = GetTemplateChild(nameof(PART_AllEmptyContentPresenter)) as ContentPresenter;

            RestoreMostRecentSelectedPivot();

            AttachEvents();

            ToggleAnyEmptyPivotItems();
            SetNoContentTemplate(AllEmptyContent);
        }

        private void AttachEvents()
        {
            Unloaded += PlayableCollectionGroupPivot_Unloaded;

            if (PART_Pivot != null)
            {
                PART_Pivot.SelectionChanged += PivotSelectionChanged;
            }

            if (ViewModel != null)
            {
                ViewModel.Tracks.CollectionChanged += AllItems_CollectionChanged;
                ViewModel.Albums.CollectionChanged += AllItems_CollectionChanged;
                ViewModel.Artists.CollectionChanged += AllItems_CollectionChanged;
                ViewModel.Playlists.CollectionChanged += AllItems_CollectionChanged;
            }
        }

        private void AllItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Threading.OnPrimaryThread(ToggleAnyEmptyPivotItems);
        }

        private void DetachEvents()
        {
            Unloaded -= PlayableCollectionGroupPivot_Unloaded;

            if (PART_Pivot != null)
            {
                PART_Pivot.SelectionChanged -= PivotSelectionChanged;
            }

            if (ViewModel != null)
            {
                ViewModel.Tracks.CollectionChanged -= AllItems_CollectionChanged;
                ViewModel.Albums.CollectionChanged -= AllItems_CollectionChanged;
                ViewModel.Artists.CollectionChanged -= AllItems_CollectionChanged;
                ViewModel.Playlists.CollectionChanged -= AllItems_CollectionChanged;
            }
        }

        private void PlayableCollectionGroupPivot_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachEvents();
        }

        /// <summary>
        /// Used to handle saving of most recently selected pivot.
        /// </summary>
        public void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_Pivot == null)
                return;

            _pivotItemPositionMemo[ViewModel.Id] = PART_Pivot.SelectedIndex;
        }

        /// <summary>
        /// Changes the <see cref="Pivot.Title"/> on the controls' primary <see cref="PART_Pivot"/>.
        /// </summary>
        /// <param name="title">The title to set. </param>
        public void SetPivotTitle(string title)
        {
            if (PART_Pivot != null)
            {
                PART_Pivot.Title = title;
            }
        }

        /// <summary>
        /// Sets the content to show when all the collections in this <see cref="IPlaylistCollectionViewModel"/> are empty.
        /// </summary>
        /// <param name="frameworkElement"></param>
        public void SetNoContentTemplate(FrameworkElement frameworkElement)
        {
            if (PART_AllEmptyContentPresenter != null)
            {
                PART_AllEmptyContentPresenter.Content = frameworkElement;
            }
        }

        private void RestoreMostRecentSelectedPivot()
        {
            if (!RestoreSelectedPivot || PART_Pivot == null)
                return;

            var pivotSelectionMemo = _pivotItemPositionMemo;

            if (pivotSelectionMemo != null && pivotSelectionMemo.TryGetValue(ViewModel.Id, out int value))
            {
                PART_Pivot.SelectedIndex = value;
            }
        }

        private void ToggleAnyEmptyPivotItems()
        {
            if (!HideEmptyPivots)
                return;

            TogglePivotItemViaCollectionCount(nameof(PART_SongsPivotItem), PART_SongsPivotItem, ViewModel.Tracks);

            TogglePivotItemViaCollectionCount(nameof(PART_AlbumsPivotItem), PART_AlbumsPivotItem, ViewModel.Albums);

            TogglePivotItemViaCollectionCount(nameof(PART_ArtistsPivotItem), PART_ArtistsPivotItem, ViewModel.Artists);

            TogglePivotItemViaCollectionCount(nameof(PART_PlaylistsPivotItem), PART_PlaylistsPivotItem, ViewModel.Playlists);

            if (PART_AllEmptyContentPresenter != null)
            {
                var allEmpty = !ViewModel.Tracks.Any() && !ViewModel.Albums.Any() && !ViewModel.Artists.Any() && !ViewModel.Playlists.Any();

                PART_AllEmptyContentPresenter.Visibility = allEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TogglePivotItemViaCollectionCount(string pivotItemName, PivotItem? pivotItem, IEnumerable<object> collectionToCheck)
        {
            if (PART_Pivot == null || pivotItem == null)
                return;

            var pivotPositionExists = _pivotItemPositionMemo.TryGetValue(pivotItemName, out var position);

            var toCheck = collectionToCheck as object[] ?? collectionToCheck.ToArray();
            if (!toCheck.Any() && PART_Pivot.Items.Contains(pivotItem))
            {
                if (!pivotPositionExists)
                {
                    _pivotItemPositionMemo.Add(pivotItemName, PART_Pivot.Items.IndexOf(pivotItem));
                }

                PART_Pivot.Items.Remove(pivotItem);
            }

            if (!PART_Pivot.Items.Contains(pivotItem) && pivotPositionExists && toCheck.Any())
            {
                PART_Pivot.Items.Insert(position, pivotItem);
            }
        }

        /// <summary>
        /// The ViewModel for this control.
        /// </summary>
        public PlayableCollectionGroupViewModel ViewModel => (PlayableCollectionGroupViewModel)DataContext;
    }
}
