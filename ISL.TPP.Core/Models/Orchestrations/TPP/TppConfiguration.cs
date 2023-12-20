// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Models.Orchestrations.TPP
{
    public class TppConfiguration
    {
        public BlobStorageSettings BlobStorageSettings { get; set; }
        public string TppPickupFolder { get; set; }
        public string TppManifestFile { get; set; }
    }
}
