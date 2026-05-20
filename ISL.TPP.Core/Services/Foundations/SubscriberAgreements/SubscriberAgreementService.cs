// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class SubscriberAgreementService : ISubscriberAgreementService
    {
        private readonly IEnumerable<BlobStorageSettings> blobStoragesSettings;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public SubscriberAgreementService(
            IEnumerable<BlobStorageSettings> blobStoragesSettings,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.blobStoragesSettings = blobStoragesSettings;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<string>> GetActiveSubscriberAgreementsAsync() =>
            TryCatch(async () =>
            {
                List<string> activeSubscriberAgreements = this.blobStoragesSettings
                    .Where(c => c.Enabled)
                    .Select(c => c.Name)
                    .ToList();

                return activeSubscriberAgreements;
            });
    }
}
