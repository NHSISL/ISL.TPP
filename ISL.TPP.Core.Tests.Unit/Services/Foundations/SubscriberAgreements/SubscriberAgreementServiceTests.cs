// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Services.Foundations.Documents;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.SubscriberAgreements
{
    public partial class SubscriberAgreementServiceTests
    {
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ISubscriberAgreementService subscriberAgreementService;
        private readonly List<BlobStorageSettings> blobStoragesSettings;

        public SubscriberAgreementServiceTests()
        {
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.blobStoragesSettings = new List<BlobStorageSettings>();

            this.subscriberAgreementService = new SubscriberAgreementService(
                blobStoragesSettings: this.blobStoragesSettings,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private BlobStorageSettings CreateBlobStorageSettings(bool enabled) =>
            new BlobStorageSettings
            {
                Name = GetRandomString(),
                AzureBlobServiceUri = GetRandomString(),
                AzureBlobContainer = GetRandomString(),
                AzureTenantId = GetRandomString(),
                AzureClientId = GetRandomString(),
                AzureClientSecret = GetRandomString(),
                Enabled = enabled
            };

        private ISubscriberAgreementService CreateServiceWithFaultingSettings(Exception exception)
        {
            var faultingEnumerableMock = new Mock<IEnumerable<BlobStorageSettings>>();

            faultingEnumerableMock
                .Setup(e => e.GetEnumerator())
                .Throws(exception);

            return new SubscriberAgreementService(
                blobStoragesSettings: faultingEnumerableMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }
    }
}
