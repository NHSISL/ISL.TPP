// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using ISL.TPP.Core.Services.Foundations.Documents;
using KellermanSoftware.CompareNetObjects;
using Moq;
using Tynamix.ObjectFiller;

namespace ISL.TPP.Core.Tests.Acceptance.Clients.Imports
{
    public partial class TppImportClientTests
    {
        private readonly TppConfiguration tppConfiguration;
        private readonly List<string> reportingGroups;

        public TppImportClientTests()
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

        private Expression<Func<Stream, bool>> SameStreamAs(Stream expectedStream)
        {
            return actualStream =>
                IsSameStream(expectedStream, actualStream);
        }

        private static bool IsSameStream(Stream expectedStream, Stream actualStream)
        {
            byte[] expectedBytes = ReadAllBytesFromStream(expectedStream);
            byte[] actualBytes = ReadAllBytesFromStream(actualStream);

            return new CompareLogic().Compare(expectedBytes, actualBytes).AreEqual;
        }

        private static byte[] ReadAllBytesFromStream(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();
    }
}
