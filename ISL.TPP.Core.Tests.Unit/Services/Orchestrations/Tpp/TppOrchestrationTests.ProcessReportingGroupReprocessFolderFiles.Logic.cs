// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldProcessReportingGroupReprocessFolderFilesAsync()
        {
            // given
            string randomReportingGroupFolder = GetRandomString();
            string inputReportingGroupFolder = randomReportingGroupFolder;
            List<string> randomFolderList = GetRandomStringList(GetRandomNumber());

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(inputReportingGroupFolder, "*"))
                    .ReturnsAsync(randomFolderList);

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                this.tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            string pickupFolder =
                Path.Combine(inputReportingGroupFolder, tppConfiguration.TppWorkingFolders.ReProcess);

            fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(pickupFolder, "*"))
                    .ReturnsAsync(randomFolderList);

            foreach (string folder in randomFolderList)
            {
                tppOrchestrationServiceMock.Setup(service =>
                    service.ProcessReportingGroupFilesAsync(folder))
                        .Returns(ValueTask.CompletedTask);
            }

            // when
            await tppOrchestrationServiceMock.Object
                .ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder);

            // then
            fileServiceMock.Verify(service =>
                service.RetrieveListOfSubFoldersAsync(pickupFolder, "*"),
                    Times.Once);

            foreach (string folder in randomFolderList)
            {
                tppOrchestrationServiceMock.Verify(service =>
                    service.ProcessReportingGroupFilesAsync(folder),
                        Times.Once);
            }

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
