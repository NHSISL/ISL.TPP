// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.Core.Models.Brokers.Storages.Blobs
{
    public class BlobStorageSettings
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string AzureBlobServiceUri { get; set; }
        public string AzureTenantId { get; set; }
        public string AzureClientId { get; set; }
        public string AzureClientSecret { get; set; }
        public string AzureBlobContainer { get; set; }
    }
}
