// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using LHDS.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Models.Orchestrations.TPP
{
    internal class TppConfiguration
    {
        public BlobStorageSettings BlobStorageSettings { get; set; }
        public string TppPickupFolder { get; set; }
        public string TppManifestFile { get; set; }
    }
}
