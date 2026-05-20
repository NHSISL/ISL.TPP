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
            string randomReportingGroup = GetRandomString();
            string inputReportingGroup = randomReportingGroup;
            string randomReportingGroupFolder = $"{GetRandomString()}\\{inputReportingGroup}";
            string inputReportingGroupFolder = randomReportingGroupFolder;
            List<string> randomFolderList = GetRandomStringList(GetRandomNumber());

            this.fileServiceMock.Setup(service =>
                service.CheckIfDirectoryExistsAsync(inputReportingGroupFolder))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(inputReportingGroupFolder, "*", SearchOption.TopDirectoryOnly))
                    .ReturnsAsync(randomFolderList);

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.subscriberAgreementServiceMock.Object,
                this.csvMapperServiceMock.Object,
                this.tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(inputReportingGroupFolder, "*", SearchOption.TopDirectoryOnly))
                    .ReturnsAsync(randomFolderList);

            foreach (string folder in randomFolderList)
            {
                tppOrchestrationServiceMock.Setup(service =>
                    service.ProcessReportingGroupFilesAsync(folder, inputReportingGroup))
                        .Returns(ValueTask.CompletedTask);
            }

            // when
            await tppOrchestrationServiceMock.Object
                .ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder, inputReportingGroup);

            // then
            fileServiceMock.Verify(service =>
                service.CheckIfDirectoryExistsAsync(inputReportingGroupFolder),
                    Times.Once);

            fileServiceMock.Verify(service =>
                service.RetrieveListOfSubFoldersAsync(inputReportingGroupFolder, "*", SearchOption.TopDirectoryOnly),
                    Times.Once);

            foreach (string folder in randomFolderList)
            {
                tppOrchestrationServiceMock.Verify(service =>
                    service.ProcessReportingGroupFilesAsync(folder, inputReportingGroup),
                        Times.Once);
            }

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder, inputReportingGroup),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
