﻿using StrixMusic.Sdk.Models.Core;
using StrixMusic.Sdk.MediaPlayback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrixMusic.Sdk.Tests.Services.MediaPlayback
{
    public class MockMediaSourceConfig : IMediaSourceConfig
    {
        public ICoreTrack Track => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public Uri MediaSourceUri => throw new NotImplementedException();

        public Stream FileStreamSource => throw new NotImplementedException();

        public string FileStreamContentType => throw new NotImplementedException();

        public DateTime? ExpirationDate => throw new NotImplementedException();
    }
}
