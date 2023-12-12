// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    public class BlobStorageBroker : IBlobStorageBroker
    {
        public BlobStorageBroker()
        { }

        public ValueTask DeleteFileAsync(string fileName, string container) =>
            throw new NotImplementedException();

        public ValueTask<string> GetDownloadLinkAsync(string fileName, string container, DateTimeOffset expiresOn) =>
            throw new NotImplementedException();

        public ValueTask InsertFileAsync(string fileName, Stream stream, string container) =>
            throw new NotImplementedException();

        public ValueTask<byte[]> SelectByFileNameAsync(string fileName, string container) =>
            throw new NotImplementedException();
    }
}