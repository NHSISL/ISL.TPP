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

            foreach (string reportingGroup in tppConfiguration.ReportingGroups)
            {
                var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);
                files.Add($@"{pickupFolder}\manifest.csv");
                //files.Add($@"{pickupFolder}\file1.csv");
                //files.Add($@"{pickupFolder}\file2.csv");
                expectedFiles.Add($@"{reportingGroup}\{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\manifest.csv");
                //expectedFiles.Add($@"{reportingGroup}\{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\file1.csv");
                //expectedFiles.Add($@"{reportingGroup}\{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\file2.csv");

                fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(
                    pickupFolder, "*"))
                    .ReturnsAsync(files);
            }

            foreach (string file in files)
            {
                fileBrokerMock.Setup(broker => broker.ReadFileAsync(file))
                    .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(file));

                fileBrokerMock.Setup(broker => broker.DeleteFileAsync(file))
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
            }

            foreach (string file in files)
            {
                fileBrokerMock.Verify(broker => broker.ReadFileAsync(file),
                    Times.Once);

                foreach (string reportingGroup in tppConfiguration.ReportingGroups)
                {
                    var pickupFolder = Path.Combine(tppConfiguration.TppPickupFolder, reportingGroup);

                    string filename =
                        $"{reportingGroup}\\{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\\" +
                        $"{file.Replace(pickupFolder, "")}";

                    filename = filename.Replace("\\\\", "\\");

                    blobStorageBrokerMock.Verify(broker =>
                        broker.UploadFileAsync(
                            filename,
                            ASCIIEncoding.UTF8.GetBytes(file),
                            tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                                Times.Once);

                    fileBrokerMock.Verify(broker =>
                        broker.DeleteFileAsync(file),
                            Times.Once);
                }
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
