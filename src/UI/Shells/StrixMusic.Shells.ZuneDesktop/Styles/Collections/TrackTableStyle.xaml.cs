﻿using Microsoft.Toolkit.Uwp.UI.Controls;
using StrixMusic.Sdk.ViewModels;
using StrixMusic.Sdk.ViewModels.Helpers.Sorting;
using System;
using Windows.UI.Xaml;

namespace StrixMusic.Shells.ZuneDesktop.Styles.Collections
{
    /// <summary>
    /// A <see cref="ResourceDictionary"/> containing the style and template for the <see cref="Sdk.Uno.Controls.TrackCollection"/> in the ZuneDesktop Shell.
    /// </summary>
    public sealed partial class TrackTableStyle : ResourceDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackTableStyle"/> class.
        /// </summary>
        public TrackTableStyle()
        {
            this.InitializeComponent();
        }

        private void ClearSortings(DataGrid grid)
        {
            foreach (var column in grid.Columns)
            {
                column.SortDirection = null;
            }
        }

        private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            SortColumn(grid, e.Column);
        }

        private void SortColumn(DataGrid grid, DataGridColumn column)
        {
            ITrackCollectionViewModel viewModel = (ITrackCollectionViewModel)grid.DataContext;

            TrackSortingType sortingType = TrackSortingType.Unsorted;
            SortDirection sortingDirection = SortDirection.Unsorted;

            switch (column.Tag)
            {
                case "Song":
                    sortingType = TrackSortingType.Alphanumerical;
                    break;
                case "Length":
                    sortingType = TrackSortingType.Duration;
                    break;
                case "DateAdded":
                    sortingType = TrackSortingType.DateAdded;
                    break;
                default:
                    return;
            }

            DataGridSortDirection? oldSortDirection = column.SortDirection;
            ClearSortings(grid);
            switch (oldSortDirection)
            {
                case null:
                case DataGridSortDirection.Descending:
                    column.SortDirection = DataGridSortDirection.Ascending;
                    sortingDirection = SortDirection.Ascending;
                    break;
                case DataGridSortDirection.Ascending:
                    column.SortDirection = DataGridSortDirection.Descending;
                    sortingDirection = SortDirection.Descending;
                    break;
            }

            viewModel.SortTrackCollection(sortingType, sortingDirection);
        }
    }
}