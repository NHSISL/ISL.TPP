// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.Documents;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents;
public partial class DocumentServiceTests
{
    [Fact]
    public async Task ShouldDeleteFileAsync()
    {
        // Given
        var randomContainer = GetRandomString();
        string randomFileName = GetRandomString();

        Document randomDocument = new Document
        {
            FileName = randomFileName,
            DocumentData = Encoding.ASCII.GetBytes(GetRandomString())
        };

        // When
        await this.documentService.RemoveDocumentByFileNameAsync(
            filename: randomDocument.FileName,
            container: randomContainer);

        // Then
        this.blobStorageBrokerMock.Verify(broker =>
            broker.DeleteFileAsync(randomDocument.FileName, randomContainer),
                Times.Once);

        this.blobStorageBrokerMock.VerifyNoOtherCalls();
        this.loggingBrokerMock.VerifyNoOtherCalls();
    }
}
