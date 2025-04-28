// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal interface IDocumentService
    {
        ValueTask AddDocumentAsync(Document document, BlobStorageSettings blobStorageSettings);
        ValueTask<Document> RetrieveDocumentByFileNameAsync(string fileName, BlobStorageSettings blobStorageSettings);
        ValueTask RemoveDocumentByFileNameAsync(string filename, BlobStorageSettings blobStorageSettings);
        ValueTask<string> GetDownloadLinkAsync(string fileName, BlobStorageSettings blobStorageSettings);
    }
}
