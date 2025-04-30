// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Azure;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetDownloadLinkAndLogItAsync()
        {
            // given
            BlobStorageSettings randomBlobStorageSettings = CreateRandomBlobStorageSettings();
            var randomString = GetRandomString();
            var requestFailedException = new RequestFailedException(randomString);

            var failedDocumentRequestException = new FailedDocumentRequestException(
                message: "Failed document request occurred, please contact support",
                innerException: requestFailedException);

            var expectedDependencyException =
                 new DocumentDependencyException(
                     message: "Document dependency error occurred, contact support.",
                     innerException: failedDocumentRequestException);

            this.dateTimeBrokerMock.Setup(broker =>
                 broker.GetCurrentDateTimeOffset())
                    .Throws(requestFailedException);

            // when
            ValueTask<string> downloadLinkTask = this.documentService
                .GetDownloadLinkAsync(fileName: randomString, blobStorageSettings: randomBlobStorageSettings);

            var actualDependencyException =
                 await Assert.ThrowsAsync<DocumentDependencyException>(downloadLinkTask.AsTask);

            // then
            actualDependencyException.Should().BeEquivalentTo(expectedDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                 broker.GetCurrentDateTimeOffset(),
                     Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                 broker.LogError(It.Is(SameExceptionAs(
                     expectedDependencyException))),
                         Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetDownloadLinkIfServiceErrorOccursAndLogItAsync()
        {
            // given
            BlobStorageSettings randomBlobStorageSettings = CreateRandomBlobStorageSettings();
            var randomString = GetRandomString();
            var serviceException = new Exception(randomString);

            var failedDocumentServiceException = new FailedDocumentServiceException(
                message: "Failed document service error occurred, contact support.",
                innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: failedDocumentServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                 broker.GetCurrentDateTimeOffset())
                    .Throws(serviceException);

            // when
            ValueTask<string> downloadLinkTask = this.documentService
                .GetDownloadLinkAsync(fileName: randomString, blobStorageSettings: randomBlobStorageSettings);

            var actualServiceException =
                 await Assert.ThrowsAsync<DocumentServiceException>(downloadLinkTask.AsTask);

            // then
            actualServiceException.Should().BeEquivalentTo(expectedDocumentServiceException);

            this.dateTimeBrokerMock.Verify(broker =>
                 broker.GetCurrentDateTimeOffset(),
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