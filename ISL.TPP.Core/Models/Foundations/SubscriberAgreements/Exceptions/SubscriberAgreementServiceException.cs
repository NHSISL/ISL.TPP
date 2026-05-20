// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.SubscriberAgreements.Exceptions
{
    public class SubscriberAgreementServiceException : Xeption
    {
        public SubscriberAgreementServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
