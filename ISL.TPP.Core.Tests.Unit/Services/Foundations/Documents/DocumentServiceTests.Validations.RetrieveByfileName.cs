// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using ISL.TPP.Core.Services.Foundations.Documents;
using Microsoft.Extensions.Configuration;
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
            string invalidContainer = invalidInput;
            string containerName = invalidInput;

            Document document = new Document
            {
                FileName = invalidInput
            };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Invalid document. Please correct the errors and try again.");

            invalidDocumentException.AddData(
                key: "FileName",
                values: "Text is required");

            invalidDocumentException.AddData(
                key: "Container",
                values: "Text is required");

            var expectedDocumentValidationException
                = new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: invalidDocumentException);

            var appSettingsStub = new Dictionary<string, string> {
                {"blobContainerName", invalidInput}
            };

            var inMemoryConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettingsStub)
                .Build();

            var documentService = new DocumentService(
                    blobStorageBroker: this.blobStorageBrokerMock.Object,
                    dateTimeBroker: this.dateTimeBrokerMock.Object,
                    loggingBroker: this.loggingBrokerMock.Object);

            // When
            ValueTask<Document> getDownloadLinkTask =
                documentService.RetrieveDocumentByFileNameAsync(
                    fileName: document.FileName,
                    container: invalidContainer);

            DocumentValidationException actualDocumentBlobValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(getDownloadLinkTask.AsTask);

            // Then
            actualDocumentBlobValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.DownloadByFileNameAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRetrieveByFileNameIfDocumentIsNotFoundAndLogItAsync()
        {
            //given
            string someContainer = GetRandomString();
            string someFileName = GetRandomString();
            byte[] nullByte = null;

            var notFoundDocumentException =
                new NotFoundDocumentException(message: $"Couldn't find documents with fileName: {someFileName}.");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: notFoundDocumentException);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.DownloadByFileNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(nullByte);

            //when
            ValueTask<Document> retrieveDocumentByIdTask =
                this.documentService.RetrieveDocumentByFileNameAsync(
                    fileName: someFileName,
                    container: someContainer);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(
                    retrieveDocumentByIdTask.AsTask);

            //then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.DownloadByFileNameAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}