// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.Core.Models.Configurations
{
    public class TppConfiguration
    {
        public string TppPickupFolder { get; set; } = string.Empty;
        public string TppSubmissionFolder { get; set; } = string.Empty;
        public string TppManifestFile { get; set; } = string.Empty;
        public int TimerIntervalInMinutes { get; set; } = 1;
        public List<BlobStorageSettings> BlobStoragesSettings { get; set; } = new List<BlobStorageSettings>();
        public TppWorkingFolders TppWorkingFolders { get; set; } = new TppWorkingFolders();
        public List<string> ReportingGroups { get; set; } = new List<string>();
        public RetryConfig RetryConfig { get; set; } = new RetryConfig();
    }
}
