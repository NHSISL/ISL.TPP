// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal interface IBlobStorageBroker
    {
        ValueTask UploadFileAsync(string fileName, byte[] data, BlobStorageSettings blobStorageSettings);
        ValueTask<byte[]> DownloadByFileNameAsync(string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask DeleteFileAsync(string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask<string> GetDownloadLinkAsync(string fileName, BlobStorageSettings blobStorageSettings, DateTimeOffset expiresOn);
    }
}
