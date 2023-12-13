// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
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
            var randomContainer = GetRandomString();
            string randomFileName = GetRandomString();

            Document randomDocument = new Document
            {
                FileName = randomFileName,
                DocumentData = Encoding.ASCII.GetBytes(GetRandomString()),
            };

            Document expectedDocument = randomDocument.DeepClone();
            expectedDocument.SHA256Hash = ComputeSHA256Hash(randomDocument.DocumentData);

            this.blobStorageBrokerMock.Setup(broker =>
                broker.SelectByFileNameAsync(randomDocument.FileName, randomContainer))
                    .ReturnsAsync(randomDocument.DocumentData);

            // When
            Document actualDocument =
                await this.documentService
                    .RetrieveDocumentByFileNameAsync(fileName: randomDocument.FileName, container: randomContainer);

            // Then
            actualDocument.Should().BeEquivalentTo(expectedDocument);

            this.blobStorageBrokerMock.Verify(broker =>
                broker.SelectByFileNameAsync(randomDocument.FileName, randomContainer),
                    Times.Once);

            this.blobStorageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}