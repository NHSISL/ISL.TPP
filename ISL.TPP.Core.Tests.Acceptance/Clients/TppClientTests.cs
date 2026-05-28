// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using Tynamix.ObjectFiller;

namespace ISL.TPP.Core.Tests.Acceptance.Clients
{
    public partial class TppClientTests
    {
        private readonly TppConfiguration tppConfiguration;
        private readonly List<string> reportingGroups;

        public TppClientTests()
        {
            this.reportingGroups = new List<string> { "ReportingGroup1" };

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
                        AzureClientSecret = "fake-secret-for-testing-only",
                        AzureTenantId = "devtenantid",
                        AzureBlobContainer = "tpp"
                    }
                },
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();
    }
}
