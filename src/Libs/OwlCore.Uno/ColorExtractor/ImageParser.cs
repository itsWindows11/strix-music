﻿using OwlCore.Uno.ColorExtractor.ColorSpaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OwlCore.Uno.ColorExtractor
{
    /// <summary>
    /// A class of methods for working with images.
    /// </summary>
    public static class ImageParser
    {
        /// <summary>
        /// Gets an <see cref="Image{TPixel}"/> with <see cref="Argb32"/> format from a url.
        /// </summary>
        /// <param name="url">The url of the image to load.</param>
        /// <returns>An Argb32 image.</returns>
        public static async Task<Image<Argb32>?> GetImage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            return (await Image.LoadAsync(await GetImageStreamAsync(url))).CloneAs<Argb32>();
        }


        private static async Task<Stream> GetImageStreamAsync(string url)
        {
            var response = await HttpWebRequest.CreateHttp(url).GetResponseAsync();
            return response.GetResponseStream();
        }

        /// <summary>
        /// Gets colors out of an image.
        /// </summary>
        /// <param name="image">The image to get colors from.</param>
        /// <param name="quality">The approxiamate amount of pixels to get/</param>
        /// <returns>An array of colors that appeared as pixels in the image.</returns>
        public static RGBColor[] GetImageColors(
            Image<Argb32> image,
            int quality = 1920)
        {
            int nth = image.Width * image.Height / quality;

            int pixelsPerRow = image.Width / nth;

            RGBColor[] colors = new RGBColor[image.Height * pixelsPerRow];


            int pos = 0;
            for (int row = 0; row < image.Height; row++)
            {
                Span<Argb32> rowPixels = image.GetPixelRowSpan(row);
                for (int i = 0; i < pixelsPerRow; i++)
                {
                    float b = rowPixels[i * nth].B / 255f;
                    float g = rowPixels[i * nth].G / 255f;
                    float r = rowPixels[i * nth].R / 255f;
                    //float a = rowPixels[i].A / 255;

                    RGBColor color = new RGBColor(r, g, b);

                    colors[pos] = color;
                    pos++;
                }
            }

            return colors;
        }
    }
}