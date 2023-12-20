// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.Core.Models.Configurations
{
    public class TppConfiguration
    {
        public string TppPickupFolder { get; set; } = string.Empty;
        public string TppManifestFile { get; set; } = string.Empty;
        public BlobStorageSettings BlobStorageSettings { get; set; } = new BlobStorageSettings();
        public RetryConfig RetryConfig { get; set; } = new RetryConfig();
    }
}
