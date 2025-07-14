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
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Clients.AzureBlobClients;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Microsoft.Extensions.Logging;

namespace ISL.TPP.Core.Brokers.Storages.Blobs
{
    internal class BlobStorageBroker : IBlobStorageBroker
    {
        private readonly ILogger<LoggingBroker> logger;

        public BlobStorageBroker(ILogger<LoggingBroker> logger)
        {
            this.logger = logger;
        }

        public async ValueTask InsertFileAsync(
            Stream input,
            string fileName,
            BlobStorageSettings blobStorageSettings)
        {
            IAzureBlobClient client = SetupClient(blobStorageSettings, logger);
            await client.UploadFileAsync(input, fileName, blobStorageSettings.AzureBlobContainer);
        }

        public async ValueTask SelectByFileNameAsync(
            Stream output,
            string fileName,
            BlobStorageSettings blobStorageSettings)
        {
            IAzureBlobClient client = SetupClient(blobStorageSettings, logger);
            await client.DownloadFileAsync(output, fileName, blobStorageSettings.AzureBlobContainer);

        }

        public async ValueTask DeleteFileAsync(string fileName, BlobStorageSettings blobStorageSettings)
        {
            IAzureBlobClient client = SetupClient(blobStorageSettings, logger);
            await client.DeleteFileAsync(fileName, blobStorageSettings.AzureBlobContainer);
        }

        public async ValueTask<string> GetDownloadLinkAsync(
            string fileName,
            BlobStorageSettings blobStorageSettings,
            DateTimeOffset expiresOn)
        {
            IAzureBlobClient client = SetupClient(blobStorageSettings, logger);
            Uri uri = await client.GetDownloadUriAsync(fileName, blobStorageSettings.AzureBlobContainer, expiresOn);

            return uri.ToString();
        }

        private static IAzureBlobClient SetupClient(
            BlobStorageSettings blobStorageSettings,
            ILogger<LoggingBroker> logger)
        {
            var blobServiceClientOptions = new BlobClientOptions()
            {
                Transport = new HttpClientTransport(new HttpClient { Timeout = new TimeSpan(1, 0, 0) }),
                Retry = { NetworkTimeout = new TimeSpan(1, 0, 0) },
                EnableTenantDiscovery = true
            };

            var client = new BlobServiceClient(
                serviceUri: new Uri(blobStorageSettings.AzureBlobServiceUri),
                credential: new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions
                    {
                        VisualStudioTenantId = blobStorageSettings.AzureTenantId,
                    }),
                options: blobServiceClientOptions);

            ILoggingBroker loggingBroker = new LoggingBroker(logger);

            IAzureBlobClient azureBlobClient = new AzureBlobClient(
                loggingBroker: loggingBroker,
                defaultClient: client);

            return azureBlobClient;
        }
    }
}