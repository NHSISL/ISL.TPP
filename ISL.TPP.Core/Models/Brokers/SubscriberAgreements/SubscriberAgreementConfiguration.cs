// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ISL.TPP.Core.Models.Brokers.SubscriberAgreements
{
    public class SubscriberAgreementConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ActiveAgreementsRelativeUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public int MaxResponseContentBufferSizeInMegaBytes { get; set; } = 400;
        public int TimeoutInSeconds { get; set; } = 600;
        public List<Guid> SupplierIds { get; set; } = new List<Guid>();
    }
}