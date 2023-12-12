// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnUploadFileAndLogItAsync()
        {
            // given
            string encryptedFileContainer = "emislanding";
            var randomString = GetRandomString();
            var randomBytes = Encoding.ASCII.GetBytes(GetRandomString());
            var randomMessage = GetRandomString();

            Document document = new Document
            {
                FileName = randomString,
                DocumentData = randomBytes
            };

            var requestFailedException = new RequestFailedException(randomMessage);

            var failedDocumentRequestException = new FailedDocumentRequestException(
                message: "Failed document request occurred, please contact support",
                innerException: requestFailedException);

            var expectedDependencyException =
                 new DocumentDependencyException(
                     message: "Document dependency error occurred, contact support.",
                     innerException: failedDocumentRequestException);

            var stream = new MemoryStream(document.DocumentData);

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.InsertFileAsync(document.FileName, It.IsAny<Stream>(), encryptedFileContainer))
                    .Throws(requestFailedException);

            // when
            ValueTask uploadFileTask = this.documentService.AddDocumentAsync(document, encryptedFileContainer);

            var actualDependencyException =
                 await Assert.ThrowsAsync<DocumentDependencyException>(uploadFileTask.AsTask);

            // then
            actualDependencyException.Should().BeEquivalentTo(expectedDependencyException);

            this.blobStorageBrokerMock.Verify(broker =>
                 broker.InsertFileAsync(document.FileName, It.IsAny<Stream>(), encryptedFileContainer),
                     Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogError(It.Is(SameExceptionAs(
                     expectedDependencyException))),
                         Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnUploadFileIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string encryptedFileContainer = "emislanding";
            var randomString = GetRandomString();
            var randomBytes = Encoding.ASCII.GetBytes(GetRandomString());
            var randomMessage = GetRandomString();

            Document document = new Document
            {
                FileName = randomString,
                DocumentData = randomBytes
            };

            var serviceException = new Exception(randomMessage);

            var failedDocumentServiceException = new FailedDocumentServiceException(
                message: "Failed document service error occurred, contact support.",
                innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: failedDocumentServiceException);

            var stream = new MemoryStream(document.DocumentData);

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.InsertFileAsync(document.FileName, It.IsAny<Stream>(), encryptedFileContainer))
                     .Throws(failedDocumentServiceException);

            // when
            ValueTask uploadFileTask = this.documentService.AddDocumentAsync(document, encryptedFileContainer);

            var actualServiceException =
                 await Assert.ThrowsAsync<DocumentServiceException>(uploadFileTask.AsTask);

            // then
            actualServiceException.Should().BeEquivalentTo(expectedDocumentServiceException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.InsertFileAsync(document.FileName, It.IsAny<Stream>(), encryptedFileContainer),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogError(It.Is(SameExceptionAs(
                     expectedDocumentServiceException))),
                         Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}