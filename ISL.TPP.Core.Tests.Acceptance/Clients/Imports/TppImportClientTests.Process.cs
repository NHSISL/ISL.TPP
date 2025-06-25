// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Clients;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Acceptance.Clients.Imports
{
    public partial class TppImportClientTests
    {
        [Fact]
        public async Task ShouldProcessNewFilesIfManifestFileArePresentAsync()
        {
            // given
            List<string> files = new List<string>();
            List<string> reprocessFiles = new List<string>();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string manifestToDate = randomDateTimeOffset.ToString("yyyyMMdd_HHmm");
            StringBuilder csvManifest = new StringBuilder();
            csvManifest.AppendLine("FileName,IsDelta,IsReference,DateExtractFrom,DateExtractTo");
            csvManifest.AppendLine($"SRSystmOnline,Y,N,20231209_2245,{manifestToDate}");

            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string filePath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup);

                string reprocessFolderPath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                string reprocessSubFolderPath = Path.Combine(
                    reprocessFolderPath,
                    manifestToDate);

                string blobDestinationFolderPath = Path.Combine(
                    reportingGroup,
                    manifestToDate);

                List<string> reprocessFolders = new List<string> { reprocessSubFolderPath };

                files.Add($@"{filePath}\{tppConfiguration.TppManifestFile}");
                files.Add($@"{filePath}\file1.csv");
                files.Add($@"{filePath}\file2.csv");
                reprocessFiles.Add($@"{reprocessSubFolderPath}\{tppConfiguration.TppManifestFile}");
                reprocessFiles.Add($@"{reprocessSubFolderPath}\file1.csv");
                reprocessFiles.Add($@"{reprocessSubFolderPath}\file2.csv");

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    filePath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(files);

                fileBrokerMock.Setup(broker => broker.GetListOfSubFoldersAsync(
                    reprocessFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(reprocessFolders);

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    reprocessSubFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(reprocessFiles);
            }

            List<string> allFiles = [.. files, .. reprocessFiles];

            foreach (string file in allFiles)
            {
                if (file.EndsWith(tppConfiguration.TppManifestFile))
                {
                    fileBrokerMock.Setup(broker =>
                        broker.ReadFileAsync(file))
                            .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(csvManifest.ToString()));
                }
                else
                {
                    fileBrokerMock.Setup(broker =>
                        broker.ReadFileAsync(file))
                            .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(file));
                }
            }

            string someFolder = GetRandomString();

            fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()))
                    .ReturnsAsync(someFolder);

            fileBrokerMock.Setup(broker =>
                broker.GetListOfFilesAsync(someFolder, "*", It.IsAny<SearchOption>()))
                    .ReturnsAsync(reprocessFiles);

            fileBrokerMock.Setup(broker =>
                broker.CheckIfDirectoryExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(true);

            fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(true);

            fileBrokerMock.Setup(broker =>
                broker.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

            fileBrokerMock.Setup(broker =>
                broker.DeleteFileAsync(It.IsAny<string>()))
                    .ReturnsAsync(true);

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);
            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            await client.Imports.ProcessFilesAsync();

            // then
            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string filePath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup);

                string reprocessFolderPath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                string reprocessSubFolderPath = Path.Combine(
                    reprocessFolderPath,
                    manifestToDate);

                List<string> reprocessFolders = new List<string> { reprocessSubFolderPath };
            }

            foreach (string file in allFiles)
            {
                if (file.EndsWith(tppConfiguration.TppManifestFile))
                {
                    fileBrokerMock.Verify(broker =>
                        broker.ReadFileAsync(file),
                            Times.Exactly(2));
                }
                else
                {
                    fileBrokerMock.Verify(broker =>
                        broker.ReadFileAsync(file),
                            Times.Once);
                }
            }

            fileBrokerMock.Verify(broker => broker.GetListOfFilesAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()),
                    Times.AtLeastOnce);

            fileBrokerMock.Verify(broker => broker.GetListOfSubFoldersAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()),
                    Times.Once);

            fileBrokerMock.Verify(broker =>
                broker.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(allFiles.Count));

            fileBrokerMock.Verify(broker =>
                broker.DeleteFileAsync(It.IsAny<string>()),
                    Times.Exactly(allFiles.Count));

            fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            fileBrokerMock.Verify(broker =>
                broker.CheckIfDirectoryExistsAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            blobStorageBrokerMock.Verify(broker =>
                broker.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<BlobStorageSettings>()),
                    Times.Exactly(allFiles.Count));

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
            dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotProcessNewFilesIfManifestFileNotPresentAsync()
        {
            // given
            List<string> files = new List<string>();
            List<string> reprocessFiles = new List<string>();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string manifestToDate = randomDateTimeOffset.ToString("yyyyMMdd_HHmm");
            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string filePath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup);

                string reprocessFolderPath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                string reprocessSubFolderPath = Path.Combine(
                    reprocessFolderPath,
                    manifestToDate);

                List<string> reprocessFolders = new List<string> { reprocessSubFolderPath };

                files.Add($@"{filePath}\file1.csv");
                files.Add($@"{filePath}\file2.csv");
                reprocessFiles.Add($@"{reprocessSubFolderPath}\file1.csv");
                reprocessFiles.Add($@"{reprocessSubFolderPath}\file2.csv");

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    filePath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(files);

                fileBrokerMock.Setup(broker => broker.GetListOfSubFoldersAsync(
                    reprocessFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(reprocessFolders);

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    reprocessSubFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                        .ReturnsAsync(reprocessFiles);
            }

            fileBrokerMock.Setup(service =>
                service.CheckIfDirectoryExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(true);

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);
            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            await client.Imports.ProcessFilesAsync();

            // then
            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                string filePath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup);

                string reprocessFolderPath = Path.Combine(
                    tppConfiguration.TppPickupFolder,
                    reportingGroup,
                    tppConfiguration.TppWorkingFolders.ReProcess);

                string reprocessSubFolderPath = Path.Combine(
                    reprocessFolderPath,
                    manifestToDate);

                List<string> reprocessFolders = new List<string> { reprocessSubFolderPath };

                fileBrokerMock.Verify(broker => broker.GetListOfFilesAsync(
                    filePath, It.IsAny<string>(), It.IsAny<SearchOption>()),
                        Times.Once);

                fileBrokerMock.Verify(broker => broker.GetListOfSubFoldersAsync(
                    reprocessFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()),
                        Times.Once);

                fileBrokerMock.Verify(broker => broker.GetListOfFilesAsync(
                    reprocessSubFolderPath, It.IsAny<string>(), It.IsAny<SearchOption>()),
                        Times.Once);
            }

            fileBrokerMock.Verify(service =>
                service.CheckIfDirectoryExistsAsync(It.IsAny<string>()),
                    Times.Exactly(3));

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
            dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
