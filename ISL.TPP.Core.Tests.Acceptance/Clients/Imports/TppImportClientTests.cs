// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using Tynamix.ObjectFiller;

namespace ISL.TPP.Core.Tests.Acceptance.Clients.Imports
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
                BlobStoragesSettings = new List<BlobStorageSettings>()
                {
                    new BlobStorageSettings
                    {
                        Name = "development",
                        Enabled = true,
                        AzureBlobServiceUri = "https://localhost:10000/devclientaccount",
                        AzureClientId = "devclientaccount",
                        AzureClientSecret = "Eby8vdM02xSZFPTOtr/KBHBeksoGMGw==",
                        AzureTenantId = "devtenantid",
                        AzureBlobContainer = "tpp"
                    }
                },
                ReportingGroups = new List<string> { "ReportingGroup1" },
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };

        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();
    }
}
