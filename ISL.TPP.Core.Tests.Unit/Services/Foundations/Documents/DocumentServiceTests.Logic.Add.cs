// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Text;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {

        [Fact]
        public async Task ShouldAddFileAsync()
        {
            // Given
            BlobStorageSettings randomBlobStorageSettings = GetRandomBlobStorageSettings();
            string randomFileName = GetRandomString();
            Stream randomStream = new MemoryStream(Encoding.UTF8.GetBytes(GetRandomString()));

            // When
            await this.documentService.AddDocumentAsync(
                input: randomStream,
                fileName: randomFileName,
                blobStorageSettings: randomBlobStorageSettings);

            // Then
            this.blobStorageBrokerMock.Verify(broker =>
                broker.InsertFileAsync(randomStream, randomFileName, randomBlobStorageSettings),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}