// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldNotProcessFilesIfManifestFileNotPresentAsync()
        {
            // given
            string pickupFolder = this.tppConfiguration.TppPickupFolder;
            string manifestFilePath = this.tppConfiguration.TppManifestFile;
            int numberOfFiles = GetRandomNumber();
            List<string> files = GetRandomFileList(numberOfFiles);
            List<string> expectedFiles = new List<string>();

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"))
                    .ReturnsAsync(files);

            // when
            List<string> actualFiles = await this.tppOrchestrationService.ProcessFilesAsync();

            // then
            expectedFiles.Should().BeEquivalentTo(actualFiles);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"),
                    Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldProcessFilesIfManifestFileNotPresentAsync()
        {
            // given
            string pickupFolder = this.tppConfiguration.TppPickupFolder;
            string manifestFile = this.tppConfiguration.TppManifestFile;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            int numberOfFiles = GetRandomNumber();
            List<string> files = GetRandomFileList(numberOfFiles);
            files.Add(manifestFile);
            List<byte[]> fileBytes = files.Select(file => Encoding.UTF8.GetBytes(file)).ToList();
            List<string> expectedFiles = new List<string>();

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"))
                    .ReturnsAsync(files);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            for (int i = 0; i < files.Count; i++)
            {
                this.fileServiceMock.Setup(service =>
                    service.ReadFromFileAsync(files[i]))
                        .ReturnsAsync(fileBytes[i]);

                this.fileServiceMock.Setup(service =>
                    service.DeleteFileAsync(files[i]))
                        .ReturnsAsync(true);
            }

            // when
            List<string> actualFiles = await this.tppOrchestrationService.ProcessFilesAsync();

            // then
            for (int i = 0; i < files.Count; i++)
            {
                this.fileServiceMock.Verify(service =>
                    service.ReadFromFileAsync(files[i]),
                        Times.Once);

                var fileName =
                    $"{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\\" +
                    $"{files[i].Replace(this.tppConfiguration.TppPickupFolder, "")}";

                var doc = new Document
                {
                    FileName = fileName,
                    DocumentData = fileBytes[i]
                };

                expectedFiles.Add(doc.FileName);

                this.documentServiceMock.Verify(service =>
                    service.AddDocumentAsync(
                        It.Is(SameDocumentAs(doc)),
                        this.tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                            Times.Once);

                this.fileServiceMock.Verify(service =>
                    service.DeleteFileAsync(files[i]),
                        Times.Once);
            }

            expectedFiles.Should().BeEquivalentTo(actualFiles);

            this.fileServiceMock.Verify(service =>
                service.RetrieveListOfFilesAsync(pickupFolder, "*"),
                    Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
