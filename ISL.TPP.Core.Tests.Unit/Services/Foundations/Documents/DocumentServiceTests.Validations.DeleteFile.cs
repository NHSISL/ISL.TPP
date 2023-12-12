// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using ISL.TPP.Core.Services.Foundations.Documents;
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
        public async Task ShouldThrowValidationExceptionOnDeleteFileIfInputsIsInvalid(string invalidInput)
        {
            // Given
            string invalidContainer = invalidInput;
            string invalidFileName = invalidInput;
            string containerName = invalidInput;

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

            var documentService = new DocumentService(
                    blobStorageBroker: this.blobStorageBrokerMock.Object,
                    dateTimeBroker: this.dateTimeBrokerMock.Object,
                    loggingBroker: this.loggingBrokerMock.Object);

            // When
            ValueTask deleteFileTask = documentService
                .RemoveDocumentByFileNameAsync(fileName: invalidFileName, invalidContainer);

            DocumentValidationException actualDocumentValidationException =
                await Assert.ThrowsAsync<DocumentValidationException>(deleteFileTask.AsTask);

            // Then
            actualDocumentValidationException.Should().BeEquivalentTo(expectedDocumentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
