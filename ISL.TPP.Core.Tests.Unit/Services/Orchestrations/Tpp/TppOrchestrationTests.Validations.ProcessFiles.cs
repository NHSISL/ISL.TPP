// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Orchestrations.TPP;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldThrowValidationErrorOnNullConfigAndLogAsync()
        {
            // given
            this.tppOrchestrationService = new TppOrchestrationService(
                fileService: this.fileServiceMock.Object,
                documentService: this.documentServiceMock.Object,
                tppConfiguration: null,
                loggingBroker: this.loggingBrokerMock.Object);

            var invalidArgumentTppOrchestrationException =
                new InvalidArgumentTppOrchestrationException(
                    message: "Null configuration TPP orchestration exception, " +
                        "please correct the errors and try again.");

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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationErrorOnInvalidConfigAndLogAsync(string invalidText)
        {
            // given
            this.tppOrchestrationService = new TppOrchestrationService(
                fileService: this.fileServiceMock.Object,
                documentService: this.documentServiceMock.Object,

                tppConfiguration: new TppConfiguration
                {
                    TppManifestFile = invalidText,
                    TppPickupFolder = invalidText,
                    BlobStorageSettings = null
                },

                loggingBroker: this.loggingBrokerMock.Object);

            var invalidArgumentTppOrchestrationException =
                new InvalidArgumentTppOrchestrationException(
                    message: "Invalid TPP orchestration argument(s), please correct the errors and try again.");

            invalidArgumentTppOrchestrationException.AddData(
               key: "TppManifestFile",
               values: "Text is required");

            invalidArgumentTppOrchestrationException.AddData(
               key: "TppPickupFolder",
               values: "Text is required");

            invalidArgumentTppOrchestrationException.AddData(
               key: "BlobStorageSettings.AzureBlobContainer",
               values: "BlobStorageSettings.AzureBlobContainer is required");

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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationErrorOnInvalidConfigurationAndLogAsync(string invalidText)
        {
            // given
            this.tppOrchestrationService = new TppOrchestrationService(
                fileService: this.fileServiceMock.Object,
                documentService: this.documentServiceMock.Object,

                tppConfiguration: new TppConfiguration
                {
                    TppManifestFile = invalidText,
                    TppPickupFolder = invalidText,
                    BlobStorageSettings = new Models.Brokers.Storages.Blobs.BlobStorageSettings
                    {
                        AzureBlobContainer = invalidText
                    }
                },

                loggingBroker: this.loggingBrokerMock.Object);

            var invalidArgumentTppOrchestrationException =
                new InvalidArgumentTppOrchestrationException(
                    message: "Invalid TPP orchestration argument(s), please correct the errors and try again.");

            invalidArgumentTppOrchestrationException.AddData(
               key: "TppManifestFile",
               values: "Text is required");

            invalidArgumentTppOrchestrationException.AddData(
               key: "TppPickupFolder",
               values: "Text is required");

            invalidArgumentTppOrchestrationException.AddData(
               key: "BlobStorageSettings.AzureBlobContainer",
               values: "BlobStorageSettings.AzureBlobContainer is required");

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
