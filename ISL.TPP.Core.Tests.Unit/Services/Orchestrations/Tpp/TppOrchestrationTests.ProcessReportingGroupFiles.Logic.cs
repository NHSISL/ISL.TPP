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
        [Fact]
        public async Task ShouldNotProcessReportingGroupFilesWhenManifestDontExistAsync()
        {
            // given
            string randomReportingGroup = GetRandomString();
            string randomReportingGroupFolder = $"{GetRandomString()}\\{randomReportingGroup}";
            string manifestFile = tppConfiguration.TppManifestFile;
            List<string> randomFiles = GetRandomStringList(GetRandomNumber());

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.TopDirectoryOnly))
                    .ReturnsAsync(randomFiles);

            this.fileServiceMock.Setup(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder))
                    .ReturnsAsync(false);

            this.fileServiceMock.Setup(service =>
                service.CreateDirectoryAsync(randomReportingGroupFolder))
                    .ReturnsAsync(true);

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

            // when
            await tppOrchestrationServiceMock.Object
                .ProcessReportingGroupFilesAsync(randomReportingGroupFolder, randomReportingGroup);

            // then
            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.TopDirectoryOnly),
                    Times.Once);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(randomReportingGroupFolder, randomReportingGroup),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CreateDirectoryAsync(randomReportingGroupFolder),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldProcessReportingGroupFilesWhenManifestExistAndMoveToFolderOnSuccessOrFailAsync(
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
            bool hasHeaderRecord = true;
            List<Manifest> manifestList = CreateRandomManifests(count);
            var manifestDateTime = manifestList.First().DateExtractTo;

            this.fileServiceMock.Setup(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder))
                    .ReturnsAsync(false);

            this.fileServiceMock.Setup(service =>
                service.CreateDirectoryAsync(randomReportingGroupFolder))
                    .ReturnsAsync(true);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.TopDirectoryOnly))
                    .ReturnsAsync(randomFiles);

            this.fileServiceMock.Setup(service =>
                service.ReadFromFileAsync(manifestFile))
                    .ReturnsAsync(manifestData);

            this.csvMapperServiceMock.Setup(service =>
                service.MapCsvToObjectAsync<Manifest>(manifestDataString, hasHeaderRecord))
                    .ReturnsAsync(manifestList);

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

            foreach (string filePath in manifestFileLastList)
            {
                var blobDestinationFilePath =
                    $"{randomReportingGroup}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                blobDestinationFilePath = blobDestinationFilePath.Replace("\\\\", "\\");

                var moveDestinationFolder = $"{tppConfiguration.TppPickupFolder}\\{randomReportingGroup}" +
                    $"\\{(isSuccess
                        ? tppConfiguration.TppWorkingFolders.Processed
                        : tppConfiguration.TppWorkingFolders.Errored)}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                moveDestinationFolder = moveDestinationFolder.Replace("\\\\", "\\");

                tppOrchestrationServiceMock.Setup(service =>
                    service.WriteFileToDestinationAsync(filePath, blobDestinationFilePath))
                        .ReturnsAsync(isSuccess);
            }

            tppOrchestrationServiceMock.Setup(service =>
                service.CleanupFilesAsync(
                    manifestFileLastList,
                    randomReportingGroup,
                    randomReportingGroupFolder,
                    manifestDateTime,
                    isSuccess))
                .Returns(ValueTask.CompletedTask);

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(manifestFileLastList.Last(), "*", SearchOption.AllDirectories))
                    .ReturnsAsync(manifestFileLastList);

            // when
            await tppOrchestrationServiceMock.Object
                .ProcessReportingGroupFilesAsync(randomReportingGroupFolder, randomReportingGroup);

            // then
            this.fileServiceMock.Verify(service =>
                service.CheckIfDirectoryExistsAsync(randomReportingGroupFolder),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.CreateDirectoryAsync(randomReportingGroupFolder),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(randomReportingGroupFolder, "*", SearchOption.TopDirectoryOnly),
                    Times.Once);

            this.fileServiceMock.Verify(service =>
                service.ReadFromFileAsync(manifestFile),
                    Times.Once);

            this.csvMapperServiceMock.Verify(service =>
                service.MapCsvToObjectAsync<Manifest>(manifestDataString, hasHeaderRecord),
                    Times.Once);

            foreach (string filePath in manifestFileLastList)
            {
                var blobDestinationFilePath =
                    $"{randomReportingGroup}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                blobDestinationFilePath = blobDestinationFilePath.Replace("\\\\", "\\");

                var moveDestinationFolder = $"{tppConfiguration.TppPickupFolder}\\{randomReportingGroup}" +
                    $"\\{(isSuccess
                        ? tppConfiguration.TppWorkingFolders.Processed
                        : tppConfiguration.TppWorkingFolders.Errored)}" +
                    $"\\{manifestDateTime}" +
                    $"\\{filePath.Replace(randomReportingGroupFolder, "")}";

                moveDestinationFolder = moveDestinationFolder.Replace("\\\\", "\\");

                tppOrchestrationServiceMock.Verify(service =>
                    service.WriteFileToDestinationAsync(filePath, blobDestinationFilePath),
                        Times.Once);
            }

            tppOrchestrationServiceMock.Verify(service => service.CleanupFilesAsync(
                manifestFileLastList,
                randomReportingGroup,
                randomReportingGroupFolder,
                manifestDateTime,
                isSuccess),
                    Times.Once);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(randomReportingGroupFolder, randomReportingGroup),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
