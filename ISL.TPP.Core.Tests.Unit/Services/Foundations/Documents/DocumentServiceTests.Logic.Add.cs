// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Text;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.Documents;
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
            string randomContainer = GetRandomString();
            string randomFileName = GetRandomString();

            Document randomDocument = new Document
            {
                FileName = randomFileName,
                DocumentData = Encoding.ASCII.GetBytes(GetRandomString())
            };

            var stream = new MemoryStream(randomDocument.DocumentData);

            // When
            await this.documentService.AddDocumentAsync(document: randomDocument, container: randomContainer);

            // Then
            this.blobStorageBrokerMock.Verify(broker =>
                broker.UploadFileAsync(randomDocument.FileName, It.IsAny<Stream>(), randomContainer),
                Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}