// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.SubscriberAgreements
{
    public partial class SubscriberAgreementServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetActiveSubscriberAgreementsIfServiceErrorOccursAsync()
        {
            // Given
            var serviceException = new Exception();

            var failedSubscriberAgreementServiceException =
                new FailedSubscriberAgreementServiceException(
                    message: "Failed subscriber agreement service error occurred, contact support.",
                    innerException: serviceException);

            var expectedSubscriberAgreementServiceException =
                new SubscriberAgreementServiceException(
                    message: "Subscriber agreement service error occurred, contact support.",
                    innerException: failedSubscriberAgreementServiceException);

            this.subscriberAgreementHttpBrokerMock
                .Setup(broker => broker.GetActiveSubscriberAgreementsAsync())
                .ThrowsAsync(serviceException);

            // When
            ValueTask<List<string>> getActiveSubscriberAgreementsTask =
                this.subscriberAgreementService.GetActiveSubscriberAgreementsAsync();

            SubscriberAgreementServiceException actualException =
                await Assert.ThrowsAsync<SubscriberAgreementServiceException>(
                    getActiveSubscriberAgreementsTask.AsTask);

            // Then
            actualException.Should().BeEquivalentTo(expectedSubscriberAgreementServiceException);

            this.subscriberAgreementHttpBrokerMock.Verify(broker =>
                broker.GetActiveSubscriberAgreementsAsync(),
                    Times.Once);

            this.subscriberAgreementHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}


