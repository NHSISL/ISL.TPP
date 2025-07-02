// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnSelectFileAndLogItAsync()
        {
            // given
            string randomContainer = GetRandomString();
            string randomFileName = GetRandomString();
            var outputStream = new MemoryStream();
            string fileName = GetRandomString();
            var randomMessage = GetRandomString();
            var requestFailedException = new RequestFailedException(randomMessage);

            var failedDocumentRequestException =
                new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support.",
                    innerException: requestFailedException);

            var expectedDependencyException =
                 new DocumentDependencyException(
                     message: "Document dependency error occurred, please contact support.",
                     innerException: failedDocumentRequestException);

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(requestFailedException);

            // when
            ValueTask getDownloadFileTask =
                this.documentService.RetrieveDocumentByFileNameAsync(
                    output: outputStream,
                    fileName: fileName,
                    container: randomContainer);

            var actualDependencyException =
                 await Assert.ThrowsAsync<DocumentDependencyException>(getDownloadFileTask.AsTask);

            // then
            actualDependencyException.Should().BeEquivalentTo(expectedDependencyException);

            this.blobStorageBrokerMock.Verify(broker =>
                 broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
                     Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogErrorAsync(It.Is(SameExceptionAs(
                     expectedDependencyException))),
                         Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveFileIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var randomContainer = GetRandomString();
            var randomFileName = GetRandomString();
            var outputStream = new MemoryStream();
            string fileName = GetRandomString();

            var randomMessage = GetRandomString();

            var serviceException = new Exception(randomMessage);

            var failedDocumentServiceException = new FailedDocumentServiceException(
                message: "Failed document service error occurred, please contact support.",
                innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, please contact support.",
                    innerException: failedDocumentServiceException);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                   .ThrowsAsync(serviceException);

            // when
            ValueTask getDownloadFileTask =
                this.documentService.RetrieveDocumentByFileNameAsync(
                    output: outputStream,
                    fileName: fileName,
                    container: randomContainer);

            var actualServiceException =
                 await Assert.ThrowsAsync<DocumentServiceException>(getDownloadFileTask.AsTask);

            // then
            actualServiceException.Should().BeEquivalentTo(expectedDocumentServiceException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.SelectByFileNameAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogErrorAsync(It.Is(SameExceptionAs(
                     expectedDocumentServiceException))),
                         Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}