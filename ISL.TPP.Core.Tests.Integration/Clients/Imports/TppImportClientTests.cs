// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using Microsoft.Extensions.Configuration;
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

            var tppManifestFile = configuration.GetValue<string>("tppManifestFile");
            var tppPickupFolder = configuration.GetValue<string>("tppPickupFolder");
            var blobStorageSettings = configuration.GetSection("blobStorage").Get<BlobStorageSettings>();

            this.tppConfiguration = new TppConfiguration
            {
                TppManifestFile = tppManifestFile,
                TppPickupFolder = tppPickupFolder,
                BlobStorageSettings = blobStorageSettings,
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };
        }
    }
}
