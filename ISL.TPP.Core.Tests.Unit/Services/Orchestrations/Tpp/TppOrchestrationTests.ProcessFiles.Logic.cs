// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldProcessFilesAsync()
        {
            // given
            int count = 1; //GetRandomNumber();
            TppConfiguration tppConfiguration = CreateRandomTppConfiguration(count);

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string folder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                string reprocessfolder = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                tppOrchestrationServiceMock.Setup(service =>
                    service.ProcessReportingGroupFilesAsync(folder, reportingGroup))
                        .Returns(ValueTask.CompletedTask);

                tppOrchestrationServiceMock.Setup(service =>
                    service.ProcessReportingGroupReprocessFolderFilesAsync(reprocessfolder, reportingGroup))
                        .Returns(ValueTask.CompletedTask);
            }

            // when
            await tppOrchestrationServiceMock.Object.ProcessFilesAsync();

            // then
            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string folder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                string reprocessfolder = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                tppOrchestrationServiceMock.Verify(service =>
                    service.ProcessReportingGroupFilesAsync(folder, reportingGroup),
                        Times.Once);

                tppOrchestrationServiceMock.Verify(service =>
                    service.ProcessReportingGroupReprocessFolderFilesAsync(reprocessfolder, reportingGroup),
                        Times.Once);
            }

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
