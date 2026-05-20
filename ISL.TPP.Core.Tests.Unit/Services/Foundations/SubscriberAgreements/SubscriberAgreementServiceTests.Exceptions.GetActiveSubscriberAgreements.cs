// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements.Exceptions;
using ISL.TPP.Core.Services.Foundations.Documents;
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

            ISubscriberAgreementService faultingService =
                CreateServiceWithFaultingSettings(serviceException);

            // When
            ValueTask<List<string>> getActiveSubscriberAgreementsTask =
                faultingService.GetActiveSubscriberAgreementsAsync();

            SubscriberAgreementServiceException actualException =
                await Assert.ThrowsAsync<SubscriberAgreementServiceException>(
                    getActiveSubscriberAgreementsTask.AsTask);

            // Then
            actualException.Should().BeEquivalentTo(expectedSubscriberAgreementServiceException);
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

