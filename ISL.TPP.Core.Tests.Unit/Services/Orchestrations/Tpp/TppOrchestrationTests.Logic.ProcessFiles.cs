// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldNotProcessFilesIfManifestFileNotPresentAsync()
        {
            // given
            string pickupFolder = this.tppConfiguration.TppPickupFolder;
            string manifestFilePath = this.tppConfiguration.TppManifestFile;
            int numberOfFiles = GetRandomNumber();
            List<string> files = GetRandomFileList(numberOfFiles);
            List<string> expectedFiles = new List<string>();

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"))
                    .ReturnsAsync(files);

            // when
            List<string> actualFiles = await this.tppOrchestrationService.ProcessFilesAsync();

            // then
            expectedFiles.Should().BeEquivalentTo(actualFiles);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"),
                    Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
