// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using ISL.TPP.Core.Models;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldCleanupFilesOnSuccessOrFailAsync(
            bool isSuccess)
        {
            // given
            string randomReportingGroup = GetRandomString();
            string randomReportingGroupFolder = $"{GetRandomString()}\\{randomReportingGroup}";
            string manifestFile = tppConfiguration.TppManifestFile;
            int count = 1; //GetRandomNumber();
            List<string> randomFiles = GetRandomStringList(count);
            randomFiles.Add(manifestFile);
            List<string> manifestFileLastList = randomFiles.DeepClone();
            string manifestDataString = GetRandomString();
            byte[] manifestData = Encoding.UTF8.GetBytes(manifestDataString);
            List<Manifest> manifestList = CreateRandomManifests(count);
            var manifestDateTime = manifestList.First().DateExtractTo;

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

            this.fileServiceMock.Setup(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder))
                    .ReturnsAsync(true);

            foreach (string filePath in manifestFileLastList)
            {
                this.fileServiceMock.Setup(service =>
                    service.CheckIfFileExistsAsync(filePath))
                        .ReturnsAsync(true);

                var moveDestinationFolder = $"{tppConfiguration.TppPickupFolder}\\{randomReportingGroup}" +
                    $"\\{(isSuccess
                        ? tppConfiguration.TppWorkingFolders.Processed
                        : tppConfiguration.TppWorkingFolders.Errored)}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                moveDestinationFolder = moveDestinationFolder.Replace("\\\\", "\\");

                this.fileServiceMock.Setup(service =>
                    service.CopyFileAsync(filePath, moveDestinationFolder))
                        .ReturnsAsync(true);

                this.fileServiceMock.Setup(service =>
                    service.DeleteFileAsync(filePath))
                        .ReturnsAsync(true);
            }

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.AllDirectories))
                    .ReturnsAsync(new List<string>());

            this.fileServiceMock.Setup(service =>
                service.DeleteDirectoryAsync(randomReportingGroupFolder, true))
                    .ReturnsAsync(true);

            // when
            await tppOrchestrationServiceMock.Object.CleanupFilesAsync(
                fileList: manifestFileLastList,
                reportingGroup: randomReportingGroup,
                reportingGroupFolder: randomReportingGroupFolder,
                manifestDateTime: manifestDateTime,
                allSuccessFull: isSuccess);

            // then
            this.fileServiceMock.Verify(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder),
                    Times.Exactly(2));

            foreach (string filePath in manifestFileLastList)
            {
                this.fileServiceMock.Verify(service =>
                    service.CheckIfFileExistsAsync(filePath),
                        Times.Once);

                var moveDestinationFolder = $"{tppConfiguration.TppPickupFolder}\\{randomReportingGroup}" +
                    $"\\{(isSuccess
                        ? tppConfiguration.TppWorkingFolders.Processed
                        : tppConfiguration.TppWorkingFolders.Errored)}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                moveDestinationFolder = moveDestinationFolder.Replace("\\\\", "\\");

                this.fileServiceMock.Verify(service =>
                    service.CopyFileAsync(filePath, moveDestinationFolder),
                        Times.Once);

                this.fileServiceMock.Verify(service =>
                    service.DeleteFileAsync(filePath),
                        Times.Once);
            }

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.AllDirectories),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.DeleteDirectoryAsync(randomReportingGroupFolder, true),
                    Times.Once);

            tppOrchestrationServiceMock.Verify(service =>
                service.CleanupFilesAsync(
                    manifestFileLastList,
                    randomReportingGroup,
                    randomReportingGroupFolder,
                    manifestDateTime,
                    isSuccess),
                        Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
