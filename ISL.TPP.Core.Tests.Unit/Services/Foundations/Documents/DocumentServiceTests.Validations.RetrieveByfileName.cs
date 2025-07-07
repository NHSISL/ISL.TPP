// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
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
        public async Task ShouldThrowValidationExceptionOnSelectByFileNameIfInputsIsInvalid(string invalidInput)
        {
            // Given
            Stream invalidStream = null;
            string invalidFileName = invalidInput;
            BlobStorageSettings invalidBlobStorageSettings = null;

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Invalid document. Please correct the errors and try again.");

            invalidDocumentException.AddData(
                key: "Output",
                values: "Stream is required");

            invalidDocumentException.AddData(
                key: "FileName",
                values: "Text is required");

            invalidDocumentException.AddData(
                key: nameof(BlobStorageSettings),
                values: "Settings is required");

            var expectedDocumentValidationException = new DocumentValidationException(
                message: "Document validation errors occured, please try again",
                innerException: invalidDocumentException);

            // When
            ValueTask getDownloadLinkTask =
                documentService.RetrieveDocumentByFileNameAsync(
                    output: invalidStream,
                    fileName: invalidFileName,
                    blobStorageSettings: invalidBlobStorageSettings);

            DocumentValidationException actualDocumentBlobValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(getDownloadLinkTask.AsTask);

            // Then
            actualDocumentBlobValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnSelectByFileNameIfSettingsIsInvalid()
        {
            // Given
            Stream ouptutStream = new MemoryStream();
            string inputFileName = GetRandomString();
            BlobStorageSettings invalidBlobStorageSettings = new BlobStorageSettings();

            var invalidDocumentException =
                new InvalidDocumentException(
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

            var expectedDocumentValidationException = new DocumentValidationException(
                message: "Document validation errors occured, please try again",
                innerException: invalidDocumentException);

            // When
            ValueTask getDownloadLinkTask =
                documentService.RetrieveDocumentByFileNameAsync(
                    output: ouptutStream,
                    fileName: inputFileName,
                    blobStorageSettings: invalidBlobStorageSettings);

            DocumentValidationException actualDocumentBlobValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(getDownloadLinkTask.AsTask);

            // Then
            actualDocumentBlobValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByFileNameIfDocumentIsNotFoundAndLogItAsync()
        {
            //given
            BlobStorageSettings someBlobStorageSettings = GetRandomBlobStorageSettings();
            string someFileName = GetRandomString();
            Stream someStream = new MemoryStream();

            var notFoundDocumentException =
                new NotFoundDocumentException(message: $"Couldn't find documents with fileName: {someFileName}.");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: notFoundDocumentException);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.SelectByFileNameAsync(someStream, It.IsAny<string>(), It.IsAny<BlobStorageSettings>()))
                    .Returns(ValueTask.CompletedTask);

            //when
            ValueTask retrieveDocumentByIdTask =
                this.documentService.RetrieveDocumentByFileNameAsync(
                    output: someStream,
                    fileName: someFileName,
                    blobStorageSettings: someBlobStorageSettings);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(
                    retrieveDocumentByIdTask.AsTask);

            //then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobStorageSettings>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}