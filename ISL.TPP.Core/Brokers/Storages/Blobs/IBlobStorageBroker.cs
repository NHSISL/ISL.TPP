// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal interface IBlobStorageBroker
    {
        ValueTask InsertFileAsync(Stream input, string fileName, string container);
        ValueTask SelectByFileNameAsync(Stream output, string fileName, string container);
        ValueTask DeleteFileAsync(string fileName, string container);
        ValueTask<string> GetDownloadLinkAsync(string fileName, string container, DateTimeOffset expiresOn);
    }
}
