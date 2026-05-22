// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;

namespace ISL.TPP.Core.Models.Foundations.SubscriberAgreements
{
    public class SubscriberAgreement
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierSharingAgreementShortName { get; set; }
        public Guid? SupplierSharingAgreementGuid { get; set; }
        public string? FtpUserName { get; set; }
        public string? FtpPublicKey { get; set; }
        public string? GpgPublicKey { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? LastPollStartDate { get; set; }
        public DateTimeOffset? LastPollEndDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset UpdatedDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
