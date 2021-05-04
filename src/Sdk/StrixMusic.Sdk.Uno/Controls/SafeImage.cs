﻿using Microsoft.Toolkit.Diagnostics;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Uno.Helpers;
using StrixMusic.Sdk.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace StrixMusic.Sdk.Uno.Controls
{
    /// <summary>
    /// An control that displays an image, or a backup image if there is none.
    /// </summary>
    /// <remarks>
    /// This control takes an <see cref="IImageCollection"/> and displays only the first, or none if none exist.
    /// </remarks>
    [TemplatePart(Name = nameof(PART_ImageRectangle), Type = typeof(Rectangle))]
    [TemplatePart(Name = nameof(PART_Fallback), Type = typeof(FrameworkElement))]
    public sealed partial class SafeImage : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeImage"/> class.
        /// </summary>
        public SafeImage()
        {
            DefaultStyleKey = typeof(SafeImage);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Loaded += SafeImage_Loaded;
        }

        /// <summary>
        /// The <see cref="IImageCollection"/> ViewModel of the control.
        /// </summary>
        public IImageCollectionViewModel ViewModel => (IImageCollectionViewModel)DataContext;

        private Rectangle? PART_ImageRectangle { get; set; }

        private ImageBrush? PART_ImageBrush { get; set; }

        private FrameworkElement? PART_Fallback { get; set; }

        private void AttachHandlers()
        {
            Unloaded += SafeImage_Unloaded;
            DataContextChanged += SafeImage_DataContextChanged;

            Guard.IsNotNull(PART_ImageBrush, nameof(PART_ImageBrush));
            PART_ImageBrush.ImageFailed += SafeImage_ImageFailed;
        }

        private void DetachHandlers()
        {
            Unloaded -= SafeImage_Unloaded;
            DataContextChanged -= SafeImage_DataContextChanged;

            Guard.IsNotNull(PART_ImageBrush, nameof(PART_ImageBrush));
            PART_ImageBrush.ImageFailed -= SafeImage_ImageFailed;
        }

        private async void SafeImage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SafeImage_Loaded;

            // Find Parts
            PART_ImageRectangle = VisualTreeHelpers.GetDataTemplateChild<Rectangle>(this, nameof(PART_ImageRectangle));
            PART_Fallback = VisualTreeHelpers.GetDataTemplateChild<FrameworkElement>(this, nameof(PART_Fallback));

            if (PART_ImageRectangle?.Fill is ImageBrush imgBrush)
                PART_ImageBrush = imgBrush;
            else
                ThrowHelper.ThrowInvalidDataException($"{nameof(PART_ImageRectangle)}'s fill must an ImageBrush.");

            await RequestImages();
            AttachHandlers();
        }

        private void SafeImage_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachHandlers();
        }

        private async void SafeImage_DataContextChanged(DependencyObject sender, DataContextChangedEventArgs args)
        {
            await RequestImages();
        }

        private void SafeImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Go to fallback if the image failed to load.
            OverrideToFallback();
        }

        private async Task RequestImages()
        {
            if (ViewModel?.PopulateMoreImagesCommand.IsRunning == false)
                await ViewModel.PopulateMoreImagesCommand.ExecuteAsync(1);
        }

        /// <summary>
        /// Set to Fallback regardless of if there's an image present.
        /// </summary>
        private void OverrideToFallback()
        {
            Guard.IsNotNull(PART_Fallback, nameof(PART_Fallback));
            PART_Fallback.Visibility = Visibility.Visible;
        }
    }
}
