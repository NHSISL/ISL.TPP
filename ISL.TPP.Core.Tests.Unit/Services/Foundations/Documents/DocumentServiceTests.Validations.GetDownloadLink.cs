// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnGetDownloadLinkIfFileNameIsInvalid(string invalidInput)
        {
            // Given
            BlobStorageSettings invalidBlobStorageSettings = null;
            string invalidFileName = invalidInput;
            string invalidContainer = invalidInput;

            var invalidDocumentException = new InvalidDocumentException(
                message: "Invalid document. Please correct the errors and try again.");

            invalidDocumentException.AddData(
                key: "FileName",
                values: "Text is required");

            invalidDocumentException.AddData(
                key: nameof(BlobStorageSettings),
                values: "Settings is required");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: invalidDocumentException);

            // When
            ValueTask<string> uploadFileTask = this.documentService
                .GetDownloadLinkAsync(invalidFileName, invalidBlobStorageSettings);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(uploadFileTask.AsTask);

            // Then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.Verify(broker =>
               broker.GetDownloadLinkAsync(invalidFileName, invalidBlobStorageSettings, It.IsAny<DateTimeOffset>()),
                   Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetDownloadLinkIfSettingsIsInvalid()
        {
            // Given
            BlobStorageSettings invalidBlobStorageSettings = new BlobStorageSettings();
            string inputFileName = GetRandomString();

            var invalidDocumentException = new InvalidDocumentException(
                message: "Invalid document. Please correct the errors and try again.");

            invalidDocumentException.AddData(
                key: nameof(BlobStorageSettings.AzureBlobServiceUri),
                values: "Text is required");

            invalidDocumentException.AddData(
                key: nameof(BlobStorageSettings.AzureTenantId),
                values: "Text is required");

            invalidDocumentException.AddData(
                key: nameof(BlobStorageSettings.AzureBlobContainer),
                values: "Text is required");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: invalidDocumentException);

            // When
            ValueTask<string> uploadFileTask = this.documentService
                .GetDownloadLinkAsync(inputFileName, invalidBlobStorageSettings);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(uploadFileTask.AsTask);

            // Then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.Verify(broker =>
               broker.GetDownloadLinkAsync(inputFileName, invalidBlobStorageSettings, It.IsAny<DateTimeOffset>()),
                   Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }
    }
}