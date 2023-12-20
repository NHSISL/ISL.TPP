// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
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

            List<string> expectedFiles = files.DeepClone();

            Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();
            Mock<IBlobStorageBroker> blobStorageBrokerMock = new Mock<IBlobStorageBroker>();

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
                    Times.Once);

                blobStorageBrokerMock.Verify(broker =>
                    broker.UploadFileAsync(
                        file.Replace(tppConfiguration.TppPickupFolder, string.Empty),
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
    }
}
