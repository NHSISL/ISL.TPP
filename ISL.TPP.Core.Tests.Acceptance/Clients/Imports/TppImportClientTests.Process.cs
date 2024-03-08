// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Clients;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Acceptance.Clients.Imports
{
    public partial class TppImportClientTests
    {
        [Fact]
        public async Task ShouldProcessNewFilesIfManifestFilePresentAsync()
        {
            // given
            List<string> files = new List<string>();
            List<string> expectedFiles = new List<string>();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            string manifestToDate = "20231211_1707";
            StringBuilder csvManifest = new StringBuilder();
            csvManifest.AppendLine("FileName,IsDelta,IsReference,DateExtractFrom,DateExtractTo");
            csvManifest.AppendLine($"SRSystmOnline,Y,N,20231209_2245,{manifestToDate}");
            List<string> reportingGroupFiles = new List<string> { "manifest.csv", "file1.csv", "file2.csv" };


            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                foreach (string file in reportingGroupFiles)
                {
                    var filPath = $@"{pickupFolder}\{file}";
                    var expectedPath = $@"{reportingGroup}\{manifestToDate}\{file}";
                    files.Add(filPath);
                    expectedFiles.Add(expectedPath);

                    if (file.EndsWith("manifest.csv"))
                    {
                        fileBrokerMock.Setup(broker => broker.ReadFileAsync(filPath))
                            .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(csvManifest.ToString()));
                    }
                    else
                    {
                        fileBrokerMock.Setup(broker => broker.ReadFileAsync(filPath))
                            .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(filPath));
                    }

                    var processedPath =
                        $"{pickupFolder}" +
                        $"\\Processed" +
                        $"\\{manifestToDate}" +
                        $"\\{file.Replace(pickupFolder, "")}";

                    processedPath = processedPath.Replace("\\\\", "\\");

                    fileBrokerMock.Setup(broker => broker.GetDirectoryAsync(processedPath))
                        .ReturnsAsync($@"{pickupFolder}\Processed\{manifestToDate}");

                    fileBrokerMock.Setup(broker => broker.CheckIfFileExistsAsync(processedPath))
                        .ReturnsAsync(false);

                    fileBrokerMock.Setup(broker => broker.MoveFileAsync(filPath, processedPath))
                        .ReturnsAsync(true);
                }

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    pickupFolder, "*"))
                    .ReturnsAsync(files);

                fileBrokerMock.Setup(broker => broker.CheckIfDirectoryExistsAsync(
                    $@"{pickupFolder}\Processed\{manifestToDate}"))
                        .ReturnsAsync(true);
            }

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);

            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Should().BeEquivalentTo(expectedFiles);

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                fileBrokerMock.Verify(broker =>
                    broker.GetListOfFilesAsync(pickupFolder, "*"),
                        Times.Once);

                foreach (string file in reportingGroupFiles)
                {
                    var filPath = $@"{pickupFolder}\{file}";

                    if (filPath.EndsWith("manifest.csv"))
                    {
                        fileBrokerMock.Verify(broker => broker.ReadFileAsync(filPath),
                            Times.Exactly(2));
                    }
                    else
                    {
                        fileBrokerMock.Verify(broker => broker.ReadFileAsync(filPath),
                            Times.Once);
                    }

                    string filename =
                        $"{reportingGroup}\\{manifestToDate}\\{file.Replace(pickupFolder, "")}";

                    filename = filename.Replace("\\\\", "\\");

                    blobStorageBrokerMock.Verify(broker =>
                        broker.UploadFileAsync(
                            filename,
                            It.IsAny<byte[]>(),
                            tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                                Times.Once);

                    var processedPath =
                        $"{pickupFolder}" +
                        $"\\Processed" +
                        $"\\{manifestToDate}" +
                        $"\\{file.Replace(pickupFolder, "")}";

                    processedPath = processedPath.Replace("\\\\", "\\");

                    fileBrokerMock.Verify(broker =>
                        broker.GetDirectoryAsync(processedPath),
                            Times.Once);

                    fileBrokerMock.Verify(broker =>
                        broker.CheckIfFileExistsAsync(processedPath),
                            Times.Once);

                    fileBrokerMock.Verify(broker =>
                        broker.MoveFileAsync(filPath, processedPath),
                            Times.Once);
                }

                fileBrokerMock.Verify(broker =>
                    broker.CheckIfDirectoryExistsAsync($@"{pickupFolder}\Processed\{manifestToDate}"),
                        Times.Exactly(reportingGroupFiles.Count));
            }

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotProcessNewFilesIfManifestFileNotPresentAsync()
        {
            // given
            List<string> files = new List<string>();

            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<string> expectedFiles = new List<string>();

            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                files.Add($@"{tppConfiguration.TppPickupFolder}\{reportingGroup}\file1.csv");
                files.Add($@"{tppConfiguration.TppPickupFolder}\{reportingGroup}\file2.csv");
                var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    pickupFolder, "*"))
                    .ReturnsAsync(files);
            }

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);

            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Should().BeEquivalentTo(expectedFiles);

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                fileBrokerMock.Verify(broker =>
                    broker.GetListOfFilesAsync(pickupFolder, "*"),
                        Times.Once);
            }

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
