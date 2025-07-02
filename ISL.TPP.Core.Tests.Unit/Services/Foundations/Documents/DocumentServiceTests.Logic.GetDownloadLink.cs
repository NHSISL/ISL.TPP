// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
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
            string encryptedFileContainer = "emislanding";
            string randomFileName = GetRandomString();
            string inputFileName = randomFileName;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset inputExpireTime = randomDateTimeOffset.AddMinutes(5);
            string randomSasUrl = GetRandomString();
            string outputSasUrl = randomSasUrl;
            string expectedSasUrl = randomSasUrl;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.GetDownloadLinkAsync(inputFileName, encryptedFileContainer, inputExpireTime))
                    .ReturnsAsync(outputSasUrl);

            // When
            string actualSasUrl =
                await this.documentService
                    .GetDownloadLinkAsync(inputFileName, encryptedFileContainer);

            // Then
            actualSasUrl.Should().Be(expectedSasUrl);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once());

            this.blobStorageBrokerMock.Verify(broker =>
                broker.GetDownloadLinkAsync(inputFileName, encryptedFileContainer, inputExpireTime),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}