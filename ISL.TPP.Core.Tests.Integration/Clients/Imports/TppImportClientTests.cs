// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.Core.Tests.Integration.Clients.Imports
{
    public partial class TppImportClientTests
    {
        private readonly TppConfiguration tppConfiguration;

        public TppImportClientTests()
        {
            tppConfiguration = new TppConfiguration
            {
                TppManifestFile = "manifest.csv",
                TppPickupFolder = @"c:\tpp\pickup",
                BlobStorageSettings = new BlobStorageSettings
                {
                    AzureBlobServiceUri = "https://localhost:10000/devclientaccount",
                    AzureClientId = "devclientaccount",
                    AzureClientSecret = "Eby8vdM02xSZFPTOtr/KBHBeksoGMGw==",
                    AzureTenantId = "devtenantid",
                    AzureBlobContainer = "tpp"
                },
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };
        }
    }
}
