// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents;
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
            BlobStorageSettings blobStorageSettings = CreateRandomBlobStorageSettings();
            string randomFileName = GetRandomString();

            Document randomDocument = new Document
            {
                FileName = randomFileName,
                DocumentData = Encoding.ASCII.GetBytes(GetRandomString()),
            };

            Document expectedDocument = randomDocument.DeepClone();
            expectedDocument.SHA256Hash = ComputeSHA256Hash(randomDocument.DocumentData);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.DownloadByFileNameAsync(randomDocument.FileName, blobStorageSettings))
                    .ReturnsAsync(randomDocument.DocumentData);

            // When
            Document actualDocument =
                await this.documentService
                    .RetrieveDocumentByFileNameAsync(fileName: randomDocument.FileName, blobStorageSettings);

            // Then
            actualDocument.Should().BeEquivalentTo(expectedDocument);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.DownloadByFileNameAsync(randomDocument.FileName, blobStorageSettings),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}