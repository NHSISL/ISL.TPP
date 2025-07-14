// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.Core.Clients.AzureBlobClients
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IAzureBlobClient
    {
        ValueTask UploadFileAsync(Stream input, string fileName, string container);
        ValueTask DownloadFileAsync(Stream output, string fileName, string container);
        ValueTask DeleteFileAsync(string fileName, string container);
        ValueTask<Uri> GetDownloadUriAsync(string fileName, string container, DateTimeOffset expiresOn);
    }
}