// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.SubscriberAgreements;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class SubscriberAgreementService : ISubscriberAgreementService
    {
        private readonly ISubscriberAgreementHttpBroker subscriberAgreementHttpBroker;
        private readonly ILoggingBroker loggingBroker;

        public SubscriberAgreementService(
            ISubscriberAgreementHttpBroker subscriberAgreementHttpBroker,
            ILoggingBroker loggingBroker)
        {
            this.subscriberAgreementHttpBroker = subscriberAgreementHttpBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<string>> GetActiveSubscriberAgreementsAsync() =>
            TryCatch(async () =>
            {
                List<SubscriberAgreement> agreements =
                    await this.subscriberAgreementHttpBroker
                        .GetActiveSubscriberAgreementsAsync();

                return agreements
                    .Select(agreement => agreement.SupplierSharingAgreementShortName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();
            });
    }
}