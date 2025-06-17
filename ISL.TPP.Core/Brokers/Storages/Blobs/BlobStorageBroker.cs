// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal class BlobStorageBroker : IBlobStorageBroker
    {
        public async ValueTask UploadFileAsync(string fileName, byte[] data, BlobStorageSettings blobStorageSettings)
        {
            BlobServiceClient blobServiceClient = SetupBlobServiceClient(blobStorageSettings);

            var blobClient = blobServiceClient.GetBlobContainerClient(blobStorageSettings.AzureBlobContainer)
                .GetBlobClient(fileName);

            using (MemoryStream stream = new MemoryStream(data))
            {
                byte[] contentHash;
                using (var md5 = MD5.Create())
                {
                    contentHash = md5.ComputeHash(data);
                }

                stream.Position = 0;

                var options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentHash = contentHash
                    },
                    TransferOptions = new Azure.Storage.StorageTransferOptions()
                    {
                        InitialTransferSize = stream.Length
                    }
                };

                await blobClient.UploadAsync(stream, options);
            }
        }

        public async ValueTask<byte[]> DownloadByFileNameAsync(string fileName, BlobStorageSettings blobStorageSettings)
        {
            BlobServiceClient blobServiceClient = SetupBlobServiceClient(blobStorageSettings);

            var blobClient = blobServiceClient
                .GetBlobContainerClient(blobStorageSettings.AzureBlobContainer).GetBlobClient(fileName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        public async ValueTask DeleteFileAsync(string fileName, BlobStorageSettings blobStorageSettings)
        {
            BlobServiceClient blobServiceClient = SetupBlobServiceClient(blobStorageSettings);

            var blobClient = blobServiceClient.GetBlobContainerClient(blobStorageSettings.AzureBlobContainer)
                .GetBlobClient(fileName);

            await blobClient.DeleteAsync(DeleteSnapshotsOption.None);
        }

        public async ValueTask<string> GetDownloadLinkAsync(string fileName, BlobStorageSettings blobStorageSettings, DateTimeOffset expiresOn)
        {
            BlobServiceClient blobServiceClient = SetupBlobServiceClient(blobStorageSettings);

            var blobClient = blobServiceClient.GetBlobContainerClient(blobStorageSettings.AzureBlobContainer)
                .GetBlobClient(fileName);

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