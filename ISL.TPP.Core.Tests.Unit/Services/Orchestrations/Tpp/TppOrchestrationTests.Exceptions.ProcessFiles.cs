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

            var expectedDependencyException =
                new TppOrchestrationDependencyValidationException(
                    message: "TPP orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: dependancyValidationException.InnerException as Xeption);

            this.fileServiceMock.Setup(service =>
               service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"))
                   .ThrowsAsync(dependancyValidationException);

            // when
            ValueTask<List<string>> processTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationDependencyValidationException actualException =
                await Assert.ThrowsAsync<TppOrchestrationDependencyValidationException>(processTask.AsTask);

            // then
            actualException.Should()
                 .BeEquivalentTo(expectedDependencyException);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogError(It.Is(SameExceptionAs(
                   expectedDependencyException))),
                       Times.Once);

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

            var expectedDependencyException =
                new TppOrchestrationDependencyException(
                    message: "TPP orchestration dependency error occurred, fix the errors and try again.",
                    innerException: dependancyException.InnerException as Xeption);

            this.fileServiceMock.Setup(service =>
               service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"))
                   .ThrowsAsync(dependancyException);

            // when
            ValueTask<List<string>> processTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationDependencyException actualException =
                await Assert.ThrowsAsync<TppOrchestrationDependencyException>(processTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedDependencyException);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(It.IsAny<string>(), "*"),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogError(It.Is(SameExceptionAs(
                   expectedDependencyException))),
                       Times.Once);

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
