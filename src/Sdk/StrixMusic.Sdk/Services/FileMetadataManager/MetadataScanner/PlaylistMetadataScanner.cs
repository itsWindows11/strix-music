﻿using OwlCore;
using OwlCore.AbstractStorage;
using OwlCore.Extensions;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;
using StrixMusic.Sdk.Services.FileMetadataManager.Models.Playlist.Smil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace StrixMusic.Sdk.Services.FileMetadataManager.MetadataScanner
{
    /// <summary>
    /// Scans all playlist files.
    /// </summary>
    public class PlaylistMetadataScanner : IDisposable
    {
        private static readonly string[] _supportedPlaylistFileFormats = { ".zpl", ".wpl", ".smil", ".m3u", ".m3u8", ".vlc", ".xspf", ".asx", ".mpcpl", ".fpl", ".pls", ".aimppl4" };
        private readonly FileMetadataScanner _fileMetadataScanner;
        private readonly SemaphoreSlim _batchLock;
        private readonly IFolderData _rootFolder;
        private readonly string _emitDebouncerId = Guid.NewGuid().ToString();
        private readonly CancellationTokenSource _scanningCancellationTokenSource;
        private readonly List<FileMetadata> _batchMetadataToEmit = new List<FileMetadata>();

        private IEnumerable<IFileData>? _discoveredFiles;
        private IEnumerable<FileMetadata>? _scannedTracks;
        private int _filesProcessed;
        private int _totalFiles;

        /// <summary>
        /// Creates a new instance <see cref="PlaylistMetadataScanner"/>.
        /// </summary>
        /// <param name="fileCoreRootFolder"></param>
        /// <param name="fileMetadataScanner"></param>
        public PlaylistMetadataScanner(IFolderData fileCoreRootFolder, FileMetadataScanner fileMetadataScanner)
        {
            _fileMetadataScanner = fileMetadataScanner;
            _rootFolder = fileCoreRootFolder;
            _batchLock = new SemaphoreSlim(1, 1);
          
            _scanningCancellationTokenSource = new CancellationTokenSource();

            AttachEvents();
        }

        private void AttachEvents()
        {
            _fileMetadataScanner.FileDiscoveryCompleted += FileMetadataScanner_FileDiscoveryCompleted;
            _fileMetadataScanner.FileScanCompleted += FileMetadataScanner_FileScanCompleted;
        }

        private void DetachEvents()
        {
            _fileMetadataScanner.FileDiscoveryCompleted -= FileMetadataScanner_FileDiscoveryCompleted;
            _fileMetadataScanner.FileDiscoveryCompleted -= FileMetadataScanner_FileDiscoveryCompleted;
        }

        /// <summary>
        /// Flag that tells if the scanner is initialized or not.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Playlist metadata scanning completed.
        /// </summary>
        public event EventHandler? PlaylistMetadataScanCompleted;

        /// <summary>
        /// Processed file count updated.
        /// </summary>
        public event EventHandler? PlaylistMetadataProcessedFileCountUpdated;

        /// <summary>
        /// Raised when a new playlist with metadata is discovered.
        /// </summary>
        public event EventHandler<IEnumerable<FileMetadata>>? PlaylistMetadataAdded;

        /// <summary>
        /// Raised when a previously scanned file has been removed from the file system.
        /// </summary>
        public event EventHandler<IEnumerable<FileMetadata>>? PlaylistMetadataRemoved;

        private void FileMetadataScanner_FileDiscoveryCompleted(object sender, IEnumerable<IFileData> e)
        {
            _discoveredFiles = e;
        }

        private async void FileMetadataScanner_FileScanCompleted(object sender, IEnumerable<FileMetadata> e)
        {
            _fileMetadataScanner.FileScanCompleted -= FileMetadataScanner_FileScanCompleted;

            _scannedTracks = e;

            var playlists = _discoveredFiles.Where(c => _supportedPlaylistFileFormats.Contains(c.FileExtension));

            _totalFiles = playlists.Count();

            if (_totalFiles == 0)
            {
                PlaylistMetadataScanCompleted?.Invoke(this, EventArgs.Empty);
            }

            foreach (var item in playlists)
            {
                if (_scanningCancellationTokenSource.Token.IsCancellationRequested)
                    _scanningCancellationTokenSource.Token.ThrowIfCancellationRequested();

                await ProcessFile(item);
            }
        }

        private async Task<FileMetadata?> ProcessFile(IFileData file)
        {
            var fileMetadata = new FileMetadata();

            if (_supportedPlaylistFileFormats.Contains(file.FileExtension))
                fileMetadata = await ProcessPlaylistMetadata(file);

            _filesProcessed++;

            await _batchLock.WaitAsync();

            if (fileMetadata != null)
                _batchMetadataToEmit.Add(fileMetadata);

            PlaylistMetadataProcessedFileCountUpdated?.Invoke(this, EventArgs.Empty);

            _batchLock.Release();

            _ = HandleChanged();

            return fileMetadata;
        }

        private async Task HandleChanged()
        {
            await _batchLock.WaitAsync();

            if (_totalFiles != _filesProcessed && _batchMetadataToEmit.Count < 100 && !await Flow.Debounce(_emitDebouncerId, TimeSpan.FromSeconds(5)))
                return;

            PlaylistMetadataAdded?.Invoke(this, _batchMetadataToEmit.ToArray());

            _batchMetadataToEmit.Clear();

            _batchLock.Release();

            if (_totalFiles == _filesProcessed)
                PlaylistMetadataScanCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Scans playlist file for metadata.
        /// </summary>
        /// <param name="fileData">The path to the file.</param>
        /// <returns>Fully scanned <see cref="PlaylistMetadata"/>.</returns>
        public async Task<PlaylistMetadata?> ScanPlaylistMetadata(IFileData fileData)
        {
            PlaylistMetadata? playlistMetadata;
            switch (fileData.FileExtension)
            {
                case ".zpl":
                case ".wpl":
                case ".smil":
                    playlistMetadata = await GetSmilMetadata(fileData);
                    break;

                case ".m3u":
                case ".m3u8":
                case ".vlc":
                    playlistMetadata = await GetM3UMetadata(fileData);
                    break;

                case ".xspf":
                    playlistMetadata = await GetXspfMetadata(fileData);
                    break;

                case ".asx":
                    playlistMetadata = await GetAsxMetadata(fileData); // NOT TESTED
                    break;

                case ".mpcpl":
                    playlistMetadata = await GetMpcplMetadata(fileData); // NOT TESTED
                    break;

                case ".fpl":
                    playlistMetadata = await GetFplMetadata(fileData); // NOT TESTED
                    break;

                case ".pls":
                    playlistMetadata = await GetPlsMetadata(fileData); // NOT TESTED
                    break;

                case ".aimppl4":
                    playlistMetadata = await GetAimpplMetadata(fileData);
                    break;

                default:
                    // Format not supported
                    return null;
            }

            return playlistMetadata;
        }

        /// <summary>
        /// Resolves a potentially relative file path to an absolute path.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="currentPath">The path to append to <paramref name="path"/>
        /// if it's relative.</param>
        /// <remarks>
        /// This method is safe to use on absolute paths as well.
        /// Does not work for Unix paths.
        /// </remarks>
        /// <returns>An absolute path.</returns>
        public string ResolveFilePath(string path, string currentPath)
        {
            // Check if the path is absolute
            if (IsFullPath(path))
            {
                // Path is absolute
                return path;
            }
            else
            {
                // Path is relative
                string fullPath;
                if (path.StartsWith("~", StringComparison.InvariantCulture))
                {
                    // Unix relative file path
                    fullPath = Path.GetFullPath(Path.GetDirectoryName(currentPath) + path.Substring(1));
                }
                else
                {
                    fullPath = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(currentPath), path));
                }

                return fullPath;
            }
        }

        /// <summary>
        /// Resolves a potentially relative file path to an absolute path.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="fileData">The file to resolve paths relative to.</param>
        /// <remarks>This method is safe to use on absolute paths as well.</remarks>
        /// <returns>An absolute path.</returns>
        public string ResolveFilePath(string path, IFileData fileData)
        {
            return ResolveFilePath(path, Path.GetDirectoryName(fileData.Path));
        }

        /// <summary>
        /// Determines whether a given path is full or relative.
        /// </summary>
        private bool IsFullPath(string path)
        {
            // FIXME: http:// paths are not recognized as absolute paths
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.IsPathRooted(path))
                return false;

            var pathRoot = Path.GetPathRoot(path);
            if (pathRoot.Length <= 2 && pathRoot != "/") // Accepts X:\ and \\UNC\PATH, rejects empty string, \ and X:, but accepts / to support Linux
                return false;

            if (pathRoot[0] != '\\' || pathRoot[1] != '\\')
                return true; // Rooted and not a UNC path

            return pathRoot.Trim('\\').IndexOf('\\') != -1; // A UNC server name without a share name (e.g "\\NAME" or "\\NAME\") is invalid
        }

        /// <summary>
        /// Gets the SMIL metadata from the given file.
        /// </summary>
        /// <remarks>Recognizes Zune's ZPL and WMP's WPL.</remarks>
        private async Task<PlaylistMetadata?> GetSmilMetadata(IFileData fileData)
        {
            var ser = new XmlSerializer(typeof(Smil));

            using var stream = await fileData.GetStreamAsync();
            using var xmlReader = new XmlTextReader(stream);

            var smil = ser.Deserialize(xmlReader) as Smil;
            var playlist = new PlaylistMetadata
            {
                Id = fileData.Path.HashMD5Fast(),
            };

            var mediaList = smil?.Body?.Seq?.Media?.ToList();

            playlist.Title = smil?.Head?.Title;

            if (mediaList == null)
                return null;

            playlist.TrackIds = new List<string>();

            foreach (var media in mediaList)
            {
                if (media.Src != null)
                {
                    // checks if the track path is access-able. This is the fastest way. Not sure if its future proof.
                    if (media.Src.Contains(_rootFolder.Path))
                    {
                        playlist.Duration ??= default;

                        playlist.Duration = playlist.Duration?.Add(TimeSpan.FromMilliseconds(media.Duration));
                        playlist.TrackIds.Add(media.Src.HashMD5Fast());
                    }
                }

                playlist.TotalTracksCount++;
            }

            return playlist;
        }

        /// <summary>
        /// Gets the M3U metadata from the given file.
        /// </summary>
        /// <remarks>Recognizes both M3U (default encoding) and M3U8 (UTF-8 encoding).</remarks>
        private async Task<PlaylistMetadata?> GetM3UMetadata(IFileData fileData)
        {
            using var stream = await fileData.GetStreamAsync();

            using var content = fileData.FileExtension == ".m3u8" ? new StreamReader(stream, Encoding.UTF8) : new StreamReader(stream);

            var playlist = new PlaylistMetadata()
            {
                Id = fileData.Path.HashMD5Fast(),
            };

            while (!content.EndOfStream)
            {
                var line = await content.ReadLineAsync();

                // Handle M3U directives
                if (line[0] == '#')
                {
                    // --++ Extended M3U ++--
                    // Playlist display title
                    if (line.StartsWith("#PLAYLIST:", StringComparison.InvariantCulture))
                    {
                        playlist.Title = line.Split(':')[1];
                    }
                }
                else
                {
                    // Assume the line is a path to a music file
                    if (!line.Contains(_rootFolder.Path))
                        continue;

                    var hash = TryGetHashFromExistingTrackRepo(new Uri(line));

                    if (hash != null)
                    {
                        playlist.TrackIds ??= new List<string>();

                        playlist.TrackIds.Add(hash);
                    }
                }
            }

            playlist.Title ??= fileData.Name; // If the title is null, filename is assigned because if a playlist has no title its not visible to the user on UI.

            return playlist;
        }

        /// <summary>
        /// Gets the XSPF metadata from the given file.
        /// </summary>
        /// <remarks>Does not support any application extensions.</remarks>
        /// <summary>
        /// Gets the XSPF metadata from the given file.
        /// </summary>
        /// <remarks>Does not support any application extensions.</remarks>
        private async Task<PlaylistMetadata?> GetXspfMetadata(IFileData fileData)
        {
            using var stream = await fileData.GetStreamAsync();

            var doc = XDocument.Load(stream);
            var xmlRoot = doc.Root;
            var xmlns = xmlRoot.GetDefaultNamespace().NamespaceName;
            var trackList = xmlRoot.Element(XName.Get("trackList", xmlns));
            var trackListElements = trackList.Elements(XName.Get("track", xmlns));

            var listElements = trackListElements as XElement[] ?? trackListElements.ToArray();

            var playlist = new PlaylistMetadata()
            {
                Id = fileData.Path.HashMD5Fast(),
                Title = xmlRoot.Element(XName.Get("title", xmlns))?.Value,
                TotalTracksCount = listElements.Length,
                Description = xmlRoot.Element(XName.Get("annotation", xmlns))?.Value,
            };
            var url = xmlRoot.Element(XName.Get("info", xmlns))?.Value;
            if (url != null)
                playlist.Url = new Uri(url);

            foreach (var media in listElements)
            {
                var location = media.Element(XName.Get("location", xmlns))?.Value;

                if (location != null)
                {
                    playlist.TrackIds ??= new List<string>();
                    var localPath = new Uri(location).LocalPath;

                    if (localPath.Contains(_rootFolder.Path))
                    {
                        var hash = new Uri(location).LocalPath.HashMD5Fast();

                        playlist.TrackIds.Add(hash);
                    }
                }
            }

            return playlist;
        }

        /// <summary>
        /// Gets the ASX metadata from the given file.
        /// </summary>
        /// <remarks>Does not support ENTRYREF.</remarks>
        private async Task<PlaylistMetadata?> GetAsxMetadata(IFileData fileData)
        {
            using var stream = await fileData.GetStreamAsync();

            var doc = XDocument.Load(stream);
            var asx = doc.Root;
            var entries = asx.Elements("entry");
            var baseUrl = asx.Element("base")?.Value ?? string.Empty;

            var metadata = new PlaylistMetadata()
            {
                Title = asx.Element("title").Value,
                TotalTracksCount = entries.Count(),
            };

            // This is only temporary until we work out how to get track IDs
            var trackMetadata = new List<TrackMetadata>(metadata.TotalTracksCount);

            if (trackMetadata == null) return null;

            foreach (var entry in entries)
            {
                var entryBaseUrl = entry.Element("base")?.Value ?? string.Empty;

                // TODO: Where does the track ID come from?
                var track = new TrackMetadata
                {
                    Title = entry.Element("title")?.Value,
                    Url = new Uri(baseUrl + entryBaseUrl + entry.Element("ref").Attribute("href").Value),
                };
                var durString = entry.Element("duration")?.Value;
                if (durString != null)
                    track.Duration = TimeSpan.Parse(durString, CultureInfo.InvariantCulture);

                trackMetadata.Add(track);
            }

            // TODO: ASX files can reference other ASX files using entryRef.
            // It works kind of like UWP XAML ItemsPresenter:
            // https://docs.microsoft.com/en-us/windows/win32/wmp/entryref-element
            return metadata;
        }

        /// <summary>
        /// Gets the MPC-PL metadata from the given file.
        /// </summary>
        private async Task<PlaylistMetadata?> GetMpcplMetadata(IFileData fileData)
        {
            using var stream = await fileData.GetStreamAsync();
            using var content = new StreamReader(stream);

            var metadata = new PlaylistMetadata();
            var tracks = new List<TrackMetadata>();

            // Make sure the file is either a "pointer" to a folder
            // or an MPC playlist
            var firstLine = await content.ReadLineAsync();
            if (firstLine != "MPCPLAYLIST")
                return null;

            while (!content.EndOfStream)
            {
                var trackMetadata = new TrackMetadata();

                var line = await content.ReadLineAsync();
                var components = Regex.Match(line, @"^(?<idx>[0-9]+),(?<attr>[A-z]+),(?<val>.+)$").Groups;

                switch (components["attr"].Value)
                {
                    case "filename":
                        var fullPath = ResolveFilePath(components["val"].Value, fileData.Path);
                        trackMetadata.Url = new Uri(fullPath);

                        var idx = int.Parse(components["idx"].Value, CultureInfo.InvariantCulture);
                        if (idx >= tracks.Count)
                            tracks.Add(trackMetadata);
                        else
                            tracks.Insert(idx, trackMetadata);
                        break;

                    case "type":
                    // TODO: No idea what this is supposed to mean.
                    // It's not documented anywhere. Probably supposed to be an enum.
                    default:
                        // Unsupported attribute
                        break;
                }
            }

            return metadata;
        }

        private static readonly byte[] FplMagic = new byte[] { 0xE1, 0xA0, 0x9C, 0x91, 0xF8, 0x3C, 0x77, 0x42, 0x85, 0x2C, 0x3B, 0xCC, 0x14, 0x01, 0xD3, 0xF2 };

        /// <summary>
        /// Gets the FPL metadata from the given file.
        /// </summary>
        /// <remarks>
        /// Supports playlists created by foobar2000 v0.9.1 and newer.
        /// Based on the specification here: https://github.com/rr-/fpl_reader/blob/master/fpl-format.md
        /// </remarks>
        private async Task<PlaylistMetadata?> GetFplMetadata(IFileData fileData)
        {
            try
            {
                using var stream = await fileData.GetStreamAsync();
                using var content = new BinaryReader(stream);

                var metadata = new PlaylistMetadata();
                var tracks = new List<TrackMetadata>();

                // Make sure the file is an FPL
                var fileMagic = content.ReadBytes(FplMagic.Length);
                if (!fileMagic.SequenceEqual(FplMagic))
                    return null;

                // foobar2000 playlists don't have titles, so set it
                // to the file name
                metadata.Title = fileData.DisplayName;

                // Get size of meta
                var metaSize = content.ReadUInt32();

                // Read meta strings (null-terminated)
                var metaBytes = new byte[metaSize];
                var metaPos = stream.Position;
                await stream.ReadAsync(metaBytes, 0, metaBytes.Length);
                var metas = new List<string>();

                if (metas == null) return null;

                var metaTemp = string.Empty;

                foreach (var b in metaBytes)
                {
                    if (b == 0x00)
                    {
                        // End of string
                        metas.Add(metaTemp);
                        metaTemp = string.Empty;
                    }
                    else
                    {
                        // TODO: Is there a better way to do this
                        metaTemp += Encoding.UTF8.GetChars(new[] { b })[0];
                    }
                }

                // Get track count
                var trackCount = content.ReadUInt32();

                // Read track metadata
                var trackMetaData = new List<TrackMetadata>();

                if (trackMetaData == null) return null;

                for (var i = 0; i < trackCount; i++)
                {
                    var trackMetadata = new TrackMetadata();

                    // Get flags
                    var flags = content.ReadInt32();

                    // Get file name offset
                    var fileNameOffset = content.ReadUInt32();

                    // Retrieve file name
                    var curPos = stream.Position;
                    stream.Seek(metaPos + fileNameOffset, SeekOrigin.Begin);
                    trackMetadata.Url = new Uri(stream.ReadNullTerminatedString(Encoding.UTF8));
                    stream.Seek(curPos, SeekOrigin.Begin);

                    // Get sub-song index (for files containing multiple tracks, like chapters)
                    var subSongIndex = content.ReadUInt32();

                    // Check if the track has metadata
                    if ((flags & 1) == 0)
                    {
                        trackMetaData.Add(trackMetadata);
                        continue;
                    }

                    // Get track file size
                    var fileSize = content.ReadUInt64();

                    // Get track file time (last modified)
                    var fileTime = content.ReadUInt64();

                    // Get track duration
                    var durationSeconds = content.ReadDouble();
                    trackMetadata.Duration = new TimeSpan(0, 0, (int)durationSeconds);

                    // Get rpg_album, rpg_track, rpk_album, rpk_track
                    // We don't need it but might as well read it
                    var rpgAlbum = content.ReadSingle();
                    var rpgTrack = content.ReadSingle();
                    var rpkAlbum = content.ReadSingle();
                    var rpkTrack = content.ReadSingle();

                    // Get entry count
                    var entryCount = (int)content.ReadUInt32();
                    var primaryKeyCount = (int)content.ReadUInt32();
                    var secondaryKeyCount = (int)content.ReadUInt32();
                    var secondaryKeysOffset = (int)content.ReadUInt32();

                    var primaryPairs = new Dictionary<string, string>(primaryKeyCount);

                    // Get primary keys
                    var primaryKeys = new Dictionary<uint, string>(primaryKeyCount);
                    for (var x = 0; x < primaryKeyCount; x++)
                    {
                        var id = content.ReadUInt32();
                        var nameOffset = content.ReadUInt32();
                        curPos = stream.Position;
                        stream.Seek(metaPos + nameOffset, SeekOrigin.Begin);
                        primaryKeys[id] = stream.ReadNullTerminatedString(Encoding.UTF8);
                        stream.Seek(curPos, SeekOrigin.Begin);
                    }

                    // Read 'unk0', no idea what it does
                    var unk0 = content.ReadUInt32();

                    // Get primary pair values
                    var previousPrimaryKey = primaryKeys.First().Value;
                    for (uint x = 0; x < primaryKeyCount; x++)
                    {
                        var valueOffset = content.ReadUInt32();
                        curPos = stream.Position;
                        stream.Seek(metaPos + valueOffset, SeekOrigin.Begin);
                        var value = stream.ReadNullTerminatedString(Encoding.UTF8);
                        stream.Seek(curPos, SeekOrigin.Begin);

                        if (primaryKeys.ContainsKey(x))
                            previousPrimaryKey = primaryKeys[x];

                        primaryPairs.Add(previousPrimaryKey, value);
                    }

                    // Get secondary pairs
                    var secondaryPairs = new Dictionary<string, string>(secondaryKeyCount);

                    if (secondaryPairs == null) return null;

                    for (var x = 0; x < secondaryKeyCount; x++)
                    {
                        // Read key
                        var keyOffset = content.ReadUInt32();
                        curPos = stream.Position;
                        stream.Seek(metaPos + keyOffset, SeekOrigin.Begin);
                        var key = stream.ReadNullTerminatedString(Encoding.UTF8);
                        stream.Seek(curPos, SeekOrigin.Begin);

                        // Read value
                        var valueOffset = content.ReadUInt32();
                        curPos = stream.Position;
                        stream.Seek(metaPos + valueOffset, SeekOrigin.Begin);
                        var value = stream.ReadNullTerminatedString(Encoding.UTF8);
                        stream.Seek(curPos, SeekOrigin.Begin);

                        secondaryPairs.Add(key, value);
                    }

                    // Check flag for 64 bits of padding
                    if ((flags & 0x04) == 1)
                        stream.Seek(64, SeekOrigin.Current);

                    // Populate TrackMetadata
                    if (primaryPairs.TryGetValue("title", out var title))
                        trackMetadata.Title = title;
                    if (primaryPairs.TryGetValue("discnumber", out var discNumStr))
                        trackMetadata.DiscNumber = uint.Parse(discNumStr);
                    if (primaryPairs.TryGetValue("tracknumber", out var trackNumStr))
                        trackMetadata.TrackNumber = uint.Parse(trackNumStr, CultureInfo.InvariantCulture);
                    if (primaryPairs.TryGetValue("genre", out var genre))
                        trackMetadata.Genres = genre.IntoList();

                    // Add the current track to the list
                    trackMetaData.Add(trackMetadata);
                }

                return metadata;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the PLS metadata from the given file.
        /// </summary>
        private async Task<PlaylistMetadata?> GetPlsMetadata(IFileData fileData)
        {
            using var stream = await fileData.GetStreamAsync();
            using var content = new StreamReader(stream);

            var metadata = new PlaylistMetadata();

            // Make sure the file is really a PLS file
            var firstLine = await content.ReadLineAsync();
            if (firstLine != "[playlist]")
                // Not a valid PLS playlist
                return null;

            var matches = new List<Match>();
            while (!content.EndOfStream)
            {
                var line = await content.ReadLineAsync();
                var match = Regex.Match(line, @"^(?<key>[A-Za-z]+)(?<idx>[0-9]*)=(?<val>.+)$", RegexOptions.Compiled);
                if (match.Success)
                    matches.Add(match);
            }

            var trackCountMatch = matches.First(m => m.Groups["key"].Value == "NumberOfEntries");

            var trackCount = uint.Parse(trackCountMatch.Groups["val"].Value, CultureInfo.InvariantCulture);

            matches.Remove(trackCountMatch);
            var tracksTable = new Dictionary<int, TrackMetadata>((int)trackCount);

            foreach (var match in matches)
            {
                var value = match.Groups["val"].Value;
                var indexStr = match.Groups["idx"]?.Value;
                if (int.TryParse(indexStr, out var index))
                {
                    if (!tracksTable.ContainsKey(index))
                        tracksTable[index] = new TrackMetadata();
                }

                switch (match.Groups["key"].Value)
                {
                    case "File":
                        tracksTable[index].Source = new Uri(ResolveFilePath(value, fileData));
                        break;

                    case "Length":
                        tracksTable[index].Duration = new TimeSpan(0, 0, int.Parse(value));
                        break;

                    case "Title":
                        tracksTable[index].Title = value;
                        break;
                }
            }

            // Collapse the tracks table to a plain list
            var tracks = tracksTable.Select(t => t.Value).PruneNull().ToList();

            return metadata;
        }

        private enum AimpplPlaylistMode
        {
            Summary,
            Settings,
            Content,
        }

        /// <summary>
        /// Gets the AIMPPL metadata from the given file.
        /// </summary>
        /// <remarks>Only tested with AIMPPL4 files.</remarks>
        private async Task<PlaylistMetadata?> GetAimpplMetadata(IFileData fileData)
        {
            // Adapted from https://github.com/ApexWeed/aimppl-copy/
            using var stream = await fileData.GetStreamAsync();
            using var content = new StreamReader(stream);

            var playlist = new PlaylistMetadata()
            {
                Id = fileData.Path.HashMD5Fast(),
            };

            var mode = AimpplPlaylistMode.Summary;
            while (!content.EndOfStream)
            {
                var line = await content.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                switch (line)
                {
                    case "#-----SUMMARY-----#":
                        mode = AimpplPlaylistMode.Summary;
                        continue;
                    case "#-----SETTINGS-----#":
                        mode = AimpplPlaylistMode.Settings;
                        continue;
                    case "#-----CONTENT-----#":
                        mode = AimpplPlaylistMode.Content;
                        continue;
                    default:
                        switch (mode)
                        {
                            case AimpplPlaylistMode.Summary:
                                {
                                    var split = line.IndexOf('=');
                                    var variable = line.Substring(0, split);
                                    var value = line.Substring(split + 1);
                                    if (variable == "Name")
                                        playlist.Title = value;

                                    break;
                                }

                            case AimpplPlaylistMode.Settings:
                                break;

                            case AimpplPlaylistMode.Content:
                                {
                                    if (string.IsNullOrWhiteSpace(line))
                                        continue;

                                    if (line.StartsWith("-", StringComparison.InvariantCulture))
                                        break;

                                    var trackComponents = line.Split('|');

                                    // checks if the track path is access-able. This is the fastest way. Not sure if its future proof.
                                    if (trackComponents.FirstOrDefault() != null && trackComponents[0].Contains(_rootFolder.Path))
                                    {
                                        playlist.Duration ??= default;

                                        if (int.TryParse(trackComponents.ElementAtOrDefault(14),
                                            out var trackDuration))
                                        {
                                            playlist.Duration =
                                                playlist.Duration?.Add(TimeSpan.FromMilliseconds(trackDuration));
                                        }

                                        var hash = TryGetHashFromExistingTrackRepo(new Uri(trackComponents[0]));

                                        if (hash != null)
                                        {
                                            playlist.TrackIds ??= new List<string>();

                                            playlist.TrackIds.Add(hash);
                                        }
                                    }

                                    break;
                                }

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                }
            }

            return playlist;
        }

        private async Task<FileMetadata?> ProcessPlaylistMetadata(IFileData file)
        {
            var playlistMetadata = await ScanPlaylistMetadata(file);

            return playlistMetadata == null ? null : new FileMetadata() { PlaylistMetadata = playlistMetadata };
        }

        private string? TryGetHashFromExistingTrackRepo(Uri path) => _scannedTracks.FirstOrDefault(c => c.TrackMetadata?.Source?.AbsoluteUri == path.AbsoluteUri)?.Id;

        private void ReleaseUnmanagedResources()
        {
            DetachEvents();
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsInitialized)
                return;

            if (disposing)
            {
                // dispose any objects you created here
                ReleaseUnmanagedResources();
                _scanningCancellationTokenSource.Cancel();
            }

            IsInitialized = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
