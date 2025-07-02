// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
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
            string tempFilePath = GetRandomString();
            string randomSource = GetRandomString();
            string randomDestination = GetRandomString();
            byte[] randomFileBytes = Encoding.UTF8.GetBytes(GetRandomString());
            bool expectedResult = true;
            MemoryStream randomStream = new MemoryStream(randomFileBytes);

            List<BlobStorageSettings> activeDestinations =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

            this.fileServiceMock.Setup(service =>
                service.GetTempFileNameAsync())
                    .ReturnsAsync(tempFilePath);

            this.fileServiceMock.Setup(service =>
                service.DeleteFileAsync(tempFilePath))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.CheckIfFileExistsAsync(tempFilePath))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.CheckIfFileExistsAsync(randomSource))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.ReadFromFileAsync(It.IsAny<Stream>(), randomSource))
                    .Returns(ValueTask.CompletedTask);

            foreach (BlobStorageSettings storageSettings in activeDestinations)
            {
                this.documentServiceMock.Setup(service =>
                    service.AddDocumentAsync(
                        It.Is(SameStreamAs(randomStream)),
                        randomDestination,
                        storageSettings.AzureBlobContainer))
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
                service.GetTempFileNameAsync(),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.DeleteFileAsync(tempFilePath),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CheckIfFileExistsAsync(tempFilePath),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CheckIfFileExistsAsync(randomSource),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.ReadFromFileAsync(It.IsAny<Stream>(), randomSource),
                    Times.Once);

            foreach (BlobStorageSettings blobStorageSettings in activeDestinations)
            {
                this.documentServiceMock.Verify(service =>
                    service.AddDocumentAsync(
                        It.IsAny<Stream>(),
                        randomDestination,
                        blobStorageSettings.AzureBlobContainer),
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
            string tempFilePath = GetRandomString();
            string randomSource = GetRandomString();
            string randomDestination = GetRandomString();
            byte[] randomFileBytes = Encoding.UTF8.GetBytes(GetRandomString());
            bool expectedResult = false;
            Exception someException = new Exception("Unable to add document");
            MemoryStream randomStream = new MemoryStream(randomFileBytes);

            List<BlobStorageSettings> activeDestinations =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

            this.fileServiceMock.Setup(service =>
                service.GetTempFileNameAsync())
                    .ReturnsAsync(tempFilePath);

            this.fileServiceMock.Setup(service =>
                service.DeleteFileAsync(tempFilePath))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.CheckIfFileExistsAsync(tempFilePath))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.CheckIfFileExistsAsync(randomSource))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.ReadFromFileAsync(It.IsAny<Stream>(), randomSource))
                    .Returns(ValueTask.CompletedTask);

            this.documentServiceMock.Setup(service =>
                service.AddDocumentAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(someException);

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
                service.GetTempFileNameAsync(),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CheckIfFileExistsAsync(randomSource),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CheckIfFileExistsAsync(tempFilePath),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.ReadFromFileAsync(It.IsAny<Stream>(), randomSource),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.DeleteFileAsync(tempFilePath),
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

                this.loggingBrokerMock.Verify(broker =>
                    broker.LogErrorAsync(It.Is(SameExceptionAs(
                        failedDocumentTppOrchestrationServiceException))),
                            Times.Once);
            }

            this.documentServiceMock.Verify(service =>
                service.AddDocumentAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(activeDestinations.Count));

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
