// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldNotProcessReportingGroupFilesWhenManifestDontExistAsync()
        {
            // given
            string randomReportingGroupFolder = GetRandomString();
            string manifestFile = tppConfiguration.TppManifestFile;
            List<string> randomFiles = GetRandomStringList(GetRandomNumber());

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*"))
                    .ReturnsAsync(randomFiles);

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

            // when
            await tppOrchestrationServiceMock.Object.ProcessReportingGroupFilesAsync(randomReportingGroupFolder);

            // then
            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*"),
                    Times.Once);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(randomReportingGroupFolder),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldProcessReportingGroupFilesWhenManifestExistAsync()
        {
            //// given
            //var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
            //    this.fileServiceMock.Object,
            //    this.documentServiceMock.Object,
            //    this.csvMapperServiceMock.Object,
            //    this.tppConfiguration,
            //    this.dateTimeBrokerMock.Object,
            //    this.loggingBrokerMock.Object)
            //{
            //    CallBase = true
            //};

            //foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            //{
            //    string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);

            //    tppOrchestrationServiceMock.Setup(service =>
            //        service.ProcessReportingGroupFilesAsync(pickupFolder))
            //            .Returns(ValueTask.CompletedTask);

            //    tppOrchestrationServiceMock.Setup(service =>
            //        service.ProcessReportingGroupReprocessFolderFilesAsync(pickupFolder))
            //            .Returns(ValueTask.CompletedTask);
            //}

            //// when
            //await this.tppOrchestrationService.ProcessFilesAsync();

            //// then
            //foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            //{
            //    string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);

            //    tppOrchestrationServiceMock.Verify(service =>
            //        service.ProcessReportingGroupFilesAsync(pickupFolder),
            //            Times.Once);

            //    tppOrchestrationServiceMock.Verify(service =>
            //        service.ProcessReportingGroupReprocessFolderFilesAsync(pickupFolder),
            //            Times.Once);
            //}

            //this.fileServiceMock.VerifyNoOtherCalls();
            //this.documentServiceMock.VerifyNoOtherCalls();
            //this.dateTimeBrokerMock.VerifyNoOtherCalls();
            //this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
