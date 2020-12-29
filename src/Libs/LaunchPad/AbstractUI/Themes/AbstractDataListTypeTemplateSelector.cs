﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LaunchPad.AbstractUI.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractUI.Models;

namespace LaunchPad.AbstractUI.Themes
{
    /// <summary>
    /// Selects the template that is used for an <see cref="AbstractDataList"/> based on the <see cref="AbstractDataList.PreferredDisplayMode"/>.
    /// </summary>
    public class AbstractDataListTypeTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The data template used to display an <see cref="AbstractTextBox"/>.
        /// </summary>
        public DataTemplate? GridTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractDataList"/>.
        /// </summary>
        public DataTemplate? ListTemplate { get; set; }

        /// <inheritdoc />
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is AbstractMutableDataListViewModel mutableViewModel)
            {
                return mutableViewModel.PreferredDisplayMode switch
                {
                    AbstractDataListPreferredDisplayMode.Grid => GridTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    AbstractDataListPreferredDisplayMode.List => ListTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    _ => throw new NotImplementedException(),
                };
            }

            if (item is AbstractDataListViewModel viewModel)
            {
                return viewModel.PreferredDisplayMode switch
                {
                    AbstractDataListPreferredDisplayMode.Grid => GridTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    AbstractDataListPreferredDisplayMode.List => ListTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    _ => throw new NotImplementedException(),
                };
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}