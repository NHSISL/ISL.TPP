// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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
            List<string> files = new List<string>
            {
                $@"{tppConfiguration.TppPickupFolder}\manifest.csv",
                $@"{tppConfiguration.TppPickupFolder}\file1.csv",
                $@"{tppConfiguration.TppPickupFolder}\file2.csv"
            };

            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            List<string> expectedFiles = files.Select(file =>
                $"{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}" +
                $"{file.Replace(this.tppConfiguration.TppPickupFolder, "")}").ToList();

            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(tppConfiguration.TppPickupFolder, "*"))
                .ReturnsAsync(files);

            foreach (string file in files)
            {
                fileBrokerMock.Setup(broker => broker.ReadFileAsync(file))
                    .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(file));
            }

            fileBrokerMock.Setup(broker => broker.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);


            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);

            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();
            Console.WriteLine($"actual files count: {actualFiles.Count}");
            Console.WriteLine($"expected files count: {expectedFiles.Count}");

            Console.WriteLine($"actual files: {string.Join(", ", actualFiles)}");
            Console.WriteLine($"expected files: {string.Join(", ", expectedFiles)}");

            // then
            actualFiles.Should().BeEquivalentTo(expectedFiles);

            fileBrokerMock.Verify(broker =>
                broker.GetListOfFilesAsync(tppConfiguration.TppPickupFolder, "*"),
                    Times.Once);

            foreach (string file in files)
            {
                fileBrokerMock.Verify(broker => broker.ReadFileAsync(file),
                    Times.Once);

                string filename =
                    $"{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}" +
                    $"{file.Replace(this.tppConfiguration.TppPickupFolder, "")}";

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

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotProcessNewFilesIfManifestFileNotPresentAsync()
        {
            // given
            List<string> files = new List<string>
            {
                $@"{tppConfiguration.TppPickupFolder}\file1.csv",
                $@"{tppConfiguration.TppPickupFolder}\file2.csv"
            };

            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<string> expectedFiles = new List<string>();

            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            Mock<IDateTimeBroker> dateTimeBrokerMock = new Mock<IDateTimeBroker>();

            dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            fileBrokerMock.Setup(broker => broker.GetListOfFilesAsync(tppConfiguration.TppPickupFolder, "*"))
                .ReturnsAsync(files);

            foreach (string file in files)
            {
                fileBrokerMock.Setup(broker => broker.ReadFileAsync(file))
                    .ReturnsAsync(ASCIIEncoding.UTF8.GetBytes(file));
            }

            fileBrokerMock.Setup(broker => broker.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);


            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => fileBrokerMock.Object);
            serviceCollection.AddTransient(_ => blobStorageBrokerMock.Object);
            serviceCollection.AddTransient(_ => dateTimeBrokerMock.Object);

            TppClient client = new TppClient(tppConfiguration, serviceCollection);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Should().BeEquivalentTo(expectedFiles);

            fileBrokerMock.Verify(broker =>
                broker.GetListOfFilesAsync(tppConfiguration.TppPickupFolder, "*"),
                    Times.Once);

            foreach (string file in files)
            {
                fileBrokerMock.Verify(broker => broker.ReadFileAsync(file),
                    Times.Never);

                blobStorageBrokerMock.Verify(broker =>
                    broker.UploadFileAsync(
                        file.Replace(tppConfiguration.TppPickupFolder, string.Empty),
                        ASCIIEncoding.UTF8.GetBytes(file),
                        tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                            Times.Never);

                fileBrokerMock.Verify(broker =>
                    broker.DeleteFileAsync(file),
                        Times.Never);
            }

            fileBrokerMock.VerifyNoOtherCalls();
            blobStorageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
