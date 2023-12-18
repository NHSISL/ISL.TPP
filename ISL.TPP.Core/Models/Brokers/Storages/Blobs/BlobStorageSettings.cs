// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace LHDS.Core.Models.Brokers.Storages.Blobs
{
    public class BlobStorageSettings
    {
        public string AzureBlobServiceUri { get; set; }
        public string AzureTenantId { get; set; }
        public string AzureBlobContainer { get; set; }
    }
}
