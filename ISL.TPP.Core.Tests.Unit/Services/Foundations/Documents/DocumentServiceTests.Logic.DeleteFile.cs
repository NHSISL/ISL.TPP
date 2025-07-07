// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents;
public partial class DocumentServiceTests
{
    [Fact]
    public async Task ShouldDeleteFileAsync()
    {
        // Given
        BlobStorageSettings randomBlobStorageSettings = GetRandomBlobStorageSettings();
        string randomFileName = GetRandomString();

        // When
        await this.documentService.RemoveDocumentByFileNameAsync(
            fileName: randomFileName,
            blobStorageSettings: randomBlobStorageSettings);

        // Then
        this.blobStorageBrokerMock.Verify(broker =>
            broker.DeleteFileAsync(randomFileName, randomBlobStorageSettings),
                Times.Once);

        this.blobStorageBrokerMock.VerifyNoOtherCalls();
        this.loggingBrokerMock.VerifyNoOtherCalls();
    }
}
