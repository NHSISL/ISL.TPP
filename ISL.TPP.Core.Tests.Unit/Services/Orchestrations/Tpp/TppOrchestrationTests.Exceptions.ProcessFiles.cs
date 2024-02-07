// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using Moq;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Theory]
        [MemberData(nameof(TppDependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnDecryptIfDependencyValidationOccursAndLogItAsync(
            Xeption dependancyValidationException)
        {
            // given
            string randomFileName = GetRandomString();

            var dependencyValidationException =
                new TppOrchestrationDependencyValidationException(
                    message: "TPP orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: dependancyValidationException.InnerException as Xeption);

            List<Exception> exceptions = new List<Exception> { dependencyValidationException };

            var aggregateException =
                new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);

            var failedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: aggregateException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: failedTppOrchestrationServiceException);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"))
                    .ThrowsAsync(dependancyValidationException);

            // when
            ValueTask<List<string>> processTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationServiceException actualTppOrchestrationServiceException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualTppOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedTppOrchestrationServiceException);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is<Exception>(ex =>
                    IsSameExceptionAs(ex as Xeption).Invoke(dependencyValidationException) ||
                    IsSameExceptionAs(ex as Xeption).Invoke(expectedTppOrchestrationServiceException))),
                        Times.Exactly(2));

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(TppDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnDycryptIfDependencyExceptionOccursAndLogItAsync(
            Xeption dependancyException)
        {
            // given
            string randomFileName = GetRandomString();

            var dependencyException =
                new TppOrchestrationDependencyException(
                    message: "TPP orchestration dependency error occurred, fix the errors and try again.",
                    innerException: dependancyException.InnerException as Xeption);

            List<Exception> exceptions = new List<Exception> { dependencyException };

            var aggregateException =
                new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);

            var failedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: aggregateException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: failedTppOrchestrationServiceException);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"))
                    .ThrowsAsync(dependancyException);

            // when
            ValueTask<List<string>> processTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTppOrchestrationServiceException);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is<Exception>(ex =>
                    IsSameExceptionAs(ex as Xeption).Invoke(dependencyException) ||
                    IsSameExceptionAs(ex as Xeption).Invoke(expectedTppOrchestrationServiceException))),
                        Times.Exactly(2));

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnProcessIfServiceErrorOccursAndLogItAsync()
        {
            //Given
            string randomFileName = GetRandomString();
            var serviceException = new Exception();

            var failedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: serviceException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: failedTppOrchestrationServiceException);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<List<string>> processTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTppOrchestrationServiceException);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedTppOrchestrationServiceException))),
                        Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
