// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {

        [Fact]
        public async Task ShouldRetrieveDownloadlinkAsync()
        {
            // Given
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
            string randomFileName = GetRandomString();
            string inputFileName = randomFileName;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputExpireTime = randomDateTimeOffset.AddMinutes(5);
            string randomSasUrl = GetRandomString();
            string outputSasUrl = randomSasUrl;
            string expectedSasUrl = randomSasUrl;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.GetDownloadLinkAsync(inputFileName, blobStorageSettings, inputExpireTime))
                    .ReturnsAsync(outputSasUrl);

            // When
            string actualSasUrl =
                await this.documentService
                    .GetDownloadLinkAsync(inputFileName, blobStorageSettings);

            // Then
            actualSasUrl.Should().Be(expectedSasUrl);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Once());

            this.blobStorageBrokerMock.Verify(broker =>
                broker.GetDownloadLinkAsync(inputFileName, blobStorageSettings, inputExpireTime),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}