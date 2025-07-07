// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal interface IBlobStorageBroker
    {
        ValueTask InsertFileAsync(Stream input, string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask SelectByFileNameAsync(Stream output, string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask DeleteFileAsync(string fileName, BlobStorageSettings blobStorageSettings);

        ValueTask<string> GetDownloadLinkAsync(
            string fileName,
            BlobStorageSettings blobStorageSettings,
            DateTimeOffset expiresOn);
    }
}
