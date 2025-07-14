// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------


namespace ISL.TPP.Core.Clients.AzureBlobClients
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Sas;
    using ISL.TPP.Core.Brokers.Loggings;

    internal class AzureBlobClient : IAzureBlobClient
    {
        private readonly ILoggingBroker loggingBroker;
        private readonly BlobServiceClient blobServiceClient;

        public AzureBlobClient(
            ILoggingBroker loggingBroker,
            BlobServiceClient defaultClient)
        {
            this.loggingBroker = loggingBroker;
            blobServiceClient = defaultClient;
        }

        public async ValueTask DownloadFileAsync(Stream output, string fileName, string container)
        {
            await loggingBroker.LogInformationAsync(fileName);

            var blobClient = blobServiceClient
                .GetBlobContainerClient(container).GetBlobClient(fileName);

            await blobClient.DownloadToAsync(output);
        }

        public async ValueTask UploadFileAsync(Stream input, string fileName, string container)
        {
            try
            {
                byte[] contentHash;
                using (var md5 = MD5.Create())
                {
                    input.Position = 0;
                    contentHash = md5.ComputeHash(input);
                }

                await loggingBroker.LogInformationAsync($"file:{fileName}, size:{input.Length}, container:{container}");
                input.Position = 0;
                var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
                var streamLength = input.Length;

                var options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentHash = contentHash
                    },
                    ProgressHandler = new Progress<long>(progress =>
                    {
                        Console.WriteLine(
                            $"file: {fileName}, progress: {progress}/{streamLength}, " +
                            $"percent:{Math.Round(progress / (double)streamLength * 100.0, 2)}");
                    }),
                    TransferOptions = new Azure.Storage.StorageTransferOptions()
                    {
                        InitialTransferSize = input.Length
                    }
                };

                await blobClient.UploadAsync(input, options);
            }
            catch (Exception ex)
            {
                await loggingBroker.LogErrorAsync(ex);
                Console.WriteLine($"Unable to write blob: {fileName}");
                throw;
            }
        }

        public async ValueTask DeleteFileAsync(string fileName, string container)
        {
            await loggingBroker.LogInformationAsync(fileName);
            var blobClient = blobServiceClient.GetBlobContainerClient(container).GetBlobClient(fileName);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.None);
        }

        public async ValueTask<Uri> GetDownloadUriAsync(string fileName, string container, DateTimeOffset expiresOn)
        {
            await loggingBroker.LogInformationAsync(fileName);
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

            return blobUriBuilder.ToUri();
        }
    }
}