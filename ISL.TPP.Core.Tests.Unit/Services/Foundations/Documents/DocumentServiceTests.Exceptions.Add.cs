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
        public async Task ShouldThrowDependencyExceptionOnUploadFileAndLogItAsync()
        {
            // given
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
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

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.UploadFileAsync(document.FileName, document.DocumentData, blobStorageSettings))
                    .Throws(requestFailedException);

            // when
            ValueTask uploadFileTask = this.documentService.AddDocumentAsync(document, blobStorageSettings);

            var actualDependencyException =
                 await Assert.ThrowsAsync<DocumentDependencyException>(uploadFileTask.AsTask);

            // then
            actualDependencyException.Should().BeEquivalentTo(expectedDependencyException);

            this.blobStorageBrokerMock.Verify(broker =>
                 broker.UploadFileAsync(document.FileName, document.DocumentData, blobStorageSettings),
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
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
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

            this.blobStorageBrokerMock.Setup(broker =>
                 broker.UploadFileAsync(document.FileName, document.DocumentData, blobStorageSettings))
                     .Throws(failedDocumentServiceException);

            // when
            ValueTask uploadFileTask = this.documentService.AddDocumentAsync(document, blobStorageSettings);

            var actualServiceException =
                 await Assert.ThrowsAsync<DocumentServiceException>(uploadFileTask.AsTask);

            // then
            actualServiceException.Should().BeEquivalentTo(expectedDocumentServiceException);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.UploadFileAsync(document.FileName, document.DocumentData, blobStorageSettings),
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