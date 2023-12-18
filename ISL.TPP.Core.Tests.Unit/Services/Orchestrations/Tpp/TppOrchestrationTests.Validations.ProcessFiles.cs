// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using LHDS.Core.Models.Orchestrations.Decryptions.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldThrowValidationErrorOnMissingConfigAndLogAsyncAsync()
        {
            // given
            var invalidArgumentTppOrchestrationException =
                new InvalidArgumentTppOrchestrationException(
                    message: "Invalid TPP orchestration argument(s), please correct the errors and try again.");

            invalidArgumentTppOrchestrationException.AddData(
               key: "TppPickupFolder",
               values: "Text is required");

            invalidArgumentTppOrchestrationException.AddData(
               key: "AzureBlobContainer",
               values: "Text is required");

            var expectedTppOrchestrationFileNameValidationException =
                new TppOrchestrationValidationException(
                    message: "TPP orchestration validation errors occurred, please try again.",
                    innerException: invalidArgumentTppOrchestrationException);

            // when
            ValueTask<List<string>> ProcessTask = this.tppOrchestrationService.ProcessFilesAsync();

            TppOrchestrationValidationException actualException =
                await Assert.ThrowsAsync<TppOrchestrationValidationException>(ProcessTask.AsTask);

            // then
            actualException.Should()
                .BeEquivalentTo(expectedTppOrchestrationFileNameValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedTppOrchestrationFileNameValidationException))),
                        Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
