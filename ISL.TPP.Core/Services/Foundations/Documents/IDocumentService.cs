// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal interface IDocumentService
    {
        ValueTask AddDocumentAsync(Stream input, string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask RetrieveDocumentByFileNameAsync(Stream output, string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask RemoveDocumentByFileNameAsync(string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask<string> GetDownloadLinkAsync(string fileName, BlobStorageSettings blobStorageSettings);
    }
}
