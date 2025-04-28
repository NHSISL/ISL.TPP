// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldWriteFileToActiveDestinationsAsync()
        {
            // given
            TppConfiguration configuration = new TppConfiguration();

            string randomSource = GetRandomString();
            string randomDestination = GetRandomString();
            byte[] randomFileBytes = Encoding.UTF8.GetBytes(GetRandomString());
            bool expectedResult = true;

            Document destinationDocument = new Document
            {
                FileName = randomDestination,
                DocumentData = randomFileBytes
            };

            List<BlobStorageSettings> activeDestinations =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

            this.fileServiceMock.Setup(service =>
                service.ReadFromFileAsync(randomSource))
                    .ReturnsAsync(randomFileBytes);

            foreach (BlobStorageSettings storageSettings in activeDestinations)
            {
                this.documentServiceMock.Setup(service =>
                    service.AddDocumentAsync(It.Is(SameDocumentAs(destinationDocument)), storageSettings))
                        .Returns(ValueTask.CompletedTask);
            }

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                this.tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            // when
            bool actualResult = await tppOrchestrationServiceMock.Object
                .WriteFileToDestinationAsync(randomSource, randomDestination);

            // then
            actualResult.Should().Be(expectedResult);

            this.fileServiceMock.Verify(service =>
                service.ReadFromFileAsync(randomSource),
                    Times.Once);

            foreach (BlobStorageSettings blobStorageSettings in activeDestinations)
            {
                this.documentServiceMock.Verify(service =>
                    service.AddDocumentAsync(It.Is(SameDocumentAs(destinationDocument)), blobStorageSettings),
                        Times.Once);
            }

            tppOrchestrationServiceMock.Verify(service =>
                service.WriteFileToDestinationAsync(randomSource, randomDestination),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseIfUnableToWriteFileToActiveDestinationsAsync()
        {
            // given
            TppConfiguration configuration = new TppConfiguration();

            string randomSource = GetRandomString();
            string randomDestination = GetRandomString();
            byte[] randomFileBytes = Encoding.UTF8.GetBytes(GetRandomString());
            bool expectedResult = false;
            Exception someException = new Exception("Unable to add document");

            Document destinationDocument = new Document
            {
                FileName = randomDestination,
                DocumentData = randomFileBytes
            };

            List<BlobStorageSettings> activeDestinations =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

            this.fileServiceMock.Setup(service =>
                service.ReadFromFileAsync(randomSource))
                    .ReturnsAsync(randomFileBytes);

            foreach (BlobStorageSettings storageSettings in activeDestinations)
            {
                this.documentServiceMock.Setup(service =>
                    service.AddDocumentAsync(It.Is(SameDocumentAs(destinationDocument)), storageSettings))
                        .ThrowsAsync(someException);
            }

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                this.tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            // when
            bool actualResult = await tppOrchestrationServiceMock.Object
                .WriteFileToDestinationAsync(randomSource, randomDestination);

            // then
            actualResult.Should().Be(expectedResult);

            this.fileServiceMock.Verify(service =>
                service.ReadFromFileAsync(randomSource),
                    Times.Once);

            foreach (BlobStorageSettings blobStorageSettings in activeDestinations)
            {
                string message =
                    $"Unable to write file '{randomSource}' to destination '{randomDestination}' on " +
                    $"{blobStorageSettings.Name}";

                FailedDocumentTppOrchestrationServiceException failedDocumentTppOrchestrationServiceException =
                    new FailedDocumentTppOrchestrationServiceException(
                        message,
                        innerException: someException as Xeption);

                this.documentServiceMock.Verify(service =>
                    service.AddDocumentAsync(It.Is(SameDocumentAs(destinationDocument)), blobStorageSettings),
                        Times.Once);

                this.loggingBrokerMock.Verify(broker =>
                    broker.LogError(It.Is(SameExceptionAs(
                        failedDocumentTppOrchestrationServiceException))),
                            Times.Once);
            }

            tppOrchestrationServiceMock.Verify(service =>
                service.WriteFileToDestinationAsync(randomSource, randomDestination),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
