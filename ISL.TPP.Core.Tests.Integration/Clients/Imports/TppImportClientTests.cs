// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using Microsoft.Extensions.Configuration;
using Tynamix.ObjectFiller;
using Xunit.Abstractions;

namespace ISL.TPP.Core.Tests.Integration.Clients.Imports
{
    public partial class TppImportClientTests
    {
        private readonly ITestOutputHelper output;
        private readonly TppConfiguration tppConfiguration;

        public TppImportClientTests()
        {
            var environmentName = "Development";

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("local.appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("TPP_");

            IConfiguration configuration = configurationBuilder.Build();

            var timerIntervalInMinutes = configuration.GetValue<int>("TimerIntervalInMinutes");
            var tppManifestFile = configuration.GetValue<string>("tppManifestFile");
            var tppPickupFolder = configuration.GetValue<string>("tppPickupFolder");
            var blobStoragesSettings = configuration.GetSection("blobStorages").Get<List<BlobStorageSettings>>() ?? [];
            var reportingGroups = configuration.GetSection("reportingGroups").Get<List<string>>() ?? [];

            this.tppConfiguration = new TppConfiguration
            {
                TppManifestFile = tppManifestFile,
                TppPickupFolder = tppPickupFolder,
                TimerIntervalInMinutes = timerIntervalInMinutes,
                BlobStoragesSettings = blobStoragesSettings,
                ReportingGroups = reportingGroups,
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };
        }

        private static string GetRandomString(int wordCount = 0)
        {
            if (wordCount < 1)
            {
                wordCount = GetRandomNumber();
            }

            return new MnemonicString(wordCount).GetValue();
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();
    }
}
