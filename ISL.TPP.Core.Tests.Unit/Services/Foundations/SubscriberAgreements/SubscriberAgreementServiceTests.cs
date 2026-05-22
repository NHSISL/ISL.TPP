// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.SubscriberAgreements;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements;
using ISL.TPP.Core.Services.Foundations.SubscriberAgreements;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.SubscriberAgreements
{
    public partial class SubscriberAgreementServiceTests
    {
        private readonly Mock<ISubscriberAgreementHttpBroker> subscriberAgreementHttpBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ISubscriberAgreementService subscriberAgreementService;

        public SubscriberAgreementServiceTests()
        {
            this.subscriberAgreementHttpBrokerMock = new Mock<ISubscriberAgreementHttpBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.subscriberAgreementService = new SubscriberAgreementService(
                subscriberAgreementHttpBroker: this.subscriberAgreementHttpBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 5).GetValue();

        private static List<SubscriberAgreement> CreateRandomSubscriberAgreements()
        {
            var agreements = new List<SubscriberAgreement>();

            for (int i = 0; i < GetRandomNumber(); i++)
            {
                agreements.Add(new SubscriberAgreement
                {
                    SupplierSharingAgreementShortName = GetRandomString()
                });
            }

            return agreements;
        }
    }
}
