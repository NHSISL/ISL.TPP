// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.SubscriberAgreements.Exceptions
{
    public class FailedSubscriberAgreementServiceException : Xeption
    {
        public FailedSubscriberAgreementServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
