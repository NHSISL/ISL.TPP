// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.SubscriberAgreements
{
    public partial class SubscriberAgreementServiceTests
    {
        [Fact]
        public async Task ShouldReturnActiveSubscriberAgreementNamesAsync()
        {
            // Given
            List<SubscriberAgreement> randomAgreements = CreateRandomSubscriberAgreements();

            List<string> expectedNames = randomAgreements
                .Select(a => a.SupplierSharingAgreementShortName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            this.subscriberAgreementHttpBrokerMock
                .Setup(broker => broker.GetActiveSubscriberAgreementsAsync())
                .ReturnsAsync(randomAgreements);

            // When
            List<string> actualNames =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualNames.Should().BeEquivalentTo(expectedNames);

            this.subscriberAgreementHttpBrokerMock.Verify(broker =>
                broker.GetActiveSubscriberAgreementsAsync(),
                    Times.Once);

            this.subscriberAgreementHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeAgreementsWithNullOrEmptyNamesAsync()
        {
            // Given
            var agreements = new List<SubscriberAgreement>
            {
                new SubscriberAgreement { SupplierSharingAgreementShortName = GetRandomString() },
                new SubscriberAgreement { SupplierSharingAgreementShortName = string.Empty },
                new SubscriberAgreement { SupplierSharingAgreementShortName = null }
            };

            List<string> expectedNames = new List<string> { agreements[0].SupplierSharingAgreementShortName };

            this.subscriberAgreementHttpBrokerMock
                .Setup(broker => broker.GetActiveSubscriberAgreementsAsync())
                .ReturnsAsync(agreements);

            // When
            List<string> actualNames =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualNames.Should().BeEquivalentTo(expectedNames);

            this.subscriberAgreementHttpBrokerMock.Verify(broker =>
                broker.GetActiveSubscriberAgreementsAsync(),
                    Times.Once);

            this.subscriberAgreementHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyListWhenNoActiveAgreementsAsync()
        {
            // Given
            var emptyAgreements = new List<SubscriberAgreement>();
            var expectedNames = new List<string>();

            this.subscriberAgreementHttpBrokerMock
                .Setup(broker => broker.GetActiveSubscriberAgreementsAsync())
                .ReturnsAsync(emptyAgreements);

            // When
            List<string> actualNames =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualNames.Should().BeEquivalentTo(expectedNames);

            this.subscriberAgreementHttpBrokerMock.Verify(broker =>
                broker.GetActiveSubscriberAgreementsAsync(),
                    Times.Once);

            this.subscriberAgreementHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

