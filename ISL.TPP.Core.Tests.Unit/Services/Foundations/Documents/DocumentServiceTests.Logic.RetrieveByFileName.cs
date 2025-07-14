// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Text;
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
        public async Task ShouldRetrieveFileAsync()
        {
            // Given
            string inputFileName = GetRandomString();
            BlobStorageSettings randomBlobStorageSettings = GetRandomBlobStorageSettings();
            BlobStorageSettings inputBlobStorageSettings = randomBlobStorageSettings;
            string randomData = GetRandomString();
            string expectedData = randomData;
            Stream dataStream = new MemoryStream();
            Stream outputStream = new MemoryStream(Encoding.UTF8.GetBytes(randomData));

            this.blobStorageBrokerMock
                .Setup(broker => broker.SelectByFileNameAsync(dataStream, inputFileName, inputBlobStorageSettings))
                .Callback<Stream, string, BlobStorageSettings>((output, fileName, container) =>
                    {
                        output.Position = 0;
                        outputStream.CopyTo(output);
                    })
                .Returns(ValueTask.CompletedTask);

            // When
            await this.documentService.RetrieveDocumentByFileNameAsync(
                output: dataStream,
                fileName: inputFileName,
                blobStorageSettings: inputBlobStorageSettings);

            // Then
            string actualData = Encoding.UTF8.GetString(ReadAllBytesFromStream(dataStream));
            actualData.Should().BeEquivalentTo(expectedData);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.SelectByFileNameAsync(It.IsAny<Stream>(), inputFileName, inputBlobStorageSettings),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}