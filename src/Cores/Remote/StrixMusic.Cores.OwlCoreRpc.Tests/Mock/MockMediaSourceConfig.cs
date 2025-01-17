﻿using StrixMusic.Sdk.MediaPlayback;
using System;
using System.IO;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Cores.OwlCoreRpc.Tests.Mock
{
    public class MockMediaSourceConfig : IMediaSourceConfig
    {
        public MockMediaSourceConfig(string id, ICoreTrack track)
        {
            Id = id;
            Track = track;
        }

        public ICoreTrack Track { get; set; }

        public string Id { get; set; }

        public Uri? MediaSourceUri { get; set; }

        public Stream? FileStreamSource { get; set; }

        public string? FileStreamContentType { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
