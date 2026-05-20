// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.SubscriberAgreements
{
    public partial class SubscriberAgreementServiceTests
    {
        [Fact]
        public async Task ShouldReturnOnlyEnabledSubscriberAgreementsAsync()
        {
            // Given
            BlobStorageSettings enabledSettings1 = CreateBlobStorageSettings(enabled: true);
            BlobStorageSettings enabledSettings2 = CreateBlobStorageSettings(enabled: true);
            BlobStorageSettings disabledSettings = CreateBlobStorageSettings(enabled: false);

            this.blobStoragesSettings.AddRange(new[]
            {
                enabledSettings1,
                disabledSettings,
                enabledSettings2
            });

            List<string> expectedSubscriberAgreements = new List<string>
            {
                enabledSettings1.Name,
                enabledSettings2.Name
            };

            // When
            List<string> actualSubscriberAgreements =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualSubscriberAgreements.Should().BeEquivalentTo(expectedSubscriberAgreements);
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyListWhenNoEnabledSubscriberAgreementsAsync()
        {
            // Given
            BlobStorageSettings disabledSettings1 = CreateBlobStorageSettings(enabled: false);
            BlobStorageSettings disabledSettings2 = CreateBlobStorageSettings(enabled: false);

            this.blobStoragesSettings.AddRange(new[]
            {
                disabledSettings1,
                disabledSettings2
            });

            List<string> expectedSubscriberAgreements = new List<string>();

            // When
            List<string> actualSubscriberAgreements =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualSubscriberAgreements.Should().BeEquivalentTo(expectedSubscriberAgreements);
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnEmptyListWhenNoBlobStorageSettingsAsync()
        {
            // Given
            List<string> expectedSubscriberAgreements = new List<string>();

            // When
            List<string> actualSubscriberAgreements =
                await this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            // Then
            actualSubscriberAgreements.Should().BeEquivalentTo(expectedSubscriberAgreements);
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
