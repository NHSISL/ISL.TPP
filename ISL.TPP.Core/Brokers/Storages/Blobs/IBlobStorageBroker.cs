// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    public interface IBlobStorageBroker
    {
        ValueTask UploadFileAsync(string fileName, byte[] data, string container);
        ValueTask<byte[]> DownloadByFileNameAsync(string fileName, string container);
        ValueTask DeleteFileAsync(string fileName, string container);
        ValueTask<string> GetDownloadLinkAsync(string fileName, string container, DateTimeOffset expiresOn);
    }
}
