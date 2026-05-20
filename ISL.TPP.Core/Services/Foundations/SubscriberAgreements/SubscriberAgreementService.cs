// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class SubscriberAgreementService : ISubscriberAgreementService
    {
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public SubscriberAgreementService(
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask<List<string>> GetActiveSubscriberAgreementsAsync() =>
            throw new System.NotImplementedException();
    }
}
