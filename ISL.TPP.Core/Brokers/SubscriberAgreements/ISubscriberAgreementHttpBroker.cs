// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements;

namespace ISL.TPP.Core.Brokers.SubscriberAgreements
{
    internal interface ISubscriberAgreementHttpBroker
    {
        ValueTask<List<SubscriberAgreement>> GetActiveSubscriberAgreementsAsync();
    }
}
