// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Clients.AzureBlobClients;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal class BlobStorageBroker : IBlobStorageBroker
    {
        private readonly IAzureBlobClient azureBlobClient;

        public BlobStorageBroker(IAzureBlobClient azureBlobClient) =>
            this.azureBlobClient = azureBlobClient;

        public async ValueTask InsertFileAsync(Stream input, string fileName, string container) =>
            await azureBlobClient.UploadFileAsync(input, fileName, container);

        public async ValueTask SelectByFileNameAsync(Stream output, string fileName, string container) =>
            await azureBlobClient.DownloadFileAsync(output, fileName, container);

        public async ValueTask DeleteFileAsync(string fileName, string container) =>
            await azureBlobClient.DeleteFileAsync(fileName, container);

        public async ValueTask<string> GetDownloadLinkAsync(
            string fileName,
            string container,
            DateTimeOffset expiresOn)
        {
            Uri uri = await azureBlobClient.GetDownloadUriAsync(fileName, container, expiresOn);

            return uri.ToString();
        }
    }
}