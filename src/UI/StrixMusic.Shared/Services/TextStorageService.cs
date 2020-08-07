﻿using System;
using System.Threading.Tasks;
using StrixMusic.Services.StorageService;
using Windows.Storage;

namespace Strix_Music.Services
{
    /// <inheritdoc cref="ITextStorageService"/>
    public class TextStorageService : ITextStorageService
    {
        private readonly StorageFolder _localFolder;

        /// <summary>
        /// Initializes a new instance of this <see cref="TextStorageService"/>
        /// </summary>
        public TextStorageService()
        {
            _localFolder = ApplicationData.Current.LocalFolder;
        }

        /// <inheritdoc />
        public async Task<bool> FileExistsAsync(string filename)
        {
            try
            {
                var file = await _localFolder.GetFileAsync(filename);
                return file != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> FileExistsAsync(string filename, string path)
        {
            try
            {
                var file = await _localFolder.GetFileAsync(path + filename);
                return file != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetValueAsync(string filename)
        {
            try
            {
                var file = await _localFolder.GetFileAsync(filename);
                var value = await FileIO.ReadTextAsync(file);
                return value;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetValueAsync(string filename, string path)
        {
            try
            {
                var file = await _localFolder.GetFileAsync(path + filename);
                var value = await FileIO.ReadTextAsync(file);
                return value;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task SetValueAsync(string filename, string value)
        {
            StorageFile fileHandle;
            try
            {
                fileHandle = await _localFolder.GetFileAsync(filename);
            }
            catch (Exception)
            {
                fileHandle = await _localFolder.CreateFileAsync(filename);
            }

            await FileIO.WriteTextAsync(fileHandle, value);
        }

        /// <inheritdoc />
        public async Task SetValueAsync(string filename, string value, string path)
        {
            StorageFile fileHandle;
            try
            {
                fileHandle = await _localFolder.GetFileAsync(filename);
            }
            catch (Exception)
            {
                fileHandle = await _localFolder.CreateFileAsync(filename);
            }

            await FileIO.WriteTextAsync(fileHandle, value);
        }
    }
}