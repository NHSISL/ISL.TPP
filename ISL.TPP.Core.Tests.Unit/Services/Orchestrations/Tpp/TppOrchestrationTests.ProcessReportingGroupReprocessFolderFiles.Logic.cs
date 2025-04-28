// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldProcessReportingGroupReprocessFolderFilesAsync()
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
