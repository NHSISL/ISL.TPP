// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using Azure;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnDeleteFileAndLogItAsync()
        {
            // given
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
            string randomFileName = GetRandomString();
            var randomMessage = GetRandomString();

            Document randomDocument = new Document
            {
                FileName = randomFileName,
                DocumentData = Encoding.ASCII.GetBytes(GetRandomString())
            };

            var requestFailedException = new RequestFailedException(randomMessage);

            var failedDocumentRequestException =
                new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support",
                    innerException: requestFailedException);

            var expectedDependencyException =
                 new DocumentDependencyException(
                     message: "Document dependency error occurred, contact support.",
                     innerException: failedDocumentRequestException);

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.DeleteFileAsync(randomDocument.FileName, blobStorageSettings))
                    .Throws(requestFailedException);

            // when
            ValueTask getDocumentTask = this.documentService
                .RemoveDocumentByFileNameAsync(randomFileName, blobStorageSettings);

            var actualDependencyException =
                 await Assert.ThrowsAsync<DocumentDependencyException>(getDocumentTask.AsTask);

            // then
            actualDependencyException.Should().BeEquivalentTo(expectedDependencyException);

            this.blobStorageBrokerMock.Verify(broker =>
                 broker.DeleteFileAsync(randomDocument.FileName, blobStorageSettings),
                     Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogError(It.Is(SameExceptionAs(
                     expectedDependencyException))),
                         Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnDeleteFileIfServiceErrorOccursAndLogItAsync()
        {
            // given
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
            string randomFileName = GetRandomString();
            var randomMessage = GetRandomString();

            Document randomDocument = new Document
            {
                FileName = randomFileName,
                DocumentData = Encoding.ASCII.GetBytes(GetRandomString())
            };

            var serviceException = new Exception(randomMessage);

            var failedDocumentServiceException = new FailedDocumentServiceException(
                message: "Failed document service error occurred, contact support.",
                innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: failedDocumentServiceException);

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.DeleteFileAsync(randomDocument.FileName, blobStorageSettings))
                     .Throws(failedDocumentServiceException);

            // when
            ValueTask getDocumentTask =
                this.documentService.RemoveDocumentByFileNameAsync(randomDocument.FileName, blobStorageSettings);

            var actualServiceException =
                 await Assert.ThrowsAsync<DocumentServiceException>(getDocumentTask.AsTask);

            // then
            actualServiceException.Should().BeEquivalentTo(expectedDocumentServiceException);

            this.blobStorageBrokerMock.Verify(broker =>
                 broker.DeleteFileAsync(randomDocument.FileName, blobStorageSettings),
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
