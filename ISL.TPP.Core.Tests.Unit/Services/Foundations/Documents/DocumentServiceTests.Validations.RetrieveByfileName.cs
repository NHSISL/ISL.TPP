// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
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
            string invalidContainer = invalidInput;

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
                key: "Container",
                values: "Text is required");

            var expectedDocumentValidationException = new DocumentValidationException(
                message: "Document validation errors occured, please try again",
                innerException: invalidDocumentException);

            // When
            ValueTask getDownloadLinkTask =
                documentService.RetrieveDocumentByFileNameAsync(
                    output: invalidStream,
                    fileName: invalidFileName,
                    container: invalidContainer);

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
            string someContainer = GetRandomString();
            string someFileName = GetRandomString();
            Stream someStream = new MemoryStream();

            var notFoundDocumentException =
                new NotFoundDocumentException(message: $"Couldn't find documents with fileName: {someFileName}.");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: notFoundDocumentException);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.SelectByFileNameAsync(someStream, It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(ValueTask.CompletedTask);

            //when
            ValueTask retrieveDocumentByIdTask =
                this.documentService.RetrieveDocumentByFileNameAsync(
                    output: someStream,
                    fileName: someFileName,
                    container: someContainer);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(
                    retrieveDocumentByIdTask.AsTask);

            //then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
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