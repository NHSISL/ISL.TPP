// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    public class BlobStorageBroker : IBlobStorageBroker
    {
        private readonly BlobServiceClient blobServiceClient;

        public BlobStorageBroker(BlobStorageSettings blobStorageSettings)
        {
            this.blobServiceClient = SetupBlobServiceClient(blobStorageSettings);
        }

        public async ValueTask UploadFileAsync(string fileName, byte[] data, string container)
        {
            var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);

            using (MemoryStream stream = new MemoryStream(data))
            {

                var options = new BlobUploadOptions
                {
                    TransferOptions = new Azure.Storage.StorageTransferOptions()
                    {
                        InitialTransferSize = stream.Length
                    }
                };

                stream.Position = 0;
                await blobClient.UploadAsync(stream, options);
            }
        }

        public async ValueTask<byte[]> DownloadByFileNameAsync(string fileName, string container)
        {
            var blobClient = blobServiceClient
                .GetBlobContainerClient(container).GetBlobClient(fileName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        public async ValueTask DeleteFileAsync(string fileName, string container)
        {
            var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.None);
        }

        public async ValueTask<string> GetDownloadLinkAsync(string fileName, string container, DateTimeOffset expiresOn)
        {
            var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
            var userDelegationKey = blobServiceClient.GetUserDelegationKey(DateTimeOffset.UtcNow, expiresOn);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b", // b for blob, c for container
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = expiresOn,
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read); // read permissions

            // Add the SAS token to the container URI.
            var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
            {
                Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, blobServiceClient.AccountName)
            };

            return await Task.FromResult(blobUriBuilder.ToUri().ToString());
        }

        private static BlobServiceClient SetupBlobServiceClient(
            BlobStorageSettings blobStorageSettings)
        {
            var blobServiceClientOptions = new BlobClientOptions()
            {
                Transport = new HttpClientTransport(new HttpClient { Timeout = new TimeSpan(1, 0, 0) }),
                Retry = { NetworkTimeout = new TimeSpan(1, 0, 0) },
                EnableTenantDiscovery = true
            };

            var blobServiceClient = new BlobServiceClient(
                serviceUri: new Uri(blobStorageSettings.AzureBlobServiceUri),
                credential: new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions
                    {
                        VisualStudioTenantId = blobStorageSettings.AzureTenantId,
                    }),
                options: blobServiceClientOptions);
            return blobServiceClient;
        }
    }
}